namespace Innocap.Mcp.FundAdmin.Repositories;

/// <summary>
/// Registry of known Innocap database field names used for structural extraction.
/// This is NOT a classifier — it holds a committed list of field names.
/// </summary>
public interface IKnownFieldsRegistry
{
    /// <summary>All known field names, in the order loaded.</summary>
    IReadOnlyList<string> AllFields { get; }
}
