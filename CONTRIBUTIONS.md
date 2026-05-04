# Contributing

## Development setup

1. Install the .NET 10 SDK.
2. Install Docker Desktop if you need to run the API with its local dependencies (other ways exist, but I find this to be the easiest).
3. Use Windows 10/11 and the `x64` platform when running the WinUI desktop client.
4. From `BankingApp`, generate local development configuration:

```powershell
python scripts/secrets/setup_dev.py
```

5. Start the local API container if needed:

```powershell
docker compose up --build
```

6. Open `BankingApp/BankingApp.slnx` in Visual Studio/Rider or build it using the `dotnet cli`.

## Running tests

Run all tests from the repository root:

```powershell
dotnet test BankingApp\BankingApp.slnx
```

Run a focused test project while developing:

```powershell
dotnet test BankingApp\tests\BankingApp.Application.Tests\BankingApp.Application.Tests.csproj
dotnet test BankingApp\tests\BankingApp.Api.Tests\BankingApp.Api.Tests.csproj
dotnet test BankingApp\tests\BankingApp.Desktop.Tests\BankingApp.Desktop.Tests.csproj
```

Integration tests may require Docker services and local configuration to be running. Do not merge changes that leave failing tests, StyleCop warnings, or analyzer warnings in touched projects.
Also there are buttons in IDE's for running tests, commands are not necessarly needed.

## Code style

StyleCop is the source of truth for baseline C# formatting, ordering, naming, and documentation rules. 
`BankingApp/Directory.Build.props` treats warnings as errors and applies `BankingApp/StyleCop.ruleset`, `BankingApp/stylecop.json`, and `BankingApp/.editorconfig`. 
If this document and StyleCop conflict, either follow StyleCop or update the StyleCop configuration in the same pull request with a clear reason.

1. Use PascalCase for types, methods, properties, events, enum values, constants, and public fields.
2. Use camelCase for local variables and method parameters.
3. Prefix private instance fields with `_` and use camelCase after the prefix, for example `_apiClient`.
4. Prefix interface names with `I`, for example `IApiClient`.
5. Use file-scoped namespaces and place `using` directives outside the namespace.
6. Order `using` directives alphabetically, with `System` namespaces first.
7. Keep one public type per `.cs` file, and name the file after that type.
8. Always declare access modifiers explicitly.
9. Keep fields private, exceptions can occur (framework or contract requierments).
10. Prefer `readonly` fields for dependencies assigned in constructors.
11. Use dependency injection instead of constructing services, repositories, clients, loggers, or configuration objects inside application code.
12. Keep controllers thin: validate transport concerns, call application services, and map responses without embedding business rules.
13. Keep domain and application logic independent from API, desktop, database, and UI framework details.
14. Return `ErrorOr<T>` across application service boundaries when an operation can fail in an expected way.
15. Use async APIs for I/O-bound work and name asynchronous methods with the `Async` suffix.
16. Pass `CancellationToken` through public async APIs when the caller can reasonably cancel the operation.
17. Do not use `async void` except for UI event handlers required by WinUI.
18. Prefer `string.Empty` over `""` for empty strings.
19. Use braces for all `if`, `else`, `for`, `foreach`, `while`, and `using` blocks.
20. Add XML documentation to public APIs when the type or member is part of a cross-project contract.
21. Write comments only when they explain intent, business rules, non-obvious tradeoffs, or external constraints.
22. In XAML, use clear `x:Name` values for elements referenced from code-behind, keep bindings explicit, and use observable state (`INotifyPropertyChanged` or observable collections) when UI data changes after load.

## Branch naming and Commit messages

Use Conventional Commits. The subject format is:

```text
type(optional-scope): short imperative summary
```

Common types include `feat`, `fix`, `test`, `docs`, `refactor`, `style`, `chore`, `build`, and `ci`. 
Use a scope when it clarifies the affected area, such as `api`, `desktop`, `auth`, `beneficiaries`, or `tests`.

Examples:

```text
feat(beneficiaries): add desktop navigation entry
fix(api): return not found for missing accounts
test: cover password reset validation
docs: add contribution guidelines
```

Keep the first line focused and under roughly 72 characters.
Add a body when the change needs context, such as migration details, tradeoffs, or follow-up work.
Reference issues or PRs when relevant.

Branch names should use the same type vocabulary and a short kebab-case description:

```text
type/something-something
```

Examples:

```text
feat/beneficiaries-page
fix/login-token-refresh
test/account-service-validation
docs/contribution-guidelines
```

## Pull request checklist

Before requesting review, confirm:

- The PR scope matches its issue or clearly explains any intentional scope change.
- Code follows the rules in this document and passes StyleCop/analyzer checks.
- New or changed behavior is covered by focused tests.
- Existing relevant tests were run, and any tests not run are called out in the PR.
- API changes include request/response DTOs, validation behavior, and error mappings.
- Desktop UI changes are reachable through navigation and update correctly after data changes.
- Async code handles expected failures without unobserved exceptions or deadlocks.
- Public contracts have useful names and XML documentation where appropriate.
- No secrets, local-only settings, generated build output, or unrelated files are included.
- The PR description explains what changed, how it was tested, and any known limitations.
