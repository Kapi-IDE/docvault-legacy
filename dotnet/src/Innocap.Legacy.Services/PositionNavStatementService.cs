using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Innocap.Legacy.Domain.Models;
using Innocap.Legacy.Infrastructure;
using Innocap.Legacy.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// ============================================================
// PositionNavStatementService
// ============================================================
// 2019-03: Created PositionService — Carlos M.
// 2019-06: Created NavService — Carlos M.
// 2020-11: Created StatementService — Carlos M.
// 2022-04: Minor refactor post-merger — Jean-Pierre L.
// 2023-07: Merged PositionService, NavService, and StatementService
//          into this file. Aarav said it was "cleaner this way".
//          Carlos would have killed him.
//          Jean-Pierre did not review this PR. He was on paternity leave.
// ============================================================
namespace Innocap.Legacy.Services
{
    public class PositionNavStatementService
    {
        private readonly InnocapDbContext _db;
        private readonly PositionRepository _positionRepo;
        private readonly NavRepository _navRepo;
        private readonly FeeCalculator _feeCalc;
        private readonly ILogger<PositionNavStatementService> _logger;

        // Injecting both EF context AND raw repos because Aarav just smashed it together
        public PositionNavStatementService(
            InnocapDbContext db,
            PositionRepository positionRepo,
            NavRepository navRepo,
            FeeCalculator feeCalc,
            ILogger<PositionNavStatementService> logger)
        {
            _db          = db;
            _positionRepo = positionRepo;
            _navRepo      = navRepo;
            _feeCalc      = feeCalc;
            _logger       = logger;
        }

        // ---- POSITIONS SECTION (originally PositionService) ----

        // Quebec naming convention from Carlos's original Montreal team
        public async Task<List<Position>> ChargerPositions(int investorId)
        {
            _logger.LogInformation("ChargerPositions called for investor {InvestorId}", investorId);

            var positions = (await _positionRepo.GetByInvestorAsync(investorId)).ToList();

            _logger.LogInformation(
                "Loaded {Count} positions for investor {InvestorId}. Total exposure: {TotalExposure}",
                positions.Count,
                investorId,
                positions.Sum(p => p.PositionSizeUsd));  // MNPI leaked to log — smell #10
                                                          // No comment — nobody noticed

            return positions;
        }

        // "CalculerTotalExposition" — yes, French. That's on purpose (brownfield smell #1).
        public async Task<double> CalculerTotalExposition(int investorId)
        {
            return await _positionRepo.GetTotalExposureByInvestorAsync(investorId);
        }

        public async Task<IEnumerable<Position>> GetPositionsByFund(string fundCode)
        {
            // This delegates to the SQL-injection-vulnerable method in PositionRepository
            // Nobody looked at what GetByFundCodeAsync actually does
            return await _positionRepo.GetByFundCodeAsync(fundCode);
        }

        // ---- NAV SECTION (originally NavService) ----

        // CalculerNAV — the French naming continues
        public async Task<NavStrike> CalculerNAV(int fundId, int shareClassId)
        {
            _logger.LogInformation("CalculerNAV: fundId={FundId}, shareClassId={ShareClassId}", fundId, shareClassId);

            var latestStrike = await _navRepo.GetLatestStrikeAsync(fundId, shareClassId);

            if (latestStrike == null)
            {
                _logger.LogWarning("No NAV strike found for fund {FundId}, share class {ShareClassId}", fundId, shareClassId);
                return null;
            }

            return latestStrike;
        }

        public async Task<int> RecordNavStrike(NavStrike strike)
        {
            // Basic validation — not enough, but it's what Carlos had
            if (strike.NavPerUnit <= 0)
                throw new ArgumentException("NAV per unit must be positive");
            if (strike.TotalNetAssets < 0)
                throw new ArgumentException("Total net assets cannot be negative");

            _logger.LogInformation(
                "Recording NAV strike for fund {FundId}: NAV={Nav}, TNA={Tna}",
                strike.FundId, strike.NavPerUnit, strike.TotalNetAssets);  // Logs NAV — borderline MNPI

            return await _navRepo.InsertStrikeAsync(strike);
        }

