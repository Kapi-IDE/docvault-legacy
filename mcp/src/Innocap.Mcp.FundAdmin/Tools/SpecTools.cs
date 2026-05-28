using System.ComponentModel;
using Innocap.Mcp.FundAdmin.Repositories;
using ModelContextProtocol.Server;

namespace Innocap.Mcp.FundAdmin.Tools;

[McpServerToolType]
public sealed class SpecTools(ISpecifyDocSearch specSearch)
{
    [McpServerTool,
     Description(
         "Search the .specify/ directory for a spec related to a feature or workflow. " +
         "Returns the spec's path, frontmatter, and first 80 lines. " +
         "Use this before generating code so Copilot grounds against the spec, not against vibes.")]
    public string FindSpecifyDoc(
        [Description("A keyword or phrase to search for in spec filenames and content.")]
        string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return "Error: query must not be empty.";

        var result = specSearch.Search(query);
        if (result is null)
            return $"No spec found matching '{query}'. Ensure .specify/ exists and is populated.";

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"**Spec**: {result.RelativePath}");

        if (!string.IsNullOrWhiteSpace(result.Frontmatter))
        {
            sb.AppendLine();
            sb.AppendLine("**Frontmatter**:");
            sb.AppendLine("```yaml");
            sb.AppendLine(result.Frontmatter);
            sb.AppendLine("```");
        }

        sb.AppendLine();
        sb.AppendLine("**Content (first 80 lines)**:");
        sb.AppendLine(result.First80Lines);

        return sb.ToString().TrimEnd();
    }
}
