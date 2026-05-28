namespace Innocap.Mcp.FundAdmin.Repositories;

/// <summary>Innocap fund-admin glossary — canonical term definitions.</summary>
public interface IGlossaryRepository
{
    /// <summary>Find a glossary entry by term name or any of its aliases (case-insensitive).</summary>
    GlossaryEntry? Find(string term);

    /// <summary>Return all terms in insertion order.</summary>
    IReadOnlyList<GlossaryEntry> All();
}

public sealed record GlossaryEntry(
    string Term,
    string Definition,
    IReadOnlyList<string> Aliases,
    string? RegulatoryContext)
{
    /// <summary>Flat string suitable for returning directly from an MCP tool.</summary>
    public string Render()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"**{Term}**");
        sb.AppendLine(Definition);
        if (Aliases.Count > 0)
            sb.AppendLine($"Aliases: {string.Join(", ", Aliases)}");
        if (!string.IsNullOrWhiteSpace(RegulatoryContext))
            sb.AppendLine($"Regulatory context: {RegulatoryContext}");
        return sb.ToString().TrimEnd();
    }
}
