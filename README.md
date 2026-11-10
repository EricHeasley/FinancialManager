# Financial Manager

A personal finance tracker built with **ASP.NET Core Blazor (.NET 8)**. Track bank accounts, credit cards,
loans and investment accounts in one place, see your net worth, and value stock holdings at live market prices.

## Features

- **Accounts** – checking, savings, cash, brokerage, retirement, credit card, loan and other accounts.
  Edit, archive/restore or delete accounts.
- **Transactions** – record deposits and withdrawals (payments and charges for credit cards and loans).
  Balances update automatically, and deleting a transaction reverses it.
- **Holdings** – add stock, ETF, fund or crypto positions to investment accounts. Buying more of a symbol you
  already hold merges the shares and recalculates the weighted average cost.
- **Live prices** – look up the current price of any ticker and see market value, today's change and
  unrealized gain or loss for every holding.
- **Dashboard** – net worth, total assets, total liabilities and investments at a glance.

## Getting started

Prerequisites: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
dotnet run --project src/FinancialManager.Web
```

Open the URL printed in the console. The SQLite database is created automatically at
`src/FinancialManager.Web/App_Data/financialmanager.db` on first run.

Run the tests:

```bash
dotnet test
```

## Architecture

The solution follows a layered (clean architecture) design. Dependencies point inward only.

```
src/
  FinancialManager.Domain          Entities and business rules (no dependencies)
  FinancialManager.Application     Use cases, read models, validation, abstractions
  FinancialManager.Infrastructure  EF Core + SQLite, Yahoo Finance client, caching, clock
  FinancialManager.Web             Blazor Server UI
tests/
  FinancialManager.UnitTests       Domain, application, persistence and HTTP client tests
```

| Concern | Approach |
| --- | --- |
| Business rules | `Account` is the aggregate root; all balance, transaction and holding changes go through it so invariants hold. |
| Error handling | Application services return `Result` / `Result<T>` for expected failures (validation, not found, provider unavailable) instead of throwing. |
| Validation | Data-annotation attributes on request models drive both UI form validation and server-side checks; the domain re-checks its own invariants. |
| Persistence | EF Core with SQLite, versioned migrations, Fluent API configurations, `IDbContextFactory` with a unit of work per operation (safe for long-lived Blazor circuits). |
| Market data | `IStockQuoteProvider` abstraction with a Yahoo Finance implementation, a caching decorator, and the standard HTTP resilience pipeline (retry, timeout, circuit breaker). |
| Configuration | All settings come from `appsettings.json` and are bound to strongly typed options that are validated at startup. |
| Testability | Time comes from `IClock`; external calls sit behind interfaces; persistence tests run against in-memory SQLite with the real schema. |

## Configuration

All settings live in `src/FinancialManager.Web/appsettings.json` and can be overridden with environment
variables (for example `Quotes__Cache__DurationSeconds=120`).

| Setting | Purpose |
| --- | --- |
| `ConnectionStrings:FinancialManager` | SQLite database location. |
| `Database:ApplyMigrationsOnStartup` | Apply pending migrations when the app starts. |
| `Display:CultureName` | Culture for number, date and currency formatting (for example `en-US`). Its currency is the base currency. |
| `Quotes:MaxConcurrentRequests` | Maximum parallel requests when pricing many holdings. |
| `Quotes:Cache:DurationSeconds` | How long a quote is reused before it is fetched again. |
| `Quotes:YahooFinance:*` | Endpoint, path, query and User-Agent for the quote provider. |
| `Quotes:Resilience:*` | Retry, timeout and circuit-breaker settings for quote requests. |

### Adding a database migration

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add <Name> \
  --project src/FinancialManager.Infrastructure \
  --output-dir Persistence/Migrations
```

## Notes and limitations

- **Single currency.** All balances are in the currency of the configured culture. Quotes are shown in the
  currency the exchange reports. Holdings priced in another currency are not converted.
- **Market data.** Prices come from Yahoo Finance's public chart endpoint, which needs no API key but is
  unofficial and may change or rate-limit. To use another provider, implement `IStockQuoteProvider` and
  register it in `Infrastructure/DependencyInjection.cs`.
- **Cash and trades.** Adding or selling shares does not move cash automatically. Record a transaction if
  the trade affected the account's cash balance.
- **Authentication.** The app is meant to run locally for a single user and has no sign-in.
  Add ASP.NET Core Identity before hosting it anywhere public.
