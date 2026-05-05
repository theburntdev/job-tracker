# Backend — C# / .NET

See root `CLAUDE.md` for project vocabulary and cross-cutting rules.

## Tech choices (decided)
- **Framework**: ASP.NET Core minimal APIs (not controller-based)
- **Language**: C# 13, .NET 10
- **ORM**: Entity Framework Core with SQLite (swap to Postgres later via connection string only)
- **Validation**: FluentValidation
- **Mapping**: Mapperly (source-generated, compile-time safe — no AutoMapper)
- **Logging**: Serilog (configured in `JobTracker.Api` host only — all other projects inject `ILogger<T>`, never reference Serilog directly)
- **OpenAPI**: `Microsoft.AspNetCore.OpenApi` (built-in, no Swashbuckle)
- **Testing**: xUnit + Testcontainers (integration) + NSubstitute (mocks where needed with strict behavior)

## Solution structure (target — not yet created)
```
backend/
  JobTracker.sln
  src/
    JobTracker.Api/             # ASP.NET Core host, minimal API endpoints
    JobTracker.Application/     # CQRS commands, queries, handlers, pipeline behaviors, repo interfaces
    JobTracker.Domain/          # Entities, value objects, domain logic — no EF or framework references here
    JobTracker.Infrastructure/  # EF DbContext, migrations, repo implementations, external services
  tests/
    JobTracker.Api.Tests/
    JobTracker.Domain.Tests/
    JobTracker.Application.Tests/
    JobTracker.Infrastructure.Tests/
    JobTracker.Testing.Common/   # Shared builders, recipes, fixtures — no production code
```

## Dependency direction
```
Api → Application → Domain
Infrastructure → Application  (implements interfaces defined in Application)
```
`Domain` has zero references to any other project in this solution.

## Essential commands
```powershell
# Build
dotnet build backend/JobTracker.sln

# Run API (hot reload)
dotnet watch --project backend/src/JobTracker.Api

# Run all tests
dotnet test backend/JobTracker.sln

# Run a single test project
dotnet test backend/tests/JobTracker.Api.Tests

# Add a migration
dotnet ef migrations add <MigrationName> --project backend/src/JobTracker.Infrastructure --startup-project backend/src/JobTracker.Api

# Apply migrations
dotnet ef database update --project backend/src/JobTracker.Infrastructure --startup-project backend/src/JobTracker.Api
```

## API style
- RESTful resource URLs — nouns, not verbs (e.g., `GET /jobs`, `POST /jobs`, `PATCH /jobs/{id}`)
- Standard HTTP verbs: `GET` read, `POST` create, `PUT` full replace, `PATCH` partial update, `DELETE` remove
- 4xx/5xx error bodies follow RFC 7807 Problem Details (`application/problem+json`)
- `201 Created` with `Location` header on successful resource creation
- `204 No Content` on successful delete
- Collection endpoints return arrays wrapped in a paged envelope: `{ "items": [...], "total": n, "page": n, "pageSize": n }`
- Pagination via query params: `?page=1&pageSize=20` (1-based, offset model)
- `PATCH` uses JSON Merge Patch (RFC 7396) — send only changed fields, omitted fields unchanged
- Authentication deferred — do not add auth middleware, JWT, or authorization attributes until explicitly tasked

## Code conventions
- Endpoints live in `JobTracker.Api` as extension methods that call `app.Map*`.
- Use MediatR for command/query dispatch
- Domain entities are plain C# classes with no EF attributes — use fluent config in `Infrastructure` via one `IEntityTypeConfiguration<TEntity>` class per entity.
- Entity primary keys are strongly-typed IDs defined as `record struct` in `JobTracker.Domain.Common`:
  ```csharp
  record struct JobId(Guid Value);
  record struct ApplicationId(Guid Value);
  ```
  Never use raw `Guid` or `int` as entity ID types in domain or application code.
- Return `TypedResults.*` from endpoints, not raw status codes.
- Use `record` types for DTOs (request/response shapes).
- Never return domain entities from endpoints — always map to a response DTO via a Mapperly mapper.
- Mappers live in `JobTracker.Api` or `JobTracker.Application` (never in `Domain`).
- Async all the way down — every method touching I/O returns `Task<T>`.
- Nullable reference types enabled (`<Nullable>enable</Nullable>`) — no `#nullable disable`, no `!` suppression without a comment explaining why.
- Never throw exceptions for expected domain errors; use `Result<T>` (defined in `JobTracker.Domain.Common`) or `IResult` directly.
  ```csharp
  // JobTracker.Domain.Common.Result<T>
  Result<T>.Success(T value)
  Result<T>.Failure(string error, ErrorKind kind = ErrorKind.Validation)
  bool IsSuccess / bool IsFailure
  T Value          // throws if failure
  string Error     // throws if success
  ErrorKind Kind   // throws if success

  enum ErrorKind { Validation, NotFound, Conflict }
  // ToHttpResult() maps: Validation → 422, NotFound → 404, Conflict → 409
  ```
