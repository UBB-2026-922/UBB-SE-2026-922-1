# BankingApp.Infrastructure

Implements every I/O contract defined in Application: persistence, security primitives, notifications, exchange rates, and caching.

Business rules live in Domain. 

Use-case orchestration lives in Application.

## Dependency rule

Depends on `BankingApp.Application` and `BankingApp.Domain`.

| Package                              | Why                                                      |
|--------------------------------------|----------------------------------------------------------|
| `Microsoft.EntityFrameworkCore`      | ORM for all persistence                                  |
| `BCrypt.Net-Next`                    | Password hashing in `HashService`                        |
| `System.IdentityModel.Tokens.Jwt`    | JWT generation and validation                            |
| `Google.Apis.Auth`                   | Google ID token validation for OAuth login               |
| `Serilog`                            | Structured logging implementation                        |
| `ErrorOr`                            | Result type, consistent with Application and Domain      |

## Structure

```
BankingApp.Infrastructure/
├── Caching/              # ILockedRateCache — short-lived in-memory rate locks
├── Common/
│   ├── Clock/
│   ├── Notifications/
│   ├── Security/
│   └── Logging/
├── DependencyInjection/
├── ExchangeRates/        # IExchangeRateService — live forex rate retrieval
└── Persistence/
    ├── AppDbContext.cs
    ├── UnitOfWork.cs
    ├── Configurations/   # One IEntityTypeConfiguration<T> per aggregate root
    ├── Migrations/
    └── Repositories/     # One IXRepository implementation per Domain aggregate root
```

## Persistence

### AppDbContext

Inherits `DbContext`. Entity configurations are applied via `ApplyConfigurationsFromAssembly` — `OnModelCreating` contains no inline configuration. Each aggregate root has a corresponding `IEntityTypeConfiguration<T>` in `Configurations/`.

### Repositories

One repository per aggregate root, implementing the `Domain.Repositories.IXRepository` interface. All methods are async. Repositories receive `AppDbContext` via constructor injection and do not wrap a secondary data-access layer.

### UnitOfWork

Wraps `AppDbContext.SaveChangesAsync`. Application handlers call `IUnitOfWork.SaveChangesAsync` once per command after all mutations.

### Migrations

EF Core code-first migrations. Run `dotnet ef migrations add` from this project when the domain model changes.

## Common services

| Class                 | Contract               | Notes                                                      |
|-----------------------|------------------------|------------------------------------------------------------|
| `HashService`         | `IHashService`         | BCrypt verify/hash                                         |
| `JsonWebTokenService` | `IJsonWebTokenService` | Issues and validates JWTs; reads config from `JwtSettings` |
| `OtpService`          | `IOtpService`          | TOTP generation and verification                           |
| `OtpAttemptTracker`   | `IOtpAttemptTracker`   | In-memory attempt counter with sliding window              |
| `SystemClock`         | `ISystemClock`         | Returns `DateTime.UtcNow`                                  |
| `EmailService`        | `IEmailService`        | SMTP dispatch; templates in `EmailTemplates`               |

## Caching

`ILockedRateCache` stores short-lived `LockedRate` entries per user for the two-step forex flow. Backed by `IMemoryCache` for automatic TTL expiry without a separate eviction thread.

## Exchange rates

`IExchangeRateService` retrieves a live exchange rate for a currency pair. Implementation lives in `ExchangeRates/`.

## Registration

```csharp
services.AddInfrastructure(configuration);
```

Registers `AppDbContext`, `UnitOfWork`, all repositories, all `Common` services, `ILockedRateCache`, and `IExchangeRateService`.
