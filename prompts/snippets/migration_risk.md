# migration_risk — included via {{>migration_risk}}

Classify DACPAC diff operations into three risk tiers:

**DESTRUCTIVE (BLOCK without explicit data-check)**
- `DROP TABLE`, `DROP COLUMN`
- `ALTER COLUMN` narrowing type (e.g. `nvarchar(200) → nvarchar(50)`)
- `ALTER COLUMN` adding `NOT NULL` to a column without a default
- `DROP CONSTRAINT` on a FK that data depends on
- Renames (DACPAC sees these as DROP + ADD)

**LOCKY (WARN — long lock at deploy time)**
- `ADD COLUMN NOT NULL` with a default on a large table
- `CREATE INDEX` non-online on a large table
- `ALTER TABLE ... REBUILD`

**SAFE**
- New tables, new columns nullable, new indexes online,
  new views, new procedures, view/proc body changes.

Every DESTRUCTIVE op requires a pre-deploy data check named in the
migration note.