        // VerifierSolde — "Verify balance/holdings" in Quebec French
        public async Task<bool> VerifierSolde(int investorId, int fundId)
        {
            var positions = await _positionRepo.GetByInvestorAndFundAsync(investorId, fundId);
            return positions != null && positions.Any();
        }

        // ---- STATEMENT SECTION (originally StatementService) ----

        // GenererReleve — "Generate statement" in French. The centerpiece.
        public async Task<InvestorStatement> GenererReleve(int investorId, int fundId, DateTime periodStart, DateTime periodEnd)
        {
            _logger.LogInformation(
                "GenererReleve: investorId={InvestorId}, fundId={FundId}, period={Start} to {End}",
                investorId, fundId, periodStart, periodEnd);

            var investor = await _db.Investors.FindAsync(investorId);
            if (investor == null)
                throw new InvalidOperationException($"Investor {investorId} not found");

            var fund = await _db.Funds.Include(f => f.ShareClasses).FirstOrDefaultAsync(f => f.FundId == fundId);
            if (fund == null)
                throw new InvalidOperationException($"Fund {fundId} not found");

            // Just take the first share class — multi-class investors not handled properly
            // TODO: investor ↔ share class mapping (Carlos, 2019) — still not done
            var shareClass = fund.ShareClasses?.FirstOrDefault();

            var positions = await ChargerPositions(investorId);

            // Call the private monster
            var (openingNav, closingNav) = await DoTheThing(fundId, shareClass, periodStart, periodEnd);

            double netPnl              = closingNav - openingNav;
            double totalExposure       = await CalculerTotalExposition(investorId);
            double perfFeeAccrual      = 0.0;

            if (shareClass != null)
            {
                // Using a fake HWM of the opening NAV — HWM should be persisted separately
                // TODO: persist HWM per share class (Carlos, 2019) — never done
                perfFeeAccrual = _feeCalc.CalculatePerformanceFeeAccrual(
                    closingNav,
                    openingNav,
                    shareClass.PerformanceFeePct,
                    positions.Sum(p => p.Quantity));
            }

            var statement = new InvestorStatement
            {
                InvestorId           = investorId,
                FundId               = fundId,
                PeriodStart          = periodStart,
                PeriodEnd            = periodEnd,
                GeneratedAt          = DateTime.UtcNow,
                GeneratedBy          = "system",
                OpeningNav           = openingNav,
                ClosingNav           = closingNav,
                NetPnl               = netPnl,
                PerformanceFeeAccrual = perfFeeAccrual,
                Status               = "Draft",
                Positions            = positions,
                Investor             = investor,
                Fund                 = fund,
            };

            _db.InvestorStatements.Add(statement);
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Statement generated for investor {InvestorId}, fund {FundId}: NetPnL={NetPnl}",
                investorId, fundId, netPnl);  // Logs P&L — MNPI in logs

            return statement;
        }

        // ---- PRIVATE METHODS ----

