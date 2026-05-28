# AGENTS.md — Innocap MCP Fund Admin

This is both a workshop reference guide AND the trunk-level rules for any AI agent editing code in the Innocap MCP server.

## Project Overview

The Innocap MCP (Model Context Protocol) server is a read-only, local stdio server that surfaces fund administration metadata to VS Code and Visual Studio Copilot.

**Scope**: Glossary (25 fund-admin terms), database field structural lookup (30+ fields with sensitivity labels), ticket ID extraction, fund reference data. No cloud egress, no writes to external systems, no authentication beyond local file access.

**Use case**: An engineer working on the Innocap Investor Portal asks Copilot: "What fields can I log safely?" Copilot queries the MCP, which returns a list of fields marked `reference` (safe) and excludes fields marked `MNPI` (unsafe). Result: MNPI never gets logged.

## Tech Stack

- **.NET 9**, C# 13 (nullable enabled, file-scoped namespaces)
- **ModelContextProtocol SDK** — server-side bindings
- **YamlDotNet** — YAML parsing for committed data files
- **xUnit** + **FluentAssertions** — testing
- **System.IO + System.Text.Json** — file I/O, serialization

## Build, Test, Format

```bash
cd innocap/mcp
dotnet build
dotnet test
dotnet format --verify-no-changes
```

All three must pass before merge. No exceptions.

## Code Style

**File-scoped namespaces, sealed types, nullable enabled**:

```csharp
namespace Innocap.Mcp.FundAdmin.Tools;

public sealed class GlossaryTool
{
    private readonly IFundAdminRepository _repository;

    public GlossaryTool(IFundAdminRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<GlossaryEntry?> ResolveTermAsync(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return null;

        return await _repository.GetGlossaryEntryAsync(term.ToLowerInvariant());
    }
}
```

**Async naming** — Method ends with `Async` if it returns `Task` or `Task<T>`. No fire-and-forget; all I/O is awaited.

**Records for DTOs**, not classes:

```csharp
public sealed record GlossaryEntry(
    string Term,
    IReadOnlyList<string> Aliases,
    string Definition,
    string Regulatory,
    IReadOnlyList<string> SeeAlso
);
```

## Architecture

```
innocap/mcp/
├── src/Innocap.Mcp.FundAdmin/
│   ├── Program.cs                  # Server entry point, DI setup
│   ├── Tools/
│   │   ├── GlossaryTool.cs
│   │   ├── DatabaseFieldsTool.cs
│   │   ├── FundReferenceTool.cs
│   │   └── TicketIdExtractorTool.cs
│   ├── Repositories/
│   │   ├── IFundAdminRepository.cs
│   │   └── YamlFundAdminRepository.cs
│   └── Data/
│       ├── glossary.yaml           # ~25 fund-admin terms (CODEOWNERS-protected)
│       ├── known-db-fields.yaml    # ~30 field definitions (CODEOWNERS-protected)
│       ├── funds.yaml              # ~12 fictional funds (CODEOWNERS-protected)
│       └── jira-projects.yaml      # ~6 project prefixes (CODEOWNERS-protected)
└── tests/Innocap.Mcp.FundAdmin.Tests/
    ├── Tools/
    ├── Repositories/
    └── Data/  (fixture YAML files)
```

## Data Files (YAML)

All data lives in committed `Data/*.yaml` files. These are the source of truth.

- **glossary.yaml**: Fund administration terms (NAV, HWM, AIFMD, etc.)
- **known-db-fields.yaml**: Structural database field metadata — table, sensitivity label
- **funds.yaml**: Reference fund data (code, name, jurisdiction, custodian)
- **jira-projects.yaml**: Ticket project prefixes for ID extraction

**Why YAML?** Human-editable, version-controlled, schema-validated on load.

## PR Conventions

Use **Conventional Commits**:
- `feat:` new tool or MCP capability
- `fix:` bug fix or security patch
- `test:` test additions or test infra
- `docs:` documentation, examples, data updates
- `refactor:` code structure, no behavior change
- `chore:` dependency updates, CI/CD

**Example**:
```
feat: add GlossaryTool to resolve fund-admin terminology

Implements the Glossary tool for MCP, loading definitions from
data/glossary.yaml. Supports term lookup by exact match and alias.

Tools-Tested-By: @innocap/risk-eng
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Required trailers**:
- `AI-Assisted:` (if Claude/Copilot participated in authoring)
- `Tools-Tested-By:` (if a human reviewed tool behavior against real use cases)

## Security Guardrails

Each rule pairs a "don't" with a "do":

| Don't | Do |
|-------|-----|
| Classify fields with regex (`\d{6}` = "likely MNPI") | Load field definitions from `known-db-fields.yaml`; use structural lookup |
| Add tools that write to external systems | Keep all tools read-only; writes need separate MCP with explicit Frederic approval |
| Log to stdout (it's MCP transport) | Log to stderr only; use `Console.Error.WriteLine` |
| Hardcode field lists in code | Load from `Data/*.yaml`; no field name should be duplicated in code |
| Add cloud-egress dependencies (HttpClient, AWS SDK) | Keep this MCP fully local and offline-capable |
| Assume a field is safe without checking the glossary | Cross-reference every field mention against `known-db-fields.yaml` |

## When in Doubt

Before adding a new tool that touches sensitive fields (MNPI, position data, investor identity):
- Ping `@innocap/risk-eng` for a security review
- Document the sensitivity level (MNPI, position-data, investor-identity, audit, reference)
- Add a test case that verifies the tool doesn't leak the sensitive field in its response

## Definition of Done

For any PR to merge:
1. `dotnet build` passes (no warnings)
2. `dotnet test` passes (100% green)
3. `dotnet format --verify-no-changes` passes (no style violations)
4. Conventional Commits format with required trailers (AI-Assisted, Tools-Tested-By)
5. Tests added for new tools or field-sensitive logic
6. YAML data files are schema-validated on load
7. No new hardcoded field names, fund codes, or project prefixes (must come from Data/*.yaml)
8. Code review by at least one human

## Deployment

The MCP server is deployed as a standalone binary. Dev mode uses `dotnet run`; production uses a compiled `.exe` (Windows) or stripped binary (Linux/macOS).

Configuration via `.vscode/mcp.json` (see `mcp/tools/mcp-config-template.json`).
