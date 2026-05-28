using Microsoft.Extensions.Configuration;

namespace Innocap.Mcp.FundAdmin.Repositories;

/// <summary>
/// Scans the <c>.specify/</c> folder (configurable via <c>FundAdmin:SpecifyDirectory</c>)
/// for spec documents.
///
/// Search strategy:
/// 1. Filename contains query (case-insensitive).
/// 2. First 200 lines of the file contain query (case-insensitive).
///
/// Returns the first match found.
/// </summary>
public sealed class FileSystemSpecifyDocSearch : ISpecifyDocSearch
{
    private readonly string _specDir;

    public FileSystemSpecifyDocSearch(IConfiguration configuration)
    {
        // Default: .specify/ relative to working directory
        var configured = configuration["FundAdmin:SpecifyDirectory"];
        _specDir = !string.IsNullOrWhiteSpace(configured) ? configured : ".specify";
    }

    public SpecifyDocResult? Search(string query)
    {
        if (!Directory.Exists(_specDir))
            return null;

        var normalizedQuery = query.Trim();
        if (string.IsNullOrEmpty(normalizedQuery))
            return null;

        // Walk all .md and .yaml files in .specify/ recursively
        var files = Directory.EnumerateFiles(_specDir, "*", SearchOption.AllDirectories)
            .Where(f =>
            {
                var ext = Path.GetExtension(f).ToLowerInvariant();
                return ext is ".md" or ".yaml" or ".yml" or ".txt";
            });

        foreach (var filePath in files)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            if (fileName.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase))
                return ReadDoc(filePath);

            // Also search inside the file
            var lines = ReadLines(filePath, limit: 200);
            if (lines.Any(l => l.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)))
                return ReadDoc(filePath);
        }

        return null;
    }

    private SpecifyDocResult ReadDoc(string filePath)
    {
        var allLines = File.ReadAllLines(filePath);
        var (frontmatter, bodyStart) = ExtractFrontmatter(allLines);
        var bodyLines = allLines.Skip(bodyStart).Take(80);

        var relativePath = Path.GetRelativePath(_specDir, filePath);
        return new SpecifyDocResult(
            relativePath,
            frontmatter,
            string.Join(Environment.NewLine, bodyLines));
    }

    private static (string? Frontmatter, int BodyStart) ExtractFrontmatter(string[] lines)
    {
        // YAML frontmatter: first line is "---", ends at the next "---"
        if (lines.Length == 0 || lines[0].Trim() != "---")
            return (null, 0);

        var end = Array.FindIndex(lines, 1, l => l.Trim() == "---");
        if (end < 0)
            return (null, 0);

        var fm = string.Join(Environment.NewLine, lines[1..end]);
        return (fm, end + 1);
    }

    private static IEnumerable<string> ReadLines(string path, int limit)
    {
        using var reader = new StreamReader(path);
        int count = 0;
        while (count < limit && reader.ReadLine() is { } line)
        {
            yield return line;
            count++;
        }
    }
}
