# Innocap MCP Fund-Admin Server

A read-only Model Context Protocol server that gives Copilot grounded access to
Innocap house context: glossary terms, Spec Kit docs, known DB fields, Jira ticket
IDs, and fund metadata.

## Prerequisites

- .NET 9 SDK (`dotnet --version` → `9.*`)
- NuGet restore will pull `ModelContextProtocol` 1.x (Microsoft official, stable)
- The `Data/` YAML files populated by the data-files agent (build succeeds with
  empty placeholder files too)

## Build

```bash
cd innocap/mcp/src/Innocap.Mcp.FundAdmin
dotnet build
```

## Run

```bash
dotnet run
```

The server speaks MCP over stdio. Wire it into your MCP host (Claude Desktop,
VS Code Copilot, etc.) by pointing the host at the binary.

### Claude Desktop example (`claude_desktop_config.json`)

```json
{
  "mcpServers": {
    "innocap-fundadmin": {
      "command": "dotnet",
      "args": ["run", "--project", "/path/to/innocap/mcp/src/Innocap.Mcp.FundAdmin"]
    }
  }
}
```

## Run tests

```bash
cd innocap/mcp/tests/Innocap.Mcp.FundAdmin.Tests
dotnet test
```

## Data files

All YAML data lives in `Data/`. The data-files agent owns the content.
Empty placeholder files (`[]`) are enough for the server to start; the tools
will return "no entries found" until real data is loaded.

| File                  | Purpose                          |
|-----------------------|----------------------------------|
| `glossary.yaml`       | Fund-admin glossary terms        |
| `known-db-fields.yaml`| Innocap DB field names           |
| `funds.yaml`          | Fund metadata (code, jurisdiction, etc.) |
| `jira-projects.yaml`  | Known Jira project prefixes      |

## Spec Kit docs (`.specify/`)

Place `.specify/` in your working directory (or configure `SpecifyDirectory`
in `appsettings.json`). `FindSpecifyDoc` scans it for matching spec files.
