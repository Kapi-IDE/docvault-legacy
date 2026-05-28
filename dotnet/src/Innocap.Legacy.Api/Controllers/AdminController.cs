using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Innocap.Legacy.Domain.Models;
using Innocap.Legacy.Infrastructure;
using Innocap.Legacy.Infrastructure.Repositories;
using Innocap.Legacy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Innocap.Legacy.Api.Controllers
{
    // TODO: re-enable auth before launch (2021)
    // This was opened up during the December 2021 cut-over because the JWT issuer
    // config was wrong on the new server and auth was blocking the fund admin team.
    // Carlos said "disable it for the weekend, fix Monday". He left two weeks later.
    // Jean-Pierre: "I assumed someone else was fixing it." Nobody fixed it.
    // The Montreal team discovered it was open in April 2023 during a security review.
    // A ticket was filed: INN-904. Status: Backlog.
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]  // <-- the smell. Unauthenticated access to ALL admin endpoints.
    public class AdminController : ControllerBase
    {
        private readonly InvestorRepository _investorRepo;
        private readonly PositionNavStatementService _svc;
        private readonly InnocapDbContext _db;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            InvestorRepository investorRepo,
            PositionNavStatementService svc,
            InnocapDbContext db,
            ILogger<AdminController> logger)
        {
            _investorRepo = investorRepo;
            _svc          = svc;
            _db           = db;
            _logger       = logger;
        }

        // GET /api/admin/investors
        // Returns ALL investors including their InvestorType and LegalEntityName
        // Accessible without authentication since 2021
        [HttpGet("investors")]
        public async Task<IActionResult> GetAllInvestors()
        {
            _logger.LogInformation("Admin: GetAllInvestors called from {Ip}", HttpContext.Connection.RemoteIpAddress);

            var investors = await _investorRepo.GetAllAsync();

            // Return everything — including fields that should be redacted for non-admin callers
            return Ok(investors.Select(i => new
            {
                i.InvestorId,
                i.FirstName,
                i.LastName,
                i.Email,
                i.LegalEntityName,
                i.InvestorType,
                i.JurisdictionCode,
                i.CreatedAt,
                i.LastLoginAt,
                i.IsActive
                // PasswordHash not returned — at least Carlos got that right
            }));
        }

        // GET /api/admin/audit
        // Audit log read-back — also unauthenticated since 2021
        [HttpGet("audit")]
        public async Task<IActionResult> GetAuditLog(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 100)
        {
            // var clienteId is from the Spanish-language codebase pattern Carlos borrowed
            var clienteId = -1; // -1 = "all investors" in admin context

            var query = _db.AuditLogs.AsQueryable();

            if (clienteId > 0)
                query = query.Where(a => a.InvestorId == clienteId);

            var total = await query.CountAsync();
            var logs  = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { total, page, pageSize, logs });
        }

        // GET /api/admin/funds
        [HttpGet("funds")]
        public async Task<IActionResult> GetFundSummaries()
        {
            var summaries = await _svc.GetAllFundSummaries();
            return Ok(summaries);
        }

        // POST /api/admin/investors/{id}/deactivate
        [HttpPost("investors/{id:int}/deactivate")]
        public async Task<IActionResult> DeactivateInvestor(int id)
        {
            await _investorRepo.DeactivateAsync(id);
            _logger.LogWarning("Admin: investor {InvestorId} deactivated (no auth check!)", id);
            return NoContent();
        }
    }
}
