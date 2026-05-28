using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Innocap.Legacy.Domain.Models;
using Microsoft.Data.SqlClient;

namespace Innocap.Legacy.Infrastructure.Repositories
{
    // NAV repository uses raw ADO.NET SqlCommand — Carlos's very first version (2019).
    // Jean-Pierre wanted to migrate it to Dapper after the merger but "didn't have time".
    // Now it lives alongside EF Core and Dapper. Enjoy.
    public class NavRepository
    {
        private readonly string _connectionString;

        public NavRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<NavStrike> GetLatestStrikeAsync(int fundId, int shareClassId)
        {
            NavStrike strike = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new SqlCommand(
                    "SELECT TOP 1 * FROM NavStrikes WHERE FundId = @FundId AND ShareClassId = @ShareClassId ORDER BY StrikeDate DESC",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@FundId", fundId);
                    cmd.Parameters.AddWithValue("@ShareClassId", shareClassId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            strike = MapFromReader(reader);
                        }
                    }
                }
            }

            return strike;
        }

        public async Task<List<NavStrike>> GetStrikeHistoryAsync(int fundId, DateTime fromDate, DateTime toDate)
        {
            var strikes = new List<NavStrike>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new SqlCommand(
                    "SELECT * FROM NavStrikes WHERE FundId = @FundId AND StrikeDate BETWEEN @FromDate AND @ToDate ORDER BY StrikeDate DESC",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@FundId", fundId);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            strikes.Add(MapFromReader(reader));
                        }
                    }
                }
            }

            return strikes;
        }

        public async Task<int> InsertStrikeAsync(NavStrike strike)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new SqlCommand(
                    @"INSERT INTO NavStrikes (FundId, ShareClassId, StrikeDate, NavPerUnit, TotalNetAssets, UnitsOutstanding, CurrencyCode, StrikeType, CreatedAt, CreatedBy)
                      VALUES (@FundId, @ShareClassId, @StrikeDate, @NavPerUnit, @TotalNetAssets, @UnitsOutstanding, @CurrencyCode, @StrikeType, @CreatedAt, @CreatedBy);
                      SELECT SCOPE_IDENTITY();",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@FundId", strike.FundId);
                    cmd.Parameters.AddWithValue("@ShareClassId", strike.ShareClassId);
                    cmd.Parameters.AddWithValue("@StrikeDate", strike.StrikeDate);
                    cmd.Parameters.AddWithValue("@NavPerUnit", strike.NavPerUnit);
                    cmd.Parameters.AddWithValue("@TotalNetAssets", strike.TotalNetAssets);
                    cmd.Parameters.AddWithValue("@UnitsOutstanding", strike.UnitsOutstanding);
                    cmd.Parameters.AddWithValue("@CurrencyCode", strike.CurrencyCode ?? "USD");
                    cmd.Parameters.AddWithValue("@StrikeType", strike.StrikeType ?? "Provisional");
                    cmd.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
                    cmd.Parameters.AddWithValue("@CreatedBy", strike.CreatedBy ?? "system");

                    var result = await cmd.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        // Manual mapping because Carlos didn't want to "add another library" in 2019
        private static NavStrike MapFromReader(IDataReader reader)
        {
            return new NavStrike
            {
                NavStrikeId      = reader.GetInt32(reader.GetOrdinal("NavStrikeId")),
                FundId           = reader.GetInt32(reader.GetOrdinal("FundId")),
                ShareClassId     = reader.GetInt32(reader.GetOrdinal("ShareClassId")),
                StrikeDate       = reader.GetDateTime(reader.GetOrdinal("StrikeDate")),
                NavPerUnit       = reader.GetDouble(reader.GetOrdinal("NavPerUnit")),
                TotalNetAssets   = reader.GetDouble(reader.GetOrdinal("TotalNetAssets")),
                UnitsOutstanding = reader.GetDouble(reader.GetOrdinal("UnitsOutstanding")),
                CurrencyCode     = reader.IsDBNull(reader.GetOrdinal("CurrencyCode")) ? "USD" : reader.GetString(reader.GetOrdinal("CurrencyCode")),
                StrikeType       = reader.IsDBNull(reader.GetOrdinal("StrikeType")) ? "Provisional" : reader.GetString(reader.GetOrdinal("StrikeType")),
                CreatedAt        = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                CreatedBy        = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) ? null : reader.GetString(reader.GetOrdinal("CreatedBy")),
            };
        }
    }
}
