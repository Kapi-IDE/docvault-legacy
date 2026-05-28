using System.ComponentModel;
using System.Text.RegularExpressions;
using Innocap.Mcp.FundAdmin.Repositories;
using ModelContextProtocol.Server;

namespace Innocap.Mcp.FundAdmin.Tools;

[McpServerToolType]
public sealed class TicketTools(IJiraProjectRegistry jiraProjects)
{
    [McpServerTool,
     Description(
         "Given a block of text, return every Jira ticket ID that matches a known Innocap project prefix " +
         "(e.g. INNOCAP-1234, PLAT-7799). " +
         "Structural extraction only — does NOT call Jira. " +
         "Pair this with the Atlassian MCP if you need ticket details.")]
    public string ListJiraTicketIdsInText(
        [Description("The block of text to scan for Jira ticket IDs.")]
        string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "No text provided.";

        var prefixes = jiraProjects.Prefixes;
        if (prefixes.Count == 0)
            return "No Jira project prefixes configured. Ensure Data/jira-projects.yaml is populated.";

        // Structural ID extraction: known format PREFIX-<digits>.
        // Regex IS allowed per project rules for "matching a known string in a known position"
        // (structured ID parsing). The prefixes come from the committed registry, not hardcoded.
        var escapedPrefixes = string.Join("|", prefixes.Select(Regex.Escape));
        var pattern = $@"\b({escapedPrefixes})-\d+\b";
        var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        var matches = regex.Matches(text)
            .Select(m => m.Value.ToUpperInvariant())
            .Distinct()
            .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return matches.Count == 0
            ? $"No Jira ticket IDs matching known prefixes ({string.Join(", ", prefixes)}) found."
            : $"Found {matches.Count} ticket ID(s):\n{string.Join("\n", matches.Select(id => $"  - {id}"))}";
    }
}