- Endpoints unwrap `Result<T>` via `result.ToHttpResult()` extension method in `JobTracker.Api` — never inline `.IsSuccess` branches in endpoint bodies.
- Every handler method and every repository interface method accepts `CancellationToken ct = default` as the last parameter and forwards it to all async calls.
- Repository interfaces use a generic base `IRepository<T, TId>` in `JobTracker.Application` — entity-specific interfaces extend it only to add queries beyond basic CRUD.
  ```csharp
  // JobTracker.Application.Common (not Domain — pagination is an application concern)
  record Page<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);

  interface IRepository<T, TId> where T : class
  {
      Task<T?> GetByIdAsync(TId id, CancellationToken ct = default);
      Task<Page<T>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
      Task AddAsync(T entity, CancellationToken ct = default);
      void Update(T entity);
      void Delete(T entity);
  }

  // Entity-specific — only add what CRUD can't cover
  interface IJobRepository : IRepository<Job, JobId>
  {
      Task<Page<Job>> GetByStageAsync(Stage stage, int page, int pageSize, CancellationToken ct = default);
  }

  // Defined in JobTracker.Application, implemented in JobTracker.Infrastructure via DbContext
  // Inject to commit across multiple repositories atomically
  interface IUnitOfWork
  {
      Task<int> SaveChangesAsync(CancellationToken ct = default);
  }
  ```
- `page` is 1-based at the API layer — repository implementations translate to 0-based EF `Skip()` via `(page - 1) * pageSize`.
- Global exception handler implemented as `IExceptionHandler` in `JobTracker.Api` — maps `ValidationException` → 422, unhandled exceptions → 500 Problem Details.

## Application layer structure
Commands, queries, handlers, and validators are co-located by feature:
```
Application/
  Jobs/
    CreateJob/
      CreateJobCommand.cs
      CreateJobCommandHandler.cs
      CreateJobCommandValidator.cs
    GetJob/
      GetJobQuery.cs
      GetJobQueryHandler.cs
```
No separate `Validators/` folder — validator lives in same folder as its command/query.

## MediatR pipeline behaviors (in `JobTracker.Application`, registered in `JobTracker.Api`)
- `ValidationBehavior<TRequest, TResponse>` — runs FluentValidation before the handler; throws `ValidationException` on failure, which a global exception handler maps to 422 Problem Details.
- `LoggingBehavior<TRequest, TResponse>` — logs request name and elapsed time via `ILogger<T>`.

## Test data builders (in `JobTracker.Testing.Common`)
- One builder per domain entity, named `<Entity>Builder` (e.g., `JobBuilder`, `ApplicationBuilder`).
- Each `WithX(...)` method sets one property and returns `this` for chaining.
- `Build()` returns a fully constructed, valid domain entity.
- Recipes are static factory methods on the builder that return a preconfigured builder for a named scenario:
  ```csharp
  // Generic
  new JobBuilder().WithTitle("Dev").WithStage(Stage.Applied).Build();

  // Recipe
  JobBuilder.WithOffer().Build();
  ApplicationBuilder.Rejected().WithNote("No feedback").Build();
  ```
- Default values in constructors must produce a valid entity — recipes override only what the scenario needs.

## What NOT to do
- Do not put business logic in endpoints or DbContext — it belongs in Domain.
- Do not use `[ApiController]` or MVC controllers — minimal APIs only.
- Do not seed test data via migrations — use test fixtures or DbContext seeding in tests.
- Do not construct domain entities directly in tests — use builders from `JobTracker.Testing.Common`.
- Do not call `SaveChangesAsync` on repositories — call it via injected `IUnitOfWork` only.
- Do not use raw `Guid` or `int` as entity ID types — use the strongly-typed ID `record struct` from `JobTracker.Domain.Common`.

## Environment / secrets
- Connection string key: `ConnectionStrings:Default`
- Store locally with: `dotnet user-secrets set "ConnectionStrings:Default" "..."` inside `JobTracker.Api`
- Never commit `appsettings.Development.json` with real values.
