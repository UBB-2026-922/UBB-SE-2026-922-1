# BankingApp

Personal banking application.

## Solution Structure

```text
src/
  BankApp.Domain/          core entities and enums
  BankApp.Application/     use cases, DTOs, interfaces
  BankApp.Infrastructure/  persistence and external integrations
  BankApp.Api/             ASP.NET Core API
  BankApp.Desktop/         WinUI desktop client
tests/
  BankApp.Domain.Tests/
  BankApp.Application.Tests/
  BankApp.Infrastructure.Tests/
  BankApp.Infrastructure.Tests.Integration/
  BankApp.Api.Tests/
  BankApp.Api.Tests.Integration/
  BankApp.Desktop.Tests/
scripts/
  client/                  local desktop configuration helpers
  server/                  local API and Docker env helpers
  db/                      schema and seed scripts
```

## Quick Start

### 1. Configure local secrets

Generate `.env` for Docker Compose:

```bash
python scripts/server/setup-dev-env.py --force
```

Write local Google OAuth settings for the desktop client:

```bash
python scripts/client/setup-dev-config.py \
    --client-id <your-client-id> \
    --client-secret <your-client-secret>
```

### 2. Start the local stack

```bash
docker compose up --build
```

The API is exposed at `http://localhost:5024`.

### 3. Open the solution

Open `BankingApp.slnx`, set `BankApp.Desktop` as the startup project, select the `x64` platform, and run it after the API is up.

## Project Docs

- API setup and configuration: [src/BankApp.Api/README.md](src/BankApp.Api/README.md)
- Desktop setup and local OAuth configuration: [src/BankApp.Desktop/README.md](src/BankApp.Desktop/README.md)

## Prerequisites

- Windows 10/11 for the desktop client
- .NET SDK 10.x
- Docker Desktop
- Python 3.10+ for the setup scripts

## Verification

```bash
dotnet test BankingApp.slnx
```
