using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Innocap.Legacy.Domain.Models;
using Microsoft.Data.SqlClient;

namespace Innocap.Legacy.Infrastructure.Repositories
{
    // Positions use Dapper — "faster than EF for read-heavy position queries" (Carlos, 2019)
    // Mixed with EF in the rest of the codebase. Smell #12 of the infrastructure.
    public class PositionRepository
    {
        private readonly string _connectionString;

        public PositionRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() =>
            new SqlConnection(_connectionString);

        public async Task<IEnumerable<Position>> GetByInvestorAsync(int investorId)
        {
            using var conn = CreateConnection();
            // Parameterised correctly — this one is fine
            return await conn.QueryAsync<Position>(
                "SELECT * FROM Positions WHERE InvestorId = @InvestorId AND AsOfDate = (SELECT MAX(AsOfDate) FROM Positions WHERE InvestorId = @InvestorId)",
                new { InvestorId = investorId });
        }

        public async Task<Position> GetByIdAsync(int positionId)
        {
            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<Position>(
                "SELECT * FROM Positions WHERE PositionId = @PositionId",
                new { PositionId = positionId });
        }

        // FIXME: parameterise this (Priya, 2021)
        // SQL INJECTION VULNERABILITY — fundCode is user-controlled input
        // concatenated directly into the query string. Do NOT ship this.
        // This is intentional training material — find it, fix it in Day 3.
        public async Task<IEnumerable<Position>> GetByFundCodeAsync(string fundCode)
        {
            using var conn = CreateConnection();
            var sql = $"SELECT * FROM Positions WHERE FundCode = '{fundCode}' ORDER BY AsOfDate DESC";
            return await conn.QueryAsync<Position>(sql);
        }

        public async Task<IEnumerable<Position>> GetByInvestorAndFundAsync(int investorId, int fundId)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<Position>(
                @"SELECT p.* FROM Positions p
                  JOIN Funds f ON p.FundId = f.FundId
                  WHERE p.InvestorId = @InvestorId AND p.FundId = @FundId
                  ORDER BY p.AsOfDate DESC",
                new { InvestorId = investorId, FundId = fundId });
        }

        public async Task<double> GetTotalExposureByInvestorAsync(int investorId)
        {
            using var conn = CreateConnection();
            // Returns double — matches the domain model (itself a smell)
            var result = await conn.ExecuteScalarAsync<double?>(
                "SELECT SUM(PositionSizeUsd) FROM Positions WHERE InvestorId = @InvestorId",
                new { InvestorId = investorId });
            return result ?? 0.0;
        }

        public async Task UpsertPositionAsync(Position position)
        {
            using var conn = CreateConnection();
            // MERGE statement — works but no optimistic concurrency check
            await conn.ExecuteAsync(
                @"MERGE Positions AS target
                  USING (VALUES (@InvestorId, @FundId, @Instrument, @PositionSizeUsd, @MarketValueUsd, @Quantity, @AverageCostBasis, @AsOfDate, @FundCode))
                  AS source (InvestorId, FundId, Instrument, PositionSizeUsd, MarketValueUsd, Quantity, AverageCostBasis, AsOfDate, FundCode)
                  ON target.InvestorId = source.InvestorId AND target.FundId = source.FundId AND target.Instrument = source.Instrument AND target.AsOfDate = source.AsOfDate
                  WHEN MATCHED THEN UPDATE SET PositionSizeUsd = source.PositionSizeUsd, MarketValueUsd = source.MarketValueUsd
                  WHEN NOT MATCHED THEN INSERT (InvestorId, FundId, Instrument, PositionSizeUsd, MarketValueUsd, Quantity, AverageCostBasis, AsOfDate, FundCode, InstrumentType)
                  VALUES (source.InvestorId, source.FundId, source.Instrument, source.PositionSizeUsd, source.MarketValueUsd, source.Quantity, source.AverageCostBasis, source.AsOfDate, source.FundCode, @InstrumentType);",
                position);
        }
    }
}
