# BankingApp.Domain

Contains all business concepts and rules with zero dependencies on infrastructure, frameworks, or other application layers.

## Dependency rule

This project has **no references** to Application, Infrastructure, or any external framework (except `ErrorOr` for result types and `NodaMoney` for monetary values). Everything else in the solution depends on this project, never the other way around.

## Structure

```
BankingApp.Domain/
├── Aggregates/          # Aggregate roots and their owned entities/events
├── Common/
│   ├── Errors/          # Typed error definitions (ErrorOr)
│   ├── Extensions/      # Extension methods on domain types
│   └── Primitives/      # Base classes: Entity<T>, AggregateRoot<T>, ValueObject
├── Enums/               # Domain enumerations
├── Repositories/        # Repository interfaces (implemented in Infrastructure)
├── Services/            # Pure domain services (stateless logic, no I/O)
├── ValueObjects/        # Custom value objects with factory validation
└── ReferenceData/       # Read-only reference data (Billers, Categories)
```

## Aggregates

Each aggregate is a consistency boundary. Only the aggregate root is referenced from outside; inner entities are accessed through the root.

| Aggregate root       | Owned entities / events                        | Purpose                                      |
|----------------------|------------------------------------------------|----------------------------------------------|
| `User`               | `Notification`, `NotificationPreference`       | Profile, preferences, notification inbox     |
| `IdentityAccount`    | `Session`, `PasswordResetToken`                | Authentication state, sessions, 2FA, lockout |
| `Account`            | `Card`, `Transaction`                          | Bank account, balance, transaction ledger    |
| `Transfer`           | —                                              | Outgoing wire/SEPA transfer lifecycle        |
| `ForexTransaction`   | `LockedRate`                                   | Currency exchange between two accounts       |
| `RateAlert`          | —                                              | User-defined FX rate trigger                 |
| `BillPayment`        | —                                              | One-off bill payment to a biller             |
| `RecurringPayment`   | —                                              | Scheduled recurring bill payment             |
| `Beneficiary`        | —                                              | Saved recipient for transfers                |
| `SavedBiller`        | —                                              | Saved biller shortcut for bill payments      |

## Primitives

- **`Entity<TId>`** — base for all entities; holds a typed `Id`.
- **`AggregateRoot<TId>`** — extends `Entity<TId>`; adds a `DomainEvents` collection. Call `Raise(event)` inside aggregate methods to record what happened. The Application layer dispatches these events after persisting.
- **`ValueObject`** — base record for value objects; record equality gives structural comparison for free.

## Value objects

Custom value objects wrap primitives and enforce their invariants at construction time. All use a **private constructor + static factory** pattern so an invalid instance can never exist.

| Type             | Factory                         | Validates                             |
|------------------|---------------------------------|---------------------------------------|
| `Email`          | `Email.Create(string)`          | Non-empty, contains `@` and a domain  |
| `Iban`           | `Iban.Create(string)`           | Length 15–34, correct country prefix  |
| `HashedPassword` | `HashedPassword.Wrap(string)`   | Wraps a pre-hashed string (no-op)     |

## Errors

All domain errors live in `Common/Errors/` and are static `ErrorOr.Error` fields grouped by feature (`AccountErrors`, `TransferErrors`, etc.). Factory methods on aggregates and value objects return `ErrorOr<T>` so the caller is forced to handle both the success and failure paths without exceptions.

## Repositories

Interfaces only — one per aggregate root. Implementations live in `BankingApp.Infrastructure`. Each interface exposes only the queries actually needed by the domain and Application layer:

- `GetByIdAsync` on every repository
- `ListByUserIdAsync` where the aggregate is user-scoped
- `UpdateAsync` / `DeleteAsync` where mutation is required
- Specialised queries (`ListDueAsync` on recurring payments, `ListActiveAsync` on billers)

## Domain services

Stateless functions that perform cross-aggregate calculations or logic that does not naturally belong on a single aggregate. They take and return domain types only.

Currently: `IbanValidationService` (structural IBAN check used by the `Iban` value object).

## Domain events

Raised inside aggregate methods via `Raise(...)` and stored on the aggregate root until the Infrastructure layer clears them after a successful persistence. The Application layer picks them up and dispatches to handlers.

| Event                         | Raised by                            |
|-------------------------------|--------------------------------------|
| `BalanceUpdatedEvent`         | `Account.ChangeBalance`              |
| `TransactionRecordedEvent`    | `Account.RecordTransaction`          |
| `TransferExecutedEvent`       | (raised in Application after commit) |
| `TransferFailedEvent`         | (raised in Application after commit) |
| `BillPaymentProcessedEvent`   | (raised in Application after commit) |
| `RecurringPaymentsExecutedEvent` | (raised in Application after commit) |
| `ForexTransactionExecutedEvent`  | (raised in Application after commit) |
| `RateAlertTriggeredEvent`     | (raised in Application after commit) |
| `UserRegisteredEvent`         | (raised in Application after commit) |
| `UserLoggedInEvent`           | (raised in Application after commit) |
| `PasswordResetRequestedEvent` | (raised in Application after commit) |
