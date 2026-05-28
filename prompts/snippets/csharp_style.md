# csharp_style — included via {{>csharp_style}}

- Target: .NET 8, C# 12, nullable enabled, implicit usings on.
- File-scoped namespaces. One public type per file.
- `var` only when the type is obvious from the right-hand side.
- Async methods end in `Async`. Always pass `CancellationToken`.
- Public surface: XML doc comments. Internal/private: no comments
  unless explaining a non-obvious WHY.
- No `#region`. No `string.Concat` for SQL. No swallowed exceptions.
- Prefer `IReadOnlyList<T>` on return surfaces, `List<T>` only for
  local mutation.
- DI: constructor injection only; no service locator.
