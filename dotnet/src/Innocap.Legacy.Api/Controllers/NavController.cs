using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Innocap.Legacy.Domain.Models;
using Innocap.Legacy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Innocap.Legacy.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NavController : ControllerBase
    {
        private readonly PositionNavStatementService _svc;
        private readonly ILogger<NavController> _logger;

        public NavController(
            PositionNavStatementService svc,
            ILogger<NavController> logger)
        {
            _svc    = svc;
            _logger = logger;
        }

        // GET /api/nav/strikes?fundId=1&shareClassId=1
        [HttpGet("strikes")]
        public async Task<IActionResult> GetLatestStrike([FromQuery] int fundId, [FromQuery] int shareClassId)
        {
            var strike = await _svc.CalculerNAV(fundId, shareClassId);
            if (strike == null)
                return NotFound(new { error = $"No NAV strike found for fund {fundId}, share class {shareClassId}" });

            // Apply the load-bearing 2022 Q4 rounding hotfix before returning
            strike = ApplyNavRoundingHotfix(strike);

            return Ok(strike);
        }

        // GET /api/nav/summary
        [HttpGet("summary")]
        public async Task<IActionResult> GetFundSummary([FromQuery] int fundId)
        {
            var summary = await _svc.GetFundSummary(fundId);
            if (summary == null)
                return NotFound(new { error = $"Fund {fundId} not found" });

            return Ok(summary);
        }

        // GET /api/nav/all
        [HttpGet("all")]
        public async Task<IActionResult> GetAllFundSummaries()
        {
            var summaries = await _svc.GetAllFundSummaries();
            return Ok(summaries);
        }

        // POST /api/nav/strike
        [HttpPost("strike")]
        public async Task<IActionResult> RecordNavStrike([FromBody] NavStrikeRequest request)
        {
            if (request.FundId <= 0 || request.ShareClassId <= 0)
                return BadRequest(new { error = "FundId and ShareClassId are required" });

            if (request.NavPerUnit <= 0)
                return BadRequest(new { error = "NAV per unit must be positive" });

            var performedBy = User.FindFirst(ClaimTypes.Email)?.Value ?? "unknown";

            var strike = new NavStrike
            {
                FundId           = request.FundId,
                ShareClassId     = request.ShareClassId,
                StrikeDate       = request.StrikeDate == default ? DateTime.UtcNow.Date : request.StrikeDate,
                NavPerUnit       = request.NavPerUnit,
                TotalNetAssets   = request.TotalNetAssets,
                UnitsOutstanding = request.UnitsOutstanding,
                CurrencyCode     = request.CurrencyCode ?? "USD",
                StrikeType       = request.StrikeType ?? "Provisional",
                CreatedBy        = performedBy,
            };

            // Apply hotfix to the incoming NAV before persisting
            strike = ApplyNavRoundingHotfix(strike);

            var id = await _svc.RecordNavStrike(strike);

            _logger.LogInformation(
                "NAV strike recorded: fund={FundId}, sc={ShareClassId}, nav={NavPerUnit}, id={Id}",
                strike.FundId, strike.ShareClassId, strike.NavPerUnit, id);

            return CreatedAtAction(nameof(GetLatestStrike),
                new { fundId = strike.FundId, shareClassId = strike.ShareClassId },
                new { navStrikeId = id });
        }

        // ============================================================
        // HOTFIX 2022-Q4 CLOSE: DO NOT REMOVE — Carlos.
        // The real fix needs a schema change (convert NavPerUnit from float to decimal(18,6)
        // in the SQL Server schema and regenerate the EF model). That schema change was
        // approved by Risk in November 2022 but never executed because the migration
        // window conflicted with quarter-end close. Jean-Pierre applied this workaround
        // at 11pm on 2022-12-30 to prevent incorrect NAV strikes from going to investors.
        //
        // What it does: SQL Server float → C# double introduces ~1e-10 precision error.
        // Multiplying by 10000, rounding to int, then dividing by 10000 strips the
        // floating-point noise below 4 decimal places. For NAV values < $10,000 this
        // is safe. For NAV values > $100,000 the rounding still introduces up to $0.01
        // error, which is within Innocap's acceptable NAV tolerance of $0.05.
        //
        // Failure mode if removed: NAV strikes show values like 1234.56789012341 instead of
        // 1234.5679, which fails the fund admin's automated reconciliation checks and
        // triggers a manual review escalation. This happened in Q3 2022 and cost 4 hours.
        //
        // The schema change ticket is INN-847. It has been "In Progress" since 2022-11-04.
        // ============================================================
        private static NavStrike ApplyNavRoundingHotfix(NavStrike strike)
        {
            // Step 1: strip floating-point noise from NavPerUnit to 4 decimal places
            strike.NavPerUnit = Math.Round(strike.NavPerUnit * 10000) / 10000;

            // Step 2: TotalNetAssets rounded to 2 decimal places (cents)
            strike.TotalNetAssets = Math.Round(strike.TotalNetAssets * 100) / 100;

            // Step 3: units outstanding — 6 decimal places (fractional units for some share classes)
            strike.UnitsOutstanding = Math.Round(strike.UnitsOutstanding * 1000000) / 1000000;

            // Step 4: apply the same dance to NavPerUnit a second time
            // because Jean-Pierre found a residual error on 2022-12-30 at 11:47pm
            // and "just did it again" to be safe. Yes, it's idempotent. No, it's not clean.
            strike.NavPerUnit = Math.Round(strike.NavPerUnit * 10000) / 10000;

            return strike;
        }

        public class NavStrikeRequest
        {
            public int      FundId           { get; set; }
            public int      ShareClassId     { get; set; }
            public DateTime StrikeDate       { get; set; }
            public double   NavPerUnit       { get; set; }
            public double   TotalNetAssets   { get; set; }
            public double   UnitsOutstanding { get; set; }
            public string   CurrencyCode     { get; set; }
            public string   StrikeType       { get; set; }
        }
    }
}
