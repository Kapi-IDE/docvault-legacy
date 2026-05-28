using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Innocap.Mcp.FundAdmin.Repositories;

/// <summary>
/// Loads the list of known Innocap Jira project key prefixes from
/// <c>Data/jira-projects.yaml</c>.
///
/// Supported YAML shapes:
///
/// Shape A — flat list of strings:
/// <code>
/// - INNOCAP
/// - PLAT
/// </code>
///
/// Shape B — list of objects (data-files agent format):
/// <code>
/// - prefix: INNOCAP
///   name: "Innocap Platform"
///   description: "Core investor portal …"
/// </code>
/// </summary>
public sealed class YamlJiraProjectRegistry : IJiraProjectRegistry
{
    public IReadOnlyList<string> Prefixes { get; }

    public YamlJiraProjectRegistry()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "jira-projects.yaml");
        Prefixes = Load(path);
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
            var objectList = deserializer.Deserialize<List<RawProject>?>(yaml) ?? [];
            if (objectList.Count > 0 && objectList[0].Prefix is not null)
            {
                return objectList
                    .Where(p => !string.IsNullOrWhiteSpace(p.Prefix))
                    .Select(p => p.Prefix!.Trim().ToUpperInvariant())
                    .Distinct()
                    .ToList();
            }
        }
        catch { /* fall through to Shape A */ }

        // Flat string list (Shape A)
        var stringList = deserializer.Deserialize<List<string>?>(yaml) ?? [];
        return stringList
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p.Trim().ToUpperInvariant())
            .Distinct()
            .ToList();
    }

    private sealed class RawProject
    {
        public string? Prefix { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
