namespace Innocap.Mcp.FundAdmin.Repositories;

/// <summary>Read-only metadata registry for Innocap funds.</summary>
public interface IFundRegistry
{
    /// <summary>
    /// Look up a fund by its short code (e.g. "ICAP-GLB-1").
    /// Returns <c>null</c> if the code is not found.
    /// </summary>
    FundMetadata? FindByCode(string fundCode);
}

/// <summary>
/// Non-sensitive structural metadata for an Innocap fund.
/// Deliberately excludes any position data, NAV values, and investor identity —
/// those require explicit elevated scopes the MCP does not have.
/// </summary>
public sealed record FundMetadata(
    string FundCode,
    string FundName,
    string Jurisdiction,
    bool AifmdApplicable,
    bool UcitsApplicable,
    int ShareClassCount,
    string? Custodian,
    string? BaseCurrency);
