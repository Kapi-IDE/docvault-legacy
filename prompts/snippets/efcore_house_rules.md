# efcore_house_rules — included via {{>efcore_house_rules}}

- EF Core 8, code-first, migrations in a separate project.
- Configuration via `IEntityTypeConfiguration<T>` — never override
  `OnModelCreating` directly for new entities.
- Naming:
  - Tables: PascalCase singular in code, mapped to snake_case plural
    in the database (e.g. `Investor` → `investors`).
  - Columns: snake_case (`commitment_amount`, `as_of_date`).
- Surrogate keys: `bigint identity`, named `<entity>_id`.
- Timestamps: every entity gets `created_at`, `updated_at` (UTC, set
  by a save-changes interceptor).
- Decimals: always specify precision/scale. NAV-related = `(28, 8)`,
  commitments = `(28, 2)`.
- No lazy loading. No client evaluation warnings allowed.
