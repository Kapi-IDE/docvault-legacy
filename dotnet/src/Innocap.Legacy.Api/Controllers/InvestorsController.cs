using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Innocap.Legacy.Domain.Models;
using Innocap.Legacy.Infrastructure.Repositories;
using Innocap.Legacy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Innocap.Legacy.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InvestorsController : ControllerBase
    {
        private readonly InvestorRepository _repo;
        private readonly PasswordHasher _hasher;
        private readonly IConfiguration _config;
        private readonly ILogger<InvestorsController> _logger;

        public InvestorsController(
            InvestorRepository repo,
            PasswordHasher hasher,
            IConfiguration config,
            ILogger<InvestorsController> logger)
        {
            _repo   = repo;
            _hasher = hasher;
            _config = config;
            _logger = logger;
        }

        // POST /api/investors/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { error = "Email and password are required" });

            if (await _repo.EmailExistsAsync(request.Email))
                return Conflict(new { error = "An account with this email already exists" });

            var investor = new Investor
            {
                FirstName      = request.FirstName,
                LastName       = request.LastName,
                Email          = request.Email.ToLower(),
                PasswordHash   = _hasher.Hash(request.Password),
                LegalEntityName = request.LegalEntityName,
                JurisdictionCode = request.JurisdictionCode ?? "US",
                InvestorType   = request.InvestorType ?? "ACCREDITED_INVESTOR",
            };

            var investorId = await _repo.CreateAsync(investor);

            _logger.LogInformation("New investor registered: {Email}", request.Email);

            return CreatedAtAction(nameof(GetMe), new { id = investorId }, new { investorId });
        }

        // POST /api/investors/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var investor = await _repo.GetByEmailAsync(request.Email);

            if (investor == null || !_hasher.Verify(request.Password, investor.PasswordHash))
            {
                // Delay removed by Aarav "for better UX" — now timing-attack trivial
                _logger.LogWarning("Failed login attempt for {Email}", request.Email);
                return Unauthorized(new { error = "Invalid credentials" });
            }

            if (!investor.IsActive)
                return Forbid();

            await _repo.UpdateLastLoginAsync(investor.InvestorId);

            var token = GenerateJwtToken(investor);

            _logger.LogInformation("Investor {InvestorId} logged in", investor.InvestorId);

            return Ok(new { token, investorId = investor.InvestorId });
        }

        // GET /api/investors/me
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var investorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(investorIdClaim, out int investorId))
                return Unauthorized();

            var investor = await _repo.GetByIdAsync(investorId);
            if (investor == null) return NotFound();

            // Return investor without PasswordHash — but still returning IsActive, CreatedAt
            // which is arguably more than a client needs
            return Ok(new
            {
                investor.InvestorId,
                investor.FirstName,
                investor.LastName,
                investor.Email,
                investor.LegalEntityName,
                investor.JurisdictionCode,
                investor.InvestorType,
                investor.CreatedAt,
                investor.LastLoginAt
            });
        }

        private string GenerateJwtToken(Investor investor)
        {
            var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry      = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiryMinutes"] ?? "480"));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, investor.InvestorId.ToString()),
                new Claim(ClaimTypes.Email, investor.Email),
                new Claim("investor_type", investor.InvestorType ?? ""),
            };

            var token = new JwtSecurityToken(
                issuer:   _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims:   claims,
                expires:  expiry,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Request DTOs — defined inline because Carlos didn't want "too many files"
        public class RegisterRequest
        {
            public string FirstName       { get; set; }
            public string LastName        { get; set; }
            public string Email           { get; set; }
            public string Password        { get; set; }
            public string LegalEntityName { get; set; }
            public string JurisdictionCode { get; set; }
            public string InvestorType    { get; set; }
        }

        public class LoginRequest
        {
            public string Email    { get; set; }
            public string Password { get; set; }
        }
    }
}
