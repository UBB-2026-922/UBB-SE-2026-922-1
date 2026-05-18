# BankingApp Setup CLI

Use `scripts/setup.py` to generate, inspect, edit, and clean BankingApp environment setup.

## Commands

```bash
python scripts/setup.py generate dev --dev-login-email <email> --dev-login-password <password> [--local | --docker-db | --docker-api]
python scripts/setup.py generate prod --connection-string "..."
python scripts/setup.py get dev <key>
python scripts/setup.py get prod <key>
python scripts/setup.py set dev <key> <value> [--user-secrets | --api-env | --compose-env]
python scripts/setup.py set prod <key> <value>
python scripts/setup.py clean dev
python scripts/setup.py clean prod
python scripts/setup.py clean all
```

## Dev Output

`generate dev` writes the values needed for local development.

Depending on mode (`--local`, `--docker-db`, `--docker-api`):

```txt
BankingApp/.env                                              (docker-db and docker-api modes)
BankingApp/src/BankingApp.Api/.env                          (docker-api mode)
BankingApp/src/BankingApp.Api user secrets                  (local and docker-db modes)
BankingApp/src/BankingApp.Desktop/appsettings.Development.json
BankingApp/src/BankingApp.Web/appsettings.Development.json
```

`--dev-login-email` and `--dev-login-password` are required. They are written to all three targets (API, Desktop, Web) so every client uses the same dev account.

### Dev login example

```bash
python scripts/setup.py generate dev \
  --dev-login-email dev@example.com \
  --dev-login-password MyPass!1 \
  --local
```

## Prod Output

`generate prod` writes:

```txt
BankingApp/src/BankingApp.Api/.env.production.generated
```

## Configuration Keys

```txt
ConnectionStrings:BankingAppDb
Jwt:Secret
Otp:Secret
Email:SmtpHost
Email:SmtpPort
Email:SmtpUser
Email:SmtpPass
Email:FromAddress
Database:ApplyMigrations
DevLogin:Email        (API + Desktop + Web — set by generate dev and set dev)
DevLogin:Password     (API + Desktop + Web — set by generate dev and set dev)
DevLogin:FullName     (API only)
```

## Notes

- `get dev DevLogin:Email` and `get dev DevLogin:Password` show all three targets (API, Desktop, Web) when using `--all`.
- `set dev DevLogin:Email` and `set dev DevLogin:Password` always propagate to Desktop and Web `appsettings.Development.json` in addition to the chosen API target.
- `get` reads the active config source for the selected environment.
- `set` updates the selected target for the chosen environment.
- `clean` removes generated setup files and development secrets.
