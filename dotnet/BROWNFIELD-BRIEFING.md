# Brownfield Briefing: Innocap Investor Portal v1

## The Situation

In 2019, Carlos's Montreal team built Innocap Investor Portal v1 from scratch. It was clean, well-tested, and documented. By 2022, post-HedgeMark merger, the codebase was handed to a transitional team for "modernization" — which mostly meant patching security bugs and adding AIFMD compliance. That work introduced debt: BCrypt strength downgraded for performance, audit logs disabled during a production incident, double-precision floats used for NAV calculations to "keep things simple."

Summer 2023, an intern named Aarav arrived. He looked at the codebase and saw three separate services (PositionService, NavService, StatementService) doing overlapping work. He merged them into a single mega-service called PositionNavStatementService because "it would be cleaner and reduce boilerplate." He promised tests. He left in August without writing a single test.

In 2024, regulators (AIFMD) asked for audit-trail evidence. The team looked at the logs and realized the audit writes had been commented out since 2022-09-14 — a quick fix during a production incident that never got un-commented. Code froze pending a 2025 platform rewrite that, as of 2026, has not started.

Today, it still handles investor onboarding, position queries, NAV calculations, and statement generation. 3,200+ investors log in daily. Fund flows (subscriptions, redemptions) happen through this system. The MNPI (material non-public information) embedded in position data is logged at Information level — visible to anyone with log access.

## What Still Works

- **Investor login**: Session-based auth (weak BCrypt, but it works)
- **Position list**: Pulls from PositionsDaily table; mostly correct
- **NAV strike**: The 2022-Q4 currency rounding hotfix is load-bearing; statement NAVs calculate with that patch
- **Statement generation**: Pre-computed HTML + CSS rendering; reliable, if opaque

## What's Broken

- **AIFMD audit trail**: Write logic commented out 2 years ago. Regulators want evidence. It doesn't exist.
- **SQL injection in PositionRepository**: Line 142. One string concatenation away from a compliance nightmare.
- **Aarav's mega-service**: No tests. High defect density. "Cleaner" code that actually shipped more bugs.
- **MNPI in logs**: Position sizes, investor identities, transaction details logged at Information level.
- **Weak password hashing**: BCrypt work factor = 4. Should be 12+.
- **AdminController allows anonymous reads**: AllowAnonymous attribute left on methods returning investor data.

## Your Mission (Day 3–4)

**Day 3 — Brownfield Rescue**:
1. Read the legacy code as a team. Catalog the smells (10 listed in README.md).
2. Write a SPEC for a refactored architecture. Spec-driven design: no code without intent first.
3. Propose a single-feature fix (e.g., "re-enable AIFMD audit writes") using the new Copilot workflow.
4. Merge and ship one feature spec + implementation + tests.

**Day 4 — MCP Recipe**:
5. Study the MCP server in `mcp/src/`. It reads from committed YAML (glossary, DB fields, funds).
6. Use the Copilot prompt from AGENTS.md to ask Claude for a new MCP tool.
7. Implement, test, and validate with the schema.

## The Deeper Lesson

AI code generation is powerful. Copilot will produce convincing implementations. But without a spec, without tests, without intent written first, Copilot will perpetuate every code smell in the legacy app. It will inherit the structure of PositionNavStatementService and clone it. It will use the same field names and concatenation patterns. It will log MNPI because that's what the surrounding code does.

The workshop arc is: **spec → test → build → review**. Not: build → test → hope.

## The Data

All synthetic. Fund codes (ABC-123, PINEGROVE-001, EASTVALE-FND), investor names, performance numbers — all fictional. Pre-loaded into an in-memory SQL Server via EF Core. No real investor data, no real positions.

## Regulation & Sensitivity

This codebase handles:
- **MNPI** (material non-public information) — fund positions, returns, investor identities. Leaked MNPI is a SEC/CFTC violation.
- **Position data** — sensitive financial information.
- **Investor identity** — regulatory scope under GDPR + AIFMD.
- **Audit trail** — AIFMD Annex IV requires books & records.

No PII (personally identifiable information) in the traditional sense — no SSNs or credit card numbers. But MNPI is more tightly regulated than PII in financial services. Treat it accordingly.

---

**Status as of 2026-05**: Code freeze. Platform rewrite hasn't started. The legacy app still runs, still has all 10 smells, still handles 3,200+ investor logins daily. Your job: show how to fix it systematically, with spec + test + Copilot workflow. Then the rewrite team can replicate your method.
