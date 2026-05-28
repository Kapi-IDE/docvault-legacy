using System.ComponentModel;
using Innocap.Mcp.FundAdmin.Repositories;
using ModelContextProtocol.Server;

namespace Innocap.Mcp.FundAdmin.Tools;

[McpServerToolType]
public sealed class DbFieldTools(IKnownFieldsRegistry registry)
{
    [McpServerTool,
     Description(
         "Given a block of text (e.g. a draft commit message, PR description, comment, or SQL snippet), " +
         "return every Innocap database field name that appears verbatim. " +
         "This is structural string matching against a committed registry — " +
         "it does NOT classify whether the text leaks MNPI. " +
         "The caller (Copilot, reviewer, or human) decides what to do with the matches.")]
    public string ListKnownDbFieldsInText(
        [Description("The block of text to scan. May be a commit message, SQL, PR body, or any free text.")]
        string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "No text provided.";

        // Structural Contains match against a committed field list.
        // Not a classifier; no MNPI inference — see tool description.
        var matches = registry.AllFields
            .Where(field => text.Contains(field, StringComparison.OrdinalIgnoreCase))
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return matches.Count == 0
            ? "No known Innocap database fields found in the provided text."
            : $"Matched {matches.Count} known field(s):\n{string.Join("\n", matches.Select(m => $"  - {m}"))}";
    }
}
