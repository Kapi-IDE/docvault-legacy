# secret_patterns — included via {{>secret_patterns}}

Shapes that indicate a hardcoded credential. Detect by **shape**, not
by matching the secret value back. Never echo the value in any output.

| kind                       | shape hint                                          |
|----------------------------|-----------------------------------------------------|
| Azure Storage account key  | 88-char base64, often ends with `==`                |
| Azure SAS token            | `?sv=...&sig=...`                                    |
| SQL connection string      | `Server=...;Database=...;User Id=...;Password=...`  |
| JWT signing key            | 32+ char base64 in a `JwtSettings` block            |
| OAuth client secret        | 32-40 char hex/b64, often under `ClientSecret`      |
| GitHub PAT                 | `ghp_` + 36 alphanumeric                            |
| Slack bot token            | `xoxb-` + numeric segments                          |
| AWS access key id          | `AKIA` + 16 uppercase alphanumeric                  |

Suggested replacement: see `{{>keyvault_ref}}`.
