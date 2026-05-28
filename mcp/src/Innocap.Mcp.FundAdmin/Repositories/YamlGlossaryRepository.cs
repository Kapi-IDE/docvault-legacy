using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Innocap.Mcp.FundAdmin.Repositories;

/// <summary>
/// Loads glossary entries from <c>Data/glossary.yaml</c> at startup.
///
/// Expected YAML shape (list of objects):
/// <code>
/// - term: NAV
///   definition: Net Asset Value — the per-share value of a fund's assets minus liabilities.
///   aliases: [net_asset_value, "net asset value"]
///   regulatory_context: UCITS Art. 84; AIFMD Annex IV
/// </code>
/// </summary>
public sealed class YamlGlossaryRepository : IGlossaryRepository
{
    private readonly List<GlossaryEntry> _entries;

    // Lookup index: all lower-cased terms + aliases → canonical entry
    private readonly Dictionary<string, GlossaryEntry> _index;

    public YamlGlossaryRepository()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "glossary.yaml");
        _entries = Load(path);
        _index = BuildIndex(_entries);
    }

    public GlossaryEntry? Find(string term)
    {
        _index.TryGetValue(term.Trim().ToLowerInvariant(), out var entry);
        return entry;
    }

    public IReadOnlyList<GlossaryEntry> All() => _entries;

    // ──────────────────────────────────────────────────────────────────────────
    // Private helpers
    // ──────────────────────────────────────────────────────────────────────────

    private static List<GlossaryEntry> Load(string path)
    {
        if (!File.Exists(path))
            return [];

        var yaml = File.ReadAllText(path);
        if (string.IsNullOrWhiteSpace(yaml))
            return [];

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        var raw = deserializer.Deserialize<List<RawEntry>?>(yaml) ?? [];

        return raw
            .Where(r => !string.IsNullOrWhiteSpace(r.Term))
            .Select(r => new GlossaryEntry(
                r.Term!.Trim(),
                r.Definition ?? string.Empty,
                (r.Aliases ?? []).Select(a => a.Trim()).Where(a => a.Length > 0).ToList(),
                r.RegulatoryContext ?? r.Regulatory))
            .ToList();
    }

    private static Dictionary<string, GlossaryEntry> BuildIndex(List<GlossaryEntry> entries)
    {
        var idx = new Dictionary<string, GlossaryEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in entries)
        {
            idx.TryAdd(e.Term.ToLowerInvariant(), e);
            foreach (var alias in e.Aliases)
                idx.TryAdd(alias.ToLowerInvariant(), e);
        }
        return idx;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // YAML DTO
    // ──────────────────────────────────────────────────────────────────────────

    private sealed class RawEntry
    {
        public string? Term { get; set; }
        public string? Definition { get; set; }
        public List<string>? Aliases { get; set; }
        // Data files may use "regulatory" or "regulatory_context" — handle both.
        public string? Regulatory { get; set; }
        public string? RegulatoryContext { get; set; }
        public List<string>? SeeAlso { get; set; }
    }
}
