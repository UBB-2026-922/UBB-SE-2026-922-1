# BankingApp Secret Scripts

This directory contains Python scripts for generating local development and production handoff configuration for the BankingApp backend.

The scripts only manage configuration and secrets. They do not create database schema, seed data, or run SQL files. EF Core migrations are the database schema source of truth.

Google OAuth values are intentionally not generated.

## Files

```txt
scripts/secrets/setup_dev.py
scripts/secrets/generate_prod_env.py
```

## Configuration Layout

Development uses two env files:

```txt
BankingApp/.env
BankingApp/src/BankingApp.Api/.env
```

`BankingApp/.env` is for Docker Compose infrastructure values. Right now that means the SQL Server SA password used by the `db` service.

`BankingApp/src/BankingApp.Api/.env` is for backend runtime values. Docker Compose injects it into the `server` container through `env_file`.

ASP.NET Core does not load `.env` files by itself. This repo does not use `DotNetEnv`. The API `.env` file is used by Docker Compose, not directly by `dotnet run`.

For direct non-Docker API development, `setup_dev.py --user-secrets` can also write the same API values to .NET User Secrets.

## Current Backend Keys

The API reads these configuration keys:

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
```

In env files, use ASP.NET Core environment variable names with double underscores:

```txt
ConnectionStrings__BankingAppDb
Jwt__Secret
Otp__Secret
Email__SmtpHost
Email__SmtpPort
Email__SmtpUser
Email__SmtpPass
Email__FromAddress
Database__ApplyMigrations
```

Do not use the old names:

```txt
ConnectionStrings:DefaultConnection
Otp:ServerSecret
OTP_SERVER_SECRET
```

## Development

Generate local Docker development configuration:

```bash
python scripts/secrets/setup_dev.py
```

If files already exist, use:

```bash
python scripts/secrets/setup_dev.py --force
```

Then start the development stack:

```bash
docker compose up --build
```

The API is exposed on:

```txt
http://localhost:5024
```

### What `setup_dev.py` Writes

Root Compose env file:

```txt
BankingApp/.env
```

Example contents:

```env
DB_SA_PASSWORD=generated-password
```

API runtime env file:

```txt
BankingApp/src/BankingApp.Api/.env
```

Example contents:

```env
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__BankingAppDb=Server=db;Database=BankingAppDb;User Id=sa;Password=generated-password;TrustServerCertificate=True;
Jwt__Secret=generated-secret
Otp__Secret=generated-secret
Email__SmtpHost=smtp.example.com
Email__SmtpPort=587
Email__SmtpUser=dev@example.com
Email__SmtpPass=placeholder
Email__FromAddress=dev@example.com
Database__ApplyMigrations=true
```

The generated JWT and OTP secrets are random. The SQL Server password is random unless provided with `--db-password`.

### Development Options

Docker database mode is the default:

```bash
python scripts/secrets/setup_dev.py --db-mode docker
```

Local SQL Server with Windows authentication:

```bash
python scripts/secrets/setup_dev.py --db-mode local
```

Configure SMTP:

```bash
python scripts/secrets/setup_dev.py --force \
  --smtp-host smtp.gmail.com \
  --smtp-port 587 \
  --smtp-user you@example.com \
  --smtp-pass app-password \
  --smtp-from you@example.com
```

If SMTP values are omitted, obvious placeholders are written and the script prints a warning. The API can still run, but email sending will fail until real SMTP credentials are configured.

Also write .NET User Secrets for direct `dotnet run` usage:

```bash
python scripts/secrets/setup_dev.py --force --user-secrets
```

This calls:

```bash
dotnet user-secrets set --project src/BankingApp.Api
```

It does not edit the user secrets storage file directly.

## Production

Production secrets should be generated once per environment and then stored in the deployment platform's secret store.

Generate a production env handoff file:

```bash
python scripts/secrets/generate_prod_env.py \
  --connection-string "Server=...;Database=BankingAppDb;User Id=...;Password=...;Encrypt=True;TrustServerCertificate=False;" \
  --smtp-host smtp.example.com \
  --smtp-port 587 \
  --smtp-user noreply@example.com \
  --smtp-pass real-password \
  --smtp-from noreply@example.com
```

Default output:

```txt
BankingApp/src/BankingApp.Api/.env.production.generated
```

Overwrite an existing generated file:

```bash
python scripts/secrets/generate_prod_env.py --force --connection-string "..."
```

Production SMTP values are required by default. To deliberately generate placeholders:

```bash
python scripts/secrets/generate_prod_env.py \
  --connection-string "..." \
  --allow-placeholder-smtp
```

### Production Migration Setting

The production generator writes:

```env
Database__ApplyMigrations=false
```

That is intentional. Production migrations should be an explicit deployment step, not a side effect of starting the API.

Recommended migration command:

```bash
dotnet ef database update --project src/BankingApp.Infrastructure --startup-project src/BankingApp.Api
```

If production later moves to Docker deployment, run migrations from a one-off migration container or deployment job before starting the API container.

## Generated Files

Generated secret files must stay uncommitted:

```txt
BankingApp/.env
BankingApp/src/BankingApp.Api/.env
BankingApp/src/BankingApp.Api/.env.production.generated
```

Committed examples:

```txt
BankingApp/.env.example
BankingApp/src/BankingApp.Api/.env.example
BankingApp/src/BankingApp.Api/.env.production.example
```

## Secret Generation

The scripts use Python's `secrets` module.

Current defaults:

```txt
SQL Server SA password: 24 characters with uppercase, lowercase, digit, and symbol
JWT secret: 48 random bytes, base64 encoded
OTP secret: 32 random bytes, base64 encoded
```

The scripts do not print raw secret values to the console.
