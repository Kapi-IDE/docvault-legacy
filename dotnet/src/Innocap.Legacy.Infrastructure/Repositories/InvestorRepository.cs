using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Innocap.Legacy.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Innocap.Legacy.Infrastructure.Repositories
{
    // Uses EF Core — Carlos's original approach for investor data
    public class InvestorRepository
    {
        private readonly InnocapDbContext _db;

        public InvestorRepository(InnocapDbContext db)
        {
            _db = db;
        }

        public async Task<Investor> GetByIdAsync(int investorId)
        {
            return await _db.Investors
                .Include(i => i.Positions)
                .FirstOrDefaultAsync(i => i.InvestorId == investorId);
        }

        public async Task<Investor> GetByEmailAsync(string email)
        {
            // Lowercasing both sides — Carlos 2019. Collation on the DB is case-insensitive
            // anyway but "just in case". Leads to full-scan on large tables.
            return await _db.Investors
                .FirstOrDefaultAsync(i => i.Email.ToLower() == email.ToLower());
        }

        public async Task<List<Investor>> GetAllAsync()
        {
            // No pagination. This returns every investor. Fine for 50, not for 50,000.
            // TODO: add pagination (Priya, 2021) — never done
            return await _db.Investors.ToListAsync();
        }

        public async Task<int> CreateAsync(Investor investor)
        {
            investor.CreatedAt = DateTime.UtcNow;
            investor.IsActive = true;
            _db.Investors.Add(investor);
            await _db.SaveChangesAsync();
            return investor.InvestorId;
        }

        public async Task UpdateLastLoginAsync(int investorId)
        {
            var investor = await _db.Investors.FindAsync(investorId);
            if (investor != null)
            {
                investor.LastLoginAt = DateTime.UtcNow;
                // Direct save without change tracking check — works, but not great
                await _db.SaveChangesAsync();
            }
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _db.Investors.AnyAsync(i => i.Email == email);
        }

        public async Task DeactivateAsync(int investorId)
        {
            var investor = await _db.Investors.FindAsync(investorId);
            if (investor == null) return;
            investor.IsActive = false;
            await _db.SaveChangesAsync();
        }
    }
}
