namespace Innocap.Mcp.FundAdmin.Repositories;

/// <summary>Search the .specify/ Spec Kit directory for a spec document.</summary>
public interface ISpecifyDocSearch
{
    /// <summary>
    /// Find the first spec whose filename or content contains <paramref name="query"/> (case-insensitive).
    /// Returns <c>null</c> if no match.
    /// </summary>
    SpecifyDocResult? Search(string query);
}

public sealed record SpecifyDocResult(
    /// <summary>Path relative to the spec directory root.</summary>
    string RelativePath,
    /// <summary>Raw frontmatter block (YAML between first --- delimiters), if present.</summary>
    string? Frontmatter,
    /// <summary>First 80 lines of the document body (after frontmatter).</summary>
    string First80Lines);
