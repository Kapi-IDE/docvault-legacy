# Innocap — Brownfield Rescue + MCP Recipe

World's largest DMA (dedicated managed account) platform for hedge funds. AUM $100B+. Acquired BNY Mellon's HedgeMark in 2022 — Montreal HQ, NYC operations, Chennai dev arm.

This workshop kit contains two parallel projects for the Innocap cohort (Day 3–4):

1. **`legacy/`** — The C# / .NET brownfield app you'll rescue and refactor on Day 3. Innocap Investor Portal v1, built Montreal 2019, partially broken in 2023. Handles 3,200+ investor-portal logins daily.
2. **`mcp/`** — A read-only MCP server recipe (Day 4) that surfaces glossary, DB-field metadata, and fund reference data to Copilot, demonstrating spec-driven design.

## Running the Legacy App

```bash
cd innocap/legacy
dotnet build
dotnet run
```

The app runs on `http://localhost:5000`. SQL Server (or H2 equivalent) in-memory database pre-loaded with synthetic fund and investor data.

## Running the MCP Server

```bash
cd innocap/mcp
dotnet build
dotnet run
```

Then configure your IDE's `mcp.json` with the server endpoint. See `mcp/tools/mcp-config-template.json` for the snippet.

## Known Issues & Smells

1. **SQL injection in PositionRepository** — Line 142 concatenates investor ID into raw SQL without parameterization
2. **BCrypt work factor = 4** — Cryptographically weak; current standard is 12+. See `PasswordService.cs:18`
3. **Quebec method names** — Carlos's Montreal team left French naming (CalculerNAV, ChargerPositions) in the codebase
4. **AllowAnonymous on AdminController** — Accidentally allows unauthenticated reads of investor positions
5. **Audit log writes commented out** — Disabled 2022-09-14 during a production incident; never re-enabled. AIFMD compliance gap.
6. **NAV stored as `double` instead of `decimal`** — Precision loss on rounding. See `NavController.cs:201`
7. **2022-Q4 currency rounding hotfix** — Lines in NavController are load-bearing. Carlos left this before he departed. DO NOT REMOVE.
8. **PositionNavStatementService mega-merge** — Aarav merged three services into one summer 2023 because "it was cleaner." No tests written; high bug density.
9. **MNPI logged at Information level** — Sensitive market position data (MNPI) written to application logs in plaintext
10. **Hardcoded SQL Server password from 2019** — Still visible in startup config. Should be environment variable.

## Historical Team

- **Carlos M.** — Montreal lead developer (left 2020). Built the original platform.
- **Priya S.** — Backend developer (left 2021). Designed the investment accounting layer.
- **Aarav P.** — Intern, summer 2023 (left 2023). Made the mega-service merge. "I'll write tests next sprint" — he did not.

## Developer Guide

See `mcp/AGENTS.md` for the MCP build rules, test standards, and AI-editing guardrails. This is both a workshop reference AND the actual trunk-level rules for edits to the MCP server.
