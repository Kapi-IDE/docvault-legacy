using System;
using Innocap.Legacy.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Innocap.Legacy.Infrastructure
{
    // EF Core 6 context — Carlos, 2019
    // Conventions used instead of Fluent API in most places.
    // A few overrides below for things that broke after the 2022 merger schema changes.
    public class InnocapDbContext : DbContext
    {
        public InnocapDbContext(DbContextOptions<InnocapDbContext> options)
            : base(options)
        {
        }

        public DbSet<Investor> Investors { get; set; }
        public DbSet<Fund> Funds { get; set; }
        public DbSet<ShareClass> ShareClasses { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<NavStrike> NavStrikes { get; set; }
        public DbSet<InvestorStatement> InvestorStatements { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Investor>(entity =>
            {
                entity.HasKey(e => e.InvestorId);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
                entity.Property(e => e.PasswordHash).HasMaxLength(128).IsRequired();
                entity.Property(e => e.JurisdictionCode).HasMaxLength(3);
            });

            modelBuilder.Entity<Fund>(entity =>
            {
                entity.HasKey(e => e.FundId);
                entity.HasIndex(e => e.FundCode).IsUnique();
                entity.Property(e => e.FundCode).HasMaxLength(30).IsRequired();
                entity.Property(e => e.LegalEntityIdentifier).HasMaxLength(20);
            });

            modelBuilder.Entity<ShareClass>(entity =>
            {
                entity.HasKey(e => e.ShareClassId);
                entity.HasOne(e => e.Fund)
                      .WithMany(f => f.ShareClasses)
                      .HasForeignKey(e => e.FundId);
            });

            modelBuilder.Entity<Position>(entity =>
            {
                entity.HasKey(e => e.PositionId);
                entity.HasOne(e => e.Investor)
                      .WithMany(i => i.Positions)
                      .HasForeignKey(e => e.InvestorId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.FundCode).HasMaxLength(30);
            });

            modelBuilder.Entity<NavStrike>(entity =>
            {
                entity.HasKey(e => e.NavStrikeId);
                entity.HasIndex(e => new { e.FundId, e.ShareClassId, e.StrikeDate });
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.AuditLogId);
                entity.Property(e => e.EventType).HasMaxLength(64).IsRequired();
                entity.Property(e => e.IpAddress).HasMaxLength(45);
            });

            // Seed data — fictional funds for local dev / training
            modelBuilder.Entity<Fund>().HasData(
                new Fund { FundId = 1, FundCode = "PINE-MASTER-01", FundName = "Pinegrove Master Fund", BaseCurrency = "USD", DomicileCountry = "KY", IsActive = true, InceptionDate = new DateTime(2015, 1, 1) },
                new Fund { FundId = 2, FundCode = "EASTVALE-FA-02", FundName = "Eastvale Founders Class A", BaseCurrency = "USD", DomicileCountry = "KY", IsActive = true, InceptionDate = new DateTime(2018, 6, 1) },
                new Fund { FundId = 3, FundCode = "NORDHOLM-SI-03", FundName = "Nordholm Systematic Income", BaseCurrency = "EUR", DomicileCountry = "IE", IsActive = true, InceptionDate = new DateTime(2017, 3, 15) }
            );
        }
    }
}
