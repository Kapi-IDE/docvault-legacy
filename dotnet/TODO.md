# Innocap TODO — Sprint 47 (2023-08-18)

## P0 — Must Do Before Q4 Close 2023
- [x] Post-merger integration of HedgeMark data feeds (completed Q4 2022)
- [x] AIFMD Annex IV field mapping (completed, then audit writes disabled 2022-09-14)
- [ ] Re-enable AIFMD audit log writes (was P0 in Sprint 46, deferred to 47, then abandoned)
- [ ] Fix SQL injection in PositionRepository (discovered 2023-06, marked P0, not touched)
- [ ] Reduce BCrypt work factor to 12+ (security audit flagged this, deferred)
- [ ] Remove AllowAnonymous from AdminController (compliance review, Sprint 46)

## P1 — Q1 2024
- [ ] Refactor PositionNavStatementService into three separate services (Aarav's mega-merge has 0 test coverage)
- [ ] Migrate NAV storage from `double` to `decimal` (precision loss on rounding, see NAV strike)
- [ ] Move hardcoded SQL Server password to environment variable
- [ ] Extract Quebec method names (CalculerNAV, ChargerPositions, etc.) to English equivalents
- [ ] Add integration tests for investor statement generation flow
- [ ] Implement proper PII/MNPI masking in application logs

## P2 — Q2 2024
- [ ] Audit fund withdrawal/subscription flows (Priya's original work, never fully reviewed post-merger)
- [ ] Document currency rounding hotfix (load-bearing code in NavController, nobody knows why it works)
- [ ] Set up Flyway migrations for schema versioning
- [ ] Add OpenAPI/Swagger docs for investor API
- [ ] Implement rate limiting on investor login endpoint (brute-force risk)

## P3 — Nice to Have
- [ ] Dark mode for investor portal (Aarav requested)
- [ ] Multi-language support for investor statements (requested by NYC operations)
- [ ] Fund performance attribution reporting (prototype exists, never finished)
- [ ] Investor preference notifications (side project, deprioritized)

## Notes

**Redis cluster** — Running on `redis.innocap-mtl.internal:6379-6384`. Priya set it up 2021, never documented the cluster topology. If it breaks, ask if Priya left any notes (she didn't). Current production load: ~2,500 sessions cached, 15 GB heap.

**The 2022-Q4 currency rounding hotfix** — Lines 247-289 in NavController. It's temporary code that became permanent. Carlos left it in his last commit before departing. The real fix requires changing the schema to use `decimal` instead of `double` for NAV calculations, and nobody wants to touch that. DO NOT REMOVE THIS HOTFIX — it's load-bearing and production NAV statements depend on it.

**Aarav's mega-service merge (Sprint 45)** — He merged PositionService, NavService, and StatementService into a single PositionNavStatementService because "it reduces boilerplate and makes the dependency graph simpler." The code is objectively shorter. He promised tests in Sprint 46. He left in August 2023 without writing any. The service now has 8,000+ lines, single responsibility has vanished, and defect density is visibly higher than the original three services. Carlos would have killed him.

**AIFMD audit writes** — Commented out 2022-09-14 during a production incident (bad query hung the database). The incident was resolved in 6 hours, but the audit writes were left commented out "to prevent regression." In 2024, regulators asked for audit trail evidence. The team had nothing to show. Compliance now requires re-enabling these writes, but nobody has traced the original performance issue or written the fix. It's P0 but nobody owns it.

**MNPI in application logs** — PositionRepository queries and results are logged at Information level. This includes investor IDs, position sizes, unrealized PnL. The AIFM (fund administrator) compliance team flagged this in 2024. No mitigation in place.

**Password hashing strength** — BCrypt work factor is 4 (set in PasswordService.cs:18). Current security standards recommend 12+. Changed to 4 in 2022 for "performance reasons" (login time was 200ms). Nobody re-evaluated after infrastructure scaling. Compliance flagged it; not fixed.

**AdminController security** — Has `[AllowAnonymous]` attribute on methods that return investor position summaries. Added in 2019 for a mobile app prototype that never shipped. Should be removed or properly authenticated.

**Hardcoded credentials** — SQL Server connection string has the password embedded in startup config (circa 2019, never refactored). Should be environment variable.

---

*Last edited by Aarav P. on 2023-08-15. Aarav: "I'll finish the AIFMD audit re-enable next sprint and write tests for the mega-service merge" — he did not.*

*Status as of 2026-05: Code freeze pending 2025 platform rewrite (which has not started). The legacy app still handles 3,200+ investor logins daily. All 10 smells still present.*
