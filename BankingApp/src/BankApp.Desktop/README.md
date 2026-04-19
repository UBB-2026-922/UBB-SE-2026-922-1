# BankingApp Desktop

WinUI 3 desktop client for BankingApp. The app runs unpackaged and talks to the local API over HTTP.

## Prerequisites

- The API must already be running
- Windows 10/11
- .NET SDK 10.x

See [../BankApp.Api/README.md](../BankApp.Api/README.md) for API setup.

## Local OAuth Configuration

`scripts/client/setup-dev-config.py` writes `src/BankApp.Desktop/appsettings.Local.json` with your Google OAuth credentials.

```bash
python scripts/client/setup-dev-config.py \
    --client-id 123456.apps.googleusercontent.com \
    --client-secret GOCSPX-abc123
```

The generated file is gitignored and can be overwritten safely by rerunning the script.

## Configuration

Load order:

```text
appsettings.json
appsettings.Local.json
environment variables
```

Key settings:

| Key | Default |
|---|---|
| `ApiBaseUrl` | `http://localhost:5024` |
| `OAuth:Google:Authority` | `https://accounts.google.com` |
| `OAuth:Google:RedirectUri` | `http://127.0.0.1:7890/` |

`OAuth:Google:ClientId` and `OAuth:Google:ClientSecret` are required for Google sign-in.

## Running

Open `BankingApp.slnx`, set `BankApp.Desktop` as the startup project, select `x64`, and run it.

For Rider, create a compound run configuration with `BankApp.Api` and `BankApp.Desktop`.
