# BankingApp API

ASP.NET Core API for BankingApp.

## Running Locally With Docker

Generate local development env files:

```bash
python scripts/secrets/setup_dev.py
```

If the env files already exist and you want to regenerate them:

```bash
python scripts/secrets/setup_dev.py --force
```

Start the local stack from the `BankingApp` directory:

```bash
docker compose up --build
```

That starts:

- SQL Server on port `1433`
- the API on `http://localhost:5024`

Docker Compose reads:

```txt
.env
src/BankingApp.Api/.env
```

The root `.env` is for Compose infrastructure values. The API `.env` is injected into the `server` container with `env_file`.

## Configuration

The API reads configuration from:

1. `appsettings.json`
2. `appsettings.Development.json`
3. environment variables
4. user secrets for local direct `dotnet run` usage

Important environment variables:

| Key | Purpose |
|---|---|
| `ConnectionStrings__BankingAppDb` | SQL Server connection string |
| `Database__ApplyMigrations` | Set to `false` to skip automatic EF Core migrations at startup |
| `Jwt__Secret` | JWT signing secret |
| `Otp__Secret` | server-side OTP secret |
| `Email__SmtpHost` | SMTP host |
| `Email__SmtpPort` | SMTP port |
| `Email__SmtpUser` | SMTP username |
| `Email__SmtpPass` | SMTP password |
| `Email__FromAddress` | sender address |

## Running Without Docker

You can also run the API directly:

```bash
dotnet run --project src/BankingApp.Api/BankingApp.Api.csproj
```

If you do that, you still need a reachable SQL Server instance and the required secrets/configuration.

One option is to write .NET User Secrets with:

```bash
python scripts/secrets/setup_dev.py --user-secrets
```

Apply schema changes with EF Core migrations.

## Production Env Generation

Generate a production env handoff file with:

```bash
python scripts/secrets/generate_prod_env.py --connection-string "Server=...;Database=BankingAppDb;User Id=...;Password=...;Encrypt=True;TrustServerCertificate=False;"
```

By default this writes:

```txt
src/BankingApp.Api/.env.production.generated
```

Do not commit generated env files. Install production values into the server, Docker, CI/CD, or hosting secret store.
