namespace Innocap.Mcp.FundAdmin.Repositories;

/// <summary>
/// Registry of known Innocap Jira project prefixes.
/// Used for structural ticket-ID extraction (not classification).
/// </summary>
public interface IJiraProjectRegistry
{
    /// <summary>
    /// All known project key prefixes (e.g. "INNOCAP", "PLAT", "RISK").
    /// Uppercase, no trailing hyphen.
    /// </summary>
    IReadOnlyList<string> Prefixes { get; }
}
