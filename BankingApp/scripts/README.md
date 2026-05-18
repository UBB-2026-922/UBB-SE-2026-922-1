# BankingApp Setup CLI

Use `scripts/setup.py` to generate, inspect, edit, and clean BankingApp environment setup.

## Commands

```bash
python scripts/setup.py generate dev [--local | --docker-db | --docker-api]
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

`generate dev` writes the values needed for local development:

```txt
BankingApp/.env
BankingApp/src/BankingApp.Api/.env
BankingApp/src/BankingApp.Api user secrets
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
DevLogin:Email
DevLogin:Password
DevLogin:FullName
```

## Notes

- `get` reads the active config source for the selected environment.
- `set` updates the selected target for the chosen environment.
- `clean` removes generated setup files and development secrets.
