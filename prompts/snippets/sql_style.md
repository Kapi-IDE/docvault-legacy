# sql_style — included via {{>sql_style}}

- T-SQL. SQL Server 2022 / Azure SQL.
- Keywords UPPERCASE. Identifiers PascalCase for tables/views,
  snake_case for columns (per `efcore_house_rules` mapping).
- Always two-part names (`schema.Table`); no `dbo` for new work.
- Parameterized always — no string concatenation, no `EXEC(@sql)`
  unless wrapped in `sp_executesql` with typed parameters.
- `SET NOCOUNT ON;` at the top of every procedure.
- Explicit column lists in `INSERT` and `SELECT` — never `SELECT *`
  outside ad-hoc investigation.
