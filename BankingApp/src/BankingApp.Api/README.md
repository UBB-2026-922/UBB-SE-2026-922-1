# BankingApp API

ASP.NET Core API for BankingApp.

## Running Locally

The simplest local setup uses Docker Compose from the repository root:

```bash
python scripts/server/setup-dev-env.py --force
docker compose up --build
```

That starts:

- SQL Server on port `1433`
- the API on `http://localhost:5024`

## Configuration

The API reads configuration from:

1. `appsettings.json`
2. `appsettings.Development.json`
3. environment variables
4. user secrets for local development

Important environment variables used by Docker Compose:

| Key | Purpose |
|---|---|
| `ConnectionStrings__DefaultConnection` | SQL Server connection string |
| `Jwt__Secret` | JWT signing secret |
| `Otp__ServerSecret` | server-side OTP secret |
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
