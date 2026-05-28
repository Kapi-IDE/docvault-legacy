# xunit_house_rules — included via {{>xunit_house_rules}}

- xUnit v2. Test class per production class, named `<Target>Tests`.
- Test method naming: `Method_Scenario_ExpectedOutcome`.
- AAA layout with `// arrange` `// act` `// assert` separators only
  when the sections aren't visually obvious.
- `[Theory]` + `[InlineData]` for parameterized cases; `[MemberData]`
  for object inputs.
- Mocks via `Moq`. Strict mode preferred where collaborator surface
  is small.
- No `Thread.Sleep`, no live network, no real database — use the
  in-process test fixtures in `Tests.Infrastructure`.
- Assertions via `FluentAssertions`.
