using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Innocap.Mcp.FundAdmin.Repositories;

/// <summary>
/// Loads fund metadata from <c>Data/funds.yaml</c>.
///
/// Expected YAML shape:
/// <code>
/// - fund_code: ICAP-GLB-1
///   fund_name: Innocap Global Fund I
///   jurisdiction: Cayman Islands
///   aifmd_applicable: false
///   ucits_applicable: false
///   share_class_count: 3
///   custodian: Northern Trust
///   base_currency: USD
/// </code>
/// </summary>
public sealed class YamlFundRegistry : IFundRegistry
{
    private readonly Dictionary<string, FundMetadata> _byCode;

    public YamlFundRegistry()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "funds.yaml");
        var list = Load(path);
        _byCode = list.ToDictionary(f => f.FundCode.ToUpperInvariant(), StringComparer.OrdinalIgnoreCase);
    }

    public FundMetadata? FindByCode(string fundCode)
    {
        _byCode.TryGetValue(fundCode.Trim().ToUpperInvariant(), out var fund);
        return fund;
    }

    private static List<FundMetadata> Load(string path)
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

        var raw = deserializer.Deserialize<List<RawFund>?>(yaml) ?? [];

        return raw
            .Where(r => !string.IsNullOrWhiteSpace(r.FundCode ?? r.Code))
            .Select(r =>
            {
                // Support both "fund_code"/"code" key variants in YAML
                var code = (r.FundCode ?? r.Code)!.Trim();
                return new FundMetadata(
                    code,
                    r.FundName ?? r.Name ?? code,
                    r.Jurisdiction ?? "Unknown",
                    r.AifmdApplicable,
                    r.UcitsApplicable,
                    r.ShareClassCount ?? r.ShareClasses ?? 0,
                    r.Custodian,
                    r.BaseCurrency);
            })
            .ToList();
    }

    private sealed class RawFund
    {
        // Canonical keys
        public string? FundCode { get; set; }
        public string? FundName { get; set; }
        public string? Jurisdiction { get; set; }
        public bool AifmdApplicable { get; set; }
        public bool UcitsApplicable { get; set; }
        public int? ShareClassCount { get; set; }
        public string? Custodian { get; set; }
        public string? BaseCurrency { get; set; }
        // Alternate keys used by data-files agent
        public string? Code { get; set; }
        public string? Name { get; set; }
        public int? ShareClasses { get; set; }
    }
}
