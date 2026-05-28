# aspnet_house_rules — included via {{>aspnet_house_rules}}

- `[ApiController]` + attribute routing. Route prefix:
  `api/v{version:apiVersion}/<plural-resource>`.
- Versioning via `Asp.Versioning`; default = `1.0`.
- Auth: `[Authorize(Policy = "...")]` on the controller; per-action
  override allowed only for explicit anonymous endpoints.
- Validation: FluentValidation, auto-registered. Return
  `ValidationProblemDetails` (RFC 7807) on failure.
- Errors: ProblemDetails. Never raw 500. Never echo exception messages
  to the client in non-dev environments.
- Dispatching: controllers are thin; all business logic in MediatR
  handlers under `Application/<Feature>/`.
