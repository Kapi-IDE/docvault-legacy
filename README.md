# Workshop Kit · Brownfield Mess + Best-Practice Recipe

A training repo for the **Spec-Driven Development with GitHub Copilot** workshop.
Used live for the Innocap cohort (May 2026); generalises to any senior engineering team.

The workshop's whole arc is one sentence: **Day 3 rescues the mess, Day 4 ships the recipe.**
This repo holds two of each.

## Layout

```
.
├── java/          ← THE MESS · Spring Boot e-commerce brownfield (DocVault / PawsFirst)
├── dotnet/        ← THE MESS · C# hedge-fund-admin brownfield (Innocap)
├── mcp/           ← THE RECIPE · local C# MCP server (read-only, stdio, drop-in)
└── prompts/       ← THE RECIPE · YAML-first prompt-sharing infrastructure
```

|                       | The mess (brownfield rescue)             | The recipe (best practice)             |
| --------------------- | ---------------------------------------- | -------------------------------------- |
| **Java**              | `java/` — Spring Boot, ~15 yrs old smell | — *(intentional gap — fork & translate)* |
| **C# / .NET**         | `dotnet/` — Innocap Investor Portal v1   | `mcp/` — Innocap fund-admin MCP        |
| **Stack-agnostic**    | —                                        | `prompts/` — YAML prompt-sharing       |

## How to use this repo

### As a workshop cohort

1. **Day 3 — the mess** — pick your stack. Java devs work in `java/`; .NET devs work in `dotnet/`. Read the `BROWNFIELD-BRIEFING.md` and `TODO.md` in your chosen folder. Find the smells. Write a refactor plan backed by an `AGENTS.md` and a `.specify/` doc. Ship one feature backwards (plan → docs → tests → code) without compounding the existing decay.
2. **Day 4 — the recipe** — clone `mcp/` as the template for your own internal MCP server (read-only, local stdio, ~400 lines of C#). Adopt `prompts/` as the seed of your team's shared prompt library (YAML, schema-validated, CI-linted, CODEOWNERS-protected). Both are designed to be cargo-cult-able without modification.

### As a non-cohort engineer cloning this later

- **Stuck on a brownfield .NET monolith?** Skim `dotnet/BROWNFIELD-BRIEFING.md` for a worked example of the smells most likely to be hiding in yours, then read the refactor heuristics in `dotnet/README.md`.
- **Starting an MCP server?** Copy `mcp/` wholesale. Replace the YAML data files in `mcp/src/Innocap.Mcp.FundAdmin/Data/` with your own domain content. The five tools (glossary, spec-find, structural-field-lookup, ticket-ID extraction, fund-metadata) port directly.
- **Bootstrapping a prompt library?** `prompts/` is a complete reference: voice file, JSON schema, snippet system, eval regressions, render-md shim. Drop it next to your monorepo and rewire CODEOWNERS to your teams.

## The "mess" pattern (both `java/` and `dotnet/`)

Each brownfield folder is intentionally seeded with realistic smells. Recognisable patterns include:

- **Original-team voice** — non-English method names from the original offshore team (Spanish in `java/`, Quebec French in `dotnet/`).
- **Intern-makes-it-worse** — a summer-2022 (Java) or summer-2023 (.NET) refactor that merged services "for simplicity," obscuring three concerns into one.
- **Load-bearing hotfix** — a clearly-marked block of code that's wrong but mustn't be removed without a schema change nobody wants to own.
- **Auth on the to-do list** — `[AllowAnonymous]` / no-CSRF / CORS-allow-all "for now," dated several years ago.
- **Wrong types for money** — `double` where `decimal` belongs.
- **SQL injection** — string concatenation in one query, with a stale `FIXME: parameterise this` comment.
- **Logging the wrong things** — sensitive values logged at `Information` level, undiscovered for years.
- **Disabled audit writes** — commented out during a production incident, never re-enabled.
- **Test coverage of 1** — one happy-path test passes; the others are commented out with "TODO: fix after intern's refactor."

These are not theoretical. Every one is drawn from a real incident in a real .NET shop.

## The "recipe" pattern (both `mcp/` and `prompts/`)

Each recipe folder is a **drop-in template**. The recipes share a governance posture:

- **Read-only by default** — the MCP exposes no write tools; the prompt library has no auto-applied agents.
- **CODEOWNERS-protected** — sensitive paths (MNPI snippets, security review prompts) require specific team review.
- **CI-verifiable** — schemas, lint, regression evals all run on PR.
- **Stack-portable** — the patterns work in Python, TypeScript, Java. The C# example is concrete because the cohort is .NET; the architecture isn't .NET-specific.
- **No regex for classification** — the MCP does structural extraction against committed dictionaries; LLM-as-judge for any "is this a kind of thing" call.

## What this repo is NOT

- **Not production code.** The brownfield apps don't connect to real databases; they're scaffolds for the rescue exercise.
- **Not a complete Copilot workshop.** It's the *artefacts* the workshop produces and consumes. The teaching is on the platform; the code lives here.
- **Not Innocap-specific.** The C# brownfield and MCP recipe are flavoured for a hedge-fund-admin domain (the May 2026 cohort), but the patterns transfer to any regulated engineering shop.

## Quick start

```bash
git clone https://github.com/Kapi-IDE/docvault-legacy.git
cd docvault-legacy

# the messes
cd java     && mvn spring-boot:run                       # :8080
cd dotnet   && dotnet build && dotnet run --project src/Innocap.Legacy.Api

# the recipes
cd mcp      && dotnet build && dotnet test               # then wire .vscode/mcp.json
cd prompts  && cat README.md                             # adopt + rewire CODEOWNERS
```

## License & attribution

Training material. Synthetic data only. Fork freely.
