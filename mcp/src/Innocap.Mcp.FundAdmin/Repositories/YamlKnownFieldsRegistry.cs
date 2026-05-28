using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Innocap.Mcp.FundAdmin.Repositories;

/// <summary>
/// Loads the committed list of Innocap database field names from
/// <c>Data/known-db-fields.yaml</c>.
///
/// Supported YAML shapes:
///
/// Shape A — flat list of strings (minimal):
/// <code>
/// - fund_id
/// - share_class_code
/// </code>
///
/// Shape B — list of objects (data-files agent format):
/// <code>
/// - field: InvestorId
///   table: dbo.Investors
///   sensitivity: investor-identity
///   description: "Unique investor identifier"
/// </code>
/// </summary>
public sealed class YamlKnownFieldsRegistry : IKnownFieldsRegistry
{
    public IReadOnlyList<string> AllFields { get; }

    public YamlKnownFieldsRegistry()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "known-db-fields.yaml");
        AllFields = Load(path);
    }

    private static IReadOnlyList<string> Load(string path)
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

        // Try object list first (Shape B)
        try
        {
            var objectList = deserializer.Deserialize<List<RawFieldEntry>?>(yaml) ?? [];
            if (objectList.Count > 0 && objectList[0].Field is not null)
            {
                return objectList
                    .Where(f => !string.IsNullOrWhiteSpace(f.Field))
                    .Select(f => f.Field!.Trim())
                    .ToList();
            }
        }
        catch { /* fall through to Shape A */ }

        // Fall back to flat string list (Shape A)
        var stringList = deserializer.Deserialize<List<string>?>(yaml) ?? [];
        return stringList
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .Select(f => f.Trim())
            .ToList();
    }

    private sealed class RawFieldEntry
    {
        public string? Field { get; set; }
        public string? Table { get; set; }
        public string? Sensitivity { get; set; }
        public string? Description { get; set; }
    }
}
