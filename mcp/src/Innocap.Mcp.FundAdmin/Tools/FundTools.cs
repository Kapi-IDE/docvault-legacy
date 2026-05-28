using System.ComponentModel;
using Innocap.Mcp.FundAdmin.Repositories;
using ModelContextProtocol.Server;

namespace Innocap.Mcp.FundAdmin.Tools;

[McpServerToolType]
public sealed class FundTools(IFundRegistry fundRegistry)
{
    [McpServerTool,
     Description(
         "Get metadata for an Innocap fund by code: jurisdiction, AIFMD-applicable, share-class count, custodian. " +
         "Returns nothing position-related, NAV-related, or investor-related — " +
         "those require explicit elevated scopes the MCP does not have.")]
    public string GetFundMetadata(
        [Description("The fund's short code, e.g. ICAP-GLB-1. Case-insensitive.")]
        string fundCode)
    {
        if (string.IsNullOrWhiteSpace(fundCode))
            return "Error: fundCode must not be empty.";

        var fund = fundRegistry.FindByCode(fundCode);
        if (fund is null)
            return $"Fund '{fundCode}' not found in the registry.";

        var regulations = new List<string>();
        if (fund.AifmdApplicable) regulations.Add("AIFMD");
        if (fund.UcitsApplicable) regulations.Add("UCITS");
        var regStr = regulations.Count > 0 ? string.Join(", ", regulations) : "None";

        return $"""
                Fund: {fund.FundCode} — {fund.FundName}
                Jurisdiction: {fund.Jurisdiction}
                Regulations: {regStr}
                Share classes: {fund.ShareClassCount}
                Custodian: {fund.Custodian ?? "Not specified"}
                Base currency: {fund.BaseCurrency ?? "Not specified"}
                """;
    }
}
