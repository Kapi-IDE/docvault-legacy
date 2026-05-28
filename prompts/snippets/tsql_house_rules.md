# tsql_house_rules — included via {{>tsql_house_rules}}

- `CREATE OR ALTER PROCEDURE` — never `CREATE PROCEDURE` alone.
- `SET NOCOUNT ON; SET XACT_ABORT ON;` at top of every procedure.
- Parameter validation block immediately after `AS BEGIN`:
  raise a typed error (`THROW 50000 + offset, ...`) for invalid inputs.
- `BEGIN TRY ... END TRY BEGIN CATCH ... END CATCH` around mutations.
- Explicit transactions with `XACT_STATE()` checks in the CATCH.
- Final `SELECT @@ROWCOUNT AS rows_affected;` for mutating procs.
- No cursors unless justified in a header comment.
- No SELECT *; explicit column lists.
