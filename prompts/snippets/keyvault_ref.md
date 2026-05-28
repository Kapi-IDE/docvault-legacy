# keyvault_ref — included via {{>keyvault_ref}}

Canonical reference format for secrets:

    @Microsoft.KeyVault(SecretUri=https://kv-<env>-<region>.vault.azure.net/secrets/<name>/)

Naming convention for `<name>`:

    <component>--<role>--<purpose>

Examples:

    investor-portal--db--password
    nav-recon--sb--connection
    confluence-bridge--api--token

Vault assignment by environment:

| env  | vault                              |
|------|------------------------------------|
| dev  | kv-dev-eastus2                     |
| uat  | kv-uat-eastus2                     |
| prod | kv-prod-eastus2                    |

NEVER paste the actual secret value. Only the reference URI.