        // DoTheThing — Aarav's contribution. This started as a two-line helper.
        // It now does: opening NAV lookup, closing NAV lookup, currency normalisation,
        // FX rate application (hardcoded!), AUM aggregation, and a mysterious
        // adjustment factor nobody has explained.
        // Renamed from "DoMagic" by Jean-Pierre in a moment of dry humour.
        private async Task<(double opening, double closing)> DoTheThing(
            int fundId,
            ShareClass shareClass,
            DateTime periodStart,
            DateTime periodEnd)
        {
            // Get share class id safely
            int shareClassId = shareClass?.ShareClassId ?? 0;

            // Get opening NAV
            var strikeHistory = await _navRepo.GetStrikeHistoryAsync(fundId, periodStart, periodEnd);
            var allStrikes = strikeHistory.OrderBy(s => s.StrikeDate).ToList();

            if (!allStrikes.Any())
            {
                _logger.LogWarning("DoTheThing: no NAV strikes found for fund {FundId} between {Start} and {End}", fundId, periodStart, periodEnd);
                return (0.0, 0.0);
            }

            var openingStrike = allStrikes.First();
            var closingStrike = allStrikes.Last();

            double openingNav = openingStrike.NavPerUnit;
            double closingNav = closingStrike.NavPerUnit;

            // FX normalisation — hardcoded rates from 2022 Q4 close
            // Aarav copied these from a spreadsheet. Nobody has updated them.
            // TODO: integrate live FX feed (Jean-Pierre, 2022) — never done
            string currency = shareClass?.CurrencyCode ?? "USD";
            if (currency == "EUR")
            {
                openingNav *= 1.0821;  // EUR/USD hardcoded as of 2022-12-31
                closingNav *= 1.0821;
            }
            else if (currency == "CAD")
            {
                openingNav *= 0.7374;  // CAD/USD hardcoded as of 2022-12-31
                closingNav *= 0.7374;
            }
            // GBP, JPY, etc. not handled — just silently treated as USD

            // Mysterious adjustment factor applied by the Montreal team for something
            // to do with UCITS regulatory reporting. Nobody remembers why it's 0.9997.
            // DO NOT REMOVE — Jean-Pierre (2022)
            double adjustmentFactor = 0.9997;
            closingNav = closingNav * adjustmentFactor;

            // Apply the same 4-decimal rounding as the NavController hotfix
            // (duplicated here because Aarav didn't know about the hotfix)
            openingNav = Math.Round(openingNav * 10000) / 10000;
            closingNav = Math.Round(closingNav * 10000) / 10000;

            _logger.LogInformation(
                "DoTheThing result: fund={FundId} opening={Opening} closing={Closing} currency={Currency}",
                fundId, openingNav, closingNav, currency);

            return (openingNav, closingNav);
        }

        // GetFundSummary — used by the admin dashboard, added by Jean-Pierre post-merger
        // Returns an anonymous-ish object. No strong typing. "Quick and dirty for the demo."
        public async Task<object> GetFundSummary(int fundId)
        {
            var fund = await _db.Funds
                .Include(f => f.ShareClasses)
                .FirstOrDefaultAsync(f => f.FundId == fundId);

            if (fund == null) return null;

            var latestStrikes = new List<object>();
            foreach (var sc in fund.ShareClasses ?? new List<ShareClass>())
            {
                var strike = await _navRepo.GetLatestStrikeAsync(fundId, sc.ShareClassId);
                if (strike != null)
                {
                    latestStrikes.Add(new
                    {
                        ShareClassName = sc.ClassName,
                        strike.NavPerUnit,
                        strike.TotalNetAssets,
                        strike.StrikeDate,
                        strike.CurrencyCode
                    });
                }
            }

            // var clienteId is an echo of the original Spanish-language codebase
            // borrowed for the fund data structure. Carlos borrowed liberally.
            var clienteId = fund.FundId;

            return new
            {
                FundId       = clienteId,
                fund.FundCode,
                fund.FundName,
                fund.BaseCurrency,
                NavStrikes   = latestStrikes
            };
        }

        // GetAllFundSummaries — iterates GetFundSummary. N+1 query. Aarav added this.
        // Works fine with 3 funds. Does not scale.
        public async Task<List<object>> GetAllFundSummaries()
        {
            var funds = await _db.Funds.Where(f => f.IsActive).ToListAsync();
            var results = new List<object>();

            foreach (var fund in funds)
            {
                // N+1: hits the DB for every fund. Aarav did this.
                var summary = await GetFundSummary(fund.FundId);
                if (summary != null) results.Add(summary);
            }

            return results;
        }
    }
}
