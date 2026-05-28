using FluentAssertions;
using Innocap.Mcp.FundAdmin.Repositories;
using Innocap.Mcp.FundAdmin.Tools;
using Xunit;

namespace Innocap.Mcp.FundAdmin.Tests;

// ─────────────────────────────────────────────────────────────────────────────
// In-memory fake — no mocking framework needed for such a simple interface.
// ─────────────────────────────────────────────────────────────────────────────
file sealed class FakeGlossaryRepository : IGlossaryRepository
{
    private readonly List<GlossaryEntry> _entries =
    [
        new GlossaryEntry(
            Term: "NAV",
            Definition: "Net Asset Value per share.",
            Aliases: ["net asset value", "net_asset_value"],
            RegulatoryContext: "UCITS Art. 84"),
        new GlossaryEntry(
            Term: "HWM",
            Definition: "High-Water Mark used for performance fees.",
            Aliases: ["high water mark", "high_water_mark"],
            RegulatoryContext: null),
    ];

    private readonly Dictionary<string, GlossaryEntry> _index;

    public FakeGlossaryRepository()
    {
        _index = new Dictionary<string, GlossaryEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in _entries)
        {
            _index.TryAdd(e.Term.ToLowerInvariant(), e);
            foreach (var alias in e.Aliases)
                _index.TryAdd(alias.ToLowerInvariant(), e);
        }
    }

    public GlossaryEntry? Find(string term) =>
        _index.TryGetValue(term.Trim().ToLowerInvariant(), out var e) ? e : null;

    public IReadOnlyList<GlossaryEntry> All() => _entries;
}

// ─────────────────────────────────────────────────────────────────────────────
public sealed class GlossaryToolsTests
{
    private readonly GlossaryTools _sut = new(new FakeGlossaryRepository());

    [Fact]
    public void GlossaryLookup_KnownTerm_ReturnsDefinition()
    {
        var result = _sut.GlossaryLookup("NAV");

        result.Should().Contain("Net Asset Value per share.");
        result.Should().Contain("NAV");
    }

    [Fact]
    public void GlossaryLookup_ByAlias_ReturnsCanonicalEntry()
    {
        var result = _sut.GlossaryLookup("high water mark");

        result.Should().Contain("HWM");
        result.Should().Contain("High-Water Mark");
    }

    [Fact]
    public void GlossaryLookup_CaseInsensitive_Matches()
    {
        var lower = _sut.GlossaryLookup("nav");
        var upper = _sut.GlossaryLookup("NAV");

        lower.Should().Be(upper);
    }

    [Fact]
    public void GlossaryLookup_UnknownTerm_ReturnsFallbackMessage()
    {
        var result = _sut.GlossaryLookup("XYZNONEXISTENT");

        result.Should().Contain("No glossary entry found");
        result.Should().Contain("GlossaryList");
    }

    [Fact]
    public void GlossaryList_ReturnsOneLinePerTerm()
    {
        var result = _sut.GlossaryList();

        result.Should().Contain("NAV");
        result.Should().Contain("HWM");
        // Two terms → two lines
        result.Split(Environment.NewLine).Length.Should().Be(2);
    }
}
