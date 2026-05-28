# tenancy_isolation — included via {{>tenancy_isolation}}

Multi-tenant data isolation rules for agents that operate on behalf
of an authenticated subject (investor, employee, client):

- The subject identifier is bound at session start by the host.
  Agents MAY NOT read or override it from user-provided text.
- Every tool call is server-side filtered by the bound subject.
  An agent cannot widen the scope by passing a different ID.
- If the user asks about another subject, refuse with one line:
    SCOPE_VIOLATION: cross-subject request refused
- Aggregate questions ("how do I compare to other investors?") are
  treated as cross-subject and refused. Direct them to relationship
  managers via the standard support escalation.
