using System.ComponentModel;
using Innocap.Mcp.FundAdmin.Repositories;
using ModelContextProtocol.Server;

namespace Innocap.Mcp.FundAdmin.Tools;

[McpServerToolType]
public sealed class GlossaryTools(IGlossaryRepository repo)
{
    [McpServerTool,
     Description(
         "Look up the Innocap house definition for a fund-admin term. " +
         "Returns the canonical definition, aliases, and regulatory context. " +
         "Use this when a dev or another agent mentions an unfamiliar term like " +
         "NAV, HWM, AIFMD, share class, crystallization, etc.")]
    public string GlossaryLookup(
        [Description("The term to look up. Case-insensitive. Aliases accepted.")]
        string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return "Error: term must not be empty. Try GlossaryList to see all available terms.";

        var entry = repo.Find(term);
        return entry is null
            ? $"No glossary entry found for '{term}'. Try GlossaryList."
            : entry.Render();
    }

    [McpServerTool,
     Description("List every term defined in the Innocap glossary. Returns one line per term.")]
    public string GlossaryList()
    {
        var all = repo.All();
        if (all.Count == 0)
            return "No glossary entries loaded. Ensure Data/glossary.yaml is populated.";

        return string.Join(Environment.NewLine, all.Select(e => e.Term));
    }
}
