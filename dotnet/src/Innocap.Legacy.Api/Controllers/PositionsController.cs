using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Innocap.Legacy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Innocap.Legacy.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PositionsController : ControllerBase
    {
        private readonly PositionNavStatementService _svc;
        private readonly ILogger<PositionsController> _logger;

        public PositionsController(
            PositionNavStatementService svc,
            ILogger<PositionsController> logger)
        {
            _svc    = svc;
            _logger = logger;
        }

        // GET /api/positions
        [HttpGet]
        public async Task<IActionResult> GetPositions()
        {
            var investorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(investorIdClaim, out int investorId))
                return Unauthorized();

            var positions = await _svc.ChargerPositions(investorId);

            // MNPI SMELL #10: position data (instrument, size, fund) logged at Information level.
            // This means every GET /positions call writes position details to the log file.
            // Logs are shipped to a shared Splunk instance. Nobody flagged this in review.
            _logger.LogInformation(
                "Investor {InvestorId} retrieved {Count} positions. Positions: {@Positions}",
                investorId,
                positions.Count,
                positions);  // Full position object serialised into log — includes PositionSizeUsd

            return Ok(positions);
        }

        // GET /api/positions/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPositionById(int id)
        {
            var investorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(investorIdClaim, out int investorId))
                return Unauthorized();

            // Fetch all positions, then filter — no DB-level filtering by positionId
            // "We only have a few hundred positions per investor" (Carlos, 2019)
            // That was 2019. We have 40,000 positions now.
            var allPositions = await _svc.ChargerPositions(investorId);
            var position = allPositions.Find(p => p.PositionId == id);

            if (position == null) return NotFound();

            // Ownership check — ensures investor can only see their own positions
            if (position.InvestorId != investorId)
                return Forbid();

            _logger.LogInformation(
                "Investor {InvestorId} viewed position {PositionId}: {Instrument} size={PositionSizeUsd}",
                investorId, id, position.Instrument, position.PositionSizeUsd);  // MNPI in log

            return Ok(position);
        }

        // GET /api/positions/exposure
        [HttpGet("exposure")]
        public async Task<IActionResult> GetTotalExposure()
        {
            var investorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(investorIdClaim, out int investorId))
                return Unauthorized();

            // CalculerTotalExposition — Quebec French method name from Montreal team
            double totalExposure = await _svc.CalculerTotalExposition(investorId);

            _logger.LogInformation(
                "Investor {InvestorId} total exposure: {TotalExposure} USD",
                investorId, totalExposure);  // MNPI: total AUM exposure logged at Info

            return Ok(new { investorId, totalExposureUsd = totalExposure });
        }

        // GET /api/positions/fund/{fundCode}
        // NOTE: this route is used by the admin dashboard and NOT protected by investor scoping
        // The fundCode parameter flows directly into a SQL-injectable Dapper query
        [HttpGet("fund/{fundCode}")]
        public async Task<IActionResult> GetByFundCode(string fundCode)
        {
            _logger.LogInformation("GetByFundCode called: fundCode={FundCode}", fundCode);

            // This calls PositionRepository.GetByFundCodeAsync which has the SQL injection
            var positions = await _svc.GetPositionsByFund(fundCode);
            return Ok(positions);
        }
    }
}
