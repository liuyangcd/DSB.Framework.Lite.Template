# AGENTS.md

## Build & Run

```pwsh
# Run the app (from repo root)
dotnet run --project .\SolutionName.HttpApi.Host\

# Build the full solution
dotnet build .\SolutionName.Backend.sln
```

No test project exists in this repo.

## EF Core Migrations

Run all EF commands from the **DbMigrations project**, NOT the Host project:

```pwsh
# Add a migration
dotnet ef migrations add <Name> --project .\SolutionName.EntityFrameworkCore.DbMigrations\ --startup-project .\SolutionName.EntityFrameworkCore.DbMigrations\

# Remove last migration
dotnet ef migrations remove --project .\SolutionName.EntityFrameworkCore.DbMigrations\ --startup-project .\SolutionName.EntityFrameworkCore.DbMigrations\

# Update database
dotnet ef database update --project .\SolutionName.EntityFrameworkCore.DbMigrations\ --startup-project .\SolutionName.EntityFrameworkCore.DbMigrations\

# Generate idempotent SQL script
dotnet ef migrations script -i -o .\script.sql --project .\SolutionName.EntityFrameworkCore.DbMigrations\ --startup-project .\SolutionName.EntityFrameworkCore.DbMigrations\
```

The `dotnet-ef` local tool is version 9.0.8 (configured in `.config/dotnet-tools.json`) but the project uses EF Core 8.0.26. The Migration.txt doc says the tool version must match the EF Core version. If `dotnet ef` fails, install the matching tool version.

The DbMigrations project has its own `appsettings.json` with a DB connection string used at design-time. The `DesignTimeSolutionNameContextFactory` reads this config.

## Architecture

ASP.NET Core 8 layered solution with 6 projects:

| Project | Purpose |
|---|---|
| `HttpApi.Host` | Web API entrypoint, controllers, startup |
| `Application` | Business logic services, Hangfire jobs |
| `Application.Contracts` | DTOs, BOs, JwtUserContext |
| `Domain` | Entities, enums (e.g. `RecordStatus`), option classes |
| `EntityFrameworkCore` | DbContext, repository impls, repository interfaces |
| `EntityFrameworkCore.DbMigrations` | EF Core migrations (design-time only) |

Dependency chain: `HttpApi.Host` -> `Application` -> `Application.Contracts` -> `EntityFrameworkCore` -> `Domain`

All projects target `net8.0` with nullable enabled.

## Key Framework (DSB.Framework.Lite) Conventions

- **Services** inherit from `SolutionNameApplicationService` (which implements `IBaseScopedService`). They are auto-registered as scoped services via `services.AddApplicationService<SolutionNameApplicationService>()`.
- **Repositories** inherit from `EntityFrameworkCoreRepository<SolutionNameContext, TEntity, TKey>` and are registered via `services.AddEntityFrameworkCoreRepository(typeof(SolutionNameContext))`.
- **Controllers** inherit from `ManagerControllerBase` (authenticated, derived from `JwtControllerBase<JwtUserContext, Guid>`) or `AnonymousControllerBase` (for unauthenticated endpoints).
- **Entities** inherit from `EntityBase<Guid>`.
- **Swagger API grouping** uses `[ApiExplorer(ModuleEnum.X)]` on controllers, wired via `typeof(ModuleEnum)` in `AddMySwagger`/`UseMySwaggerUI`.

## EF Core Entity Convention

Entity property defaults are set in the entity class itself (e.g. `public RecordStatus Status { get; set; } = RecordStatus.Normally;`), NOT via Fluent API `.HasDefaultValue()`. This avoids EF Core sentinel value issues where insert statements include all fields regardless of defaults.

## Middleware Order (Matters)

From `Program.cs`:
1. `UseForwardedHeaders`
2. `UseCors`
3. `UseGlobalExceptionMiddleware` (registered after request logging middleware if enabled)
4. `UseUnitOfWorkMiddleware<SolutionNameContext>`
5. `UseHealthChecks`
6. Swagger UI (non-Prod only)
7. `UseAuthorization`
8. `MapControllers`
9. Hangfire dashboard + jobs

## Configuration & Services

- **DB**: configured via `EntityFrameworkCoreOptions` section, supports MySql/SqlServer/Oracle/PostgreSQL
- **Redis**: `ConnectionStrings:Redis` for caching + distributed locks
- **JWT**: `JwtBearerSetting` section with PBKDF2 passwords
- **Hangfire**: `HangfireOptions` section with Redis storage
- **Swagger**: non-Prod only, JWT auth header
- **CORS**: optional via `CorsOptions`
- **File upload**: `FileUploadOptions` section, Kestrel/IIS/Form limits synced
- **Encrypted APIs**: `EncryptionApiOptionManager.Register()` with pre-shared AppId/SecretKey/Sm4Key
- **HttpClient**: `HttpClientOptions` section for named outgoing HTTP clients
- **ApiKey auth**: `ApiKeyOptions:Default` key

## Docker

Build and run via `Dockerfile.Https` (requires cert password env var). See `docker.txt` for exact commands.

## Placeholder Names

The entire solution uses `SolutionName` as a placeholder prefix. Renaming involves updating: project names, namespaces, solution file, Dockerfile entrypoint, and config section prefixes.

## Unit of Work (UoW) — 3 Rules for Saving to DB

EF Core's default transaction behavior is used: repositories do **NOT** immediately persist changes. Save timing depends on where your code runs:

### Rule 1 — API pipeline: automatic save

`UnitOfWorkMiddleware<SolutionNameContext>` wraps every HTTP request. After the controller/method completes, the middleware calls `SaveChangesAsync()` automatically. **You do nothing** — the middleware handles it.

```csharp
// In a controller action — no manual save needed
public async Task<ResultDto> CreateAsync(CreateDto input)
{
    await userRepository.InsertAsync(entity);  // no SaveChanges call
    return result;
}
// UnitOfWorkMiddleware calls SaveChangesAsync() after this returns
```

### Rule 2 — Outside the API pipeline: inject IUnitOfWork and save manually

Hangfire jobs, hosted services, console apps, etc. run **outside** the HTTP request scope, so the middleware does NOT apply. You must inject `IUnitOfWork<SolutionNameContext>` and call `await unitOfWork.SaveChangesAsync()` at the end.

Reference: `OneTimeJob.cs:46`, `CycleJob.cs:28`

```csharp
public class OneTimeJob : BackgroundJobBase<Guid>
{
    private readonly IUnitOfWork<SolutionNameContext> unitOfWork;

    public override async Task ExecuteAsync(Guid parameter, CancellationToken ct)
    {
        var user = await userRepository.GetAsync(x => x.Id == parameter, cancellationToken: ct);
        // ...
        await unitOfWork.SaveChangesAsync(ct); // REQUIRED — no middleware
    }
}
```

### Rule 3 — Multi-threading / fire-and-forget inside API pipeline: use IUnitOfWorkProvider

If you spawn new threads, `Task.Run`, or fire-and-forget work inside an API request, the DI scope (and all injected repositories + UoW) will be **disposed** when the HTTP request ends. Use `IUnitOfWorkProvider<SolutionNameContext>` to create an independent scope.

Reference: `DataSeedHostedService.cs:12-27`

```csharp
public class MyService : SolutionNameApplicationService
{
    private readonly IUnitOfWorkProvider<SolutionNameContext> unitOfWorkProvider;

    public async Task DoWorkInParallelAsync()
    {
        using var uowScope = unitOfWorkProvider.CreateScope();
        // Resolve services from the NEW scope — not from constructor injection
        var repo = uowScope.ServiceScope.ServiceProvider
            .GetRequiredService<IEntityFrameworkCoreRepository<SolutionNameContext, MyEntity, Guid>>();
        // ...
        await uowScope.Current.SaveChangesAsync();
    }
}
```

**Key class breakdown:**

| Class | Source | Purpose |
|---|---|---|
| `IUnitOfWork<TDbContext>` | NuGet `DSB.Framework.Lite.Data.EFCore.Repository` | Wraps `DbContext`; exposes `SaveChangesAsync()`, `BeginTransactionAsync()` |
| `IUnitOfWorkProvider<TDbContext>` | Same NuGet | Creates independent UoW scopes via `CreateScope()` |
| `UnitOfWorkScope<TDbContext>` | Same NuGet | Holds `IServiceScope` + `IUnitOfWork`; `Dispose()` cleans up the scope |
| `UnitOfWorkMiddleware<TDbContext>` | NuGet `DSB.Framework.Lite.Data.EFCore.Repository.WebApi` | Auto-saves after each HTTP request |

## Hangfire Job Conventions

- **Base classes** live in `SolutionName.Application.BackgroundJobs`: `BackgroundJobBase` (no parameter) and `BackgroundJobBase<T>` (with parameter). Both inherit from `SolutionNameApplicationService` and are auto-registered as scoped services.
- **Job classes** live under `SolutionName.Application.BackgroundJobs.Jobs`.
- **`CancellationToken`** is always accepted as the second parameter of `ExecuteAsync` and **must be propagated** to all async calls (`SaveChangesAsync(ct)`, repository methods with `cancellationToken: ct`, etc.). Hangfire automatically provides a cancellation token that triggers on server shutdown or job deletion.
- **Cancellation handling**: catch `OperationCanceledException` for graceful shutdown. Use `ct.ThrowIfCancellationRequested()` to manually check in loops or between steps.
- **`[JobDisplayName("描述")]`** on `ExecuteAsync` override gives readable names in the Hangfire dashboard.
- Reference: `OneTimeJob.cs` (one-time job with cancellation demo), `CycleJob.cs` (recurring job).

## Repository Operations — Delete & Query Conventions

### Logical Delete Only (No Physical Delete)

All delete operations should use **logical delete** (setting `EntityStatus`), never physical delete. Use the built-in soft-delete methods:
- `DeprecateAsync(entity)` / `DeprecateAsync(id)` / `DeprecateRangeAsync(ids|entities)` — marks entities as logically deleted (requires `SaveChangesAsync`)
- `ExecuteDeprecateAsync(id|ids|whereExpression)` — immediate logical delete (no `SaveChanges` needed)

Avoid physical delete methods unless absolutely necessary:
- `RemoveAsync(entity)` / `RemoveRangeAsync(ids|entities)` — physical delete (requires `SaveChangesAsync`)
- `ExecuteDeleteAsync(id|ids|whereExpression)` — immediate physical delete (no `SaveChanges` needed)

Default query methods (`GetListAsync`, `GetPagedListAsync`, `GetAsync`, `GetCountAsync`, etc.) are **pre-filtered** to exclude logically deleted records (`EntityStatus != deleted`).

### Repository Access Hierarchy

When accessing data through repositories, follow this priority:

| Access Point | Filtered | Use Case |
|---|---|---|
| **Default repository methods** | Yes (EntityStatus) | First choice — prefer built-in `GetAsync`, `GetListAsync`, `InsertAsync`, `UpdateAsync`, `DeprecateAsync` etc. |
| `Queryable` | Yes (EntityStatus) | Custom queries in repository impls (e.g. joins, projections) — safe, already excludes deleted records |
| `DbSet` | **No** | Raw EF Core access — **must manually add EntityStatus filter** if querying. Use only when `Queryable` is insufficient. |
| `Database` | **No** | Raw SQL execution via `ExecuteSqlRawAsync` or `FromSqlRaw` — for system-level SQL or ad-hoc queries not supported by repository methods. |

### When to Create Custom Repository Interface & Implementation

Custom repository interfaces + implementations are needed **only when a Service class injects the repository directly**. Decision rule:

- **Need custom interface/impl** (e.g. `IUserRepository` / `UserRepository`): Service layer needs to inject and call methods on this specific repository.
- **No custom interface/impl**: The repository is only used within other repositories or internally — inject the generic `IEntityFrameworkCoreRepository<SolutionNameContext, TEntity, TKey>` directly (e.g. junction tables like `SystemUserRole`).

Example of generic-only usage (no custom interface needed):

```csharp
// Inject the generic repository directly in a service
private readonly IEntityFrameworkCoreRepository<SolutionNameContext, SystemUserRoleEntity, Guid> userRoleRepository;
```

## Entity Update Strategy — Tracking Query vs. Execute Methods

When performing database updates, follow this priority order:

### Priority 1 (Preferred): Execute direct update — no entity query

When business logic does **not** need to inspect entity data before updating, use `ExecuteUpdateAsync` to issue a direct SQL UPDATE. No entity is loaded into memory, no change tracker is involved, and only the target columns are modified.

```csharp
// UpdateStatusAsync in UserService.cs:140
// No entity query needed — just flip a status field
return await userRepository.ExecuteUpdateAsync(
    inputDto.Id,
    x => x.SetProperty(y => y.Status, inputDto.Status));
```

Other direct-execute methods:
- `ExecuteDeleteAsync(id|ids|whereExpression)` — immediate physical delete
- `ExecuteDeprecateAsync(id|ids|whereExpression)` — immediate logical delete

### Priority 2: Tracking query — when entity data is needed for business logic

When business logic **requires** reading the entity first (validation, conditional updates, multi-field mapping), use a **tracking query** by passing `isTracking = true`. This lets EF Core's change tracker detect only the modified properties, so the generated UPDATE SQL includes **only the changed columns** — not the entire row.

```csharp
// UpdateAsync in UserService.cs:99
// Need to validate user exists + conditionally update password
var user = await userRepository.GetSingleAsync(inputDto.Id, true)
    ?? throw new BusinessException("用户不存在");

// TransObject only maps DTO properties that differ from entity values
inputDto.TransObject(user);

if (inputDto.PasswordText.IsNotNullOrEmpty())
{
    var (hashedPassword, salt) = passwordService.CreatePasswordHash(inputDto.PasswordText);
    user.Password = hashedPassword;
    user.Salt = salt;
}

await userRepository.UpdateAsync(user);
// Generated SQL: UPDATE ... SET Name=@p0, Phone=@p1, Password=@p2, Salt=@p3 WHERE Id=@p4
// Only modified columns appear — NOT a full-table UPDATE
```

**Key point**: Repository query methods have an `isTracking` overload parameter. Use `true` when you intend to modify the returned entity; use `false` (the default) for read-only queries to avoid change tracker overhead.

```csharp
// Read-only query — no tracking (default behavior)
var user = await userRepository.GetSingleAsync(id);                 // isTracking = false

// Query for update — tracking enabled
var user = await userRepository.GetSingleAsync(id, true);          // isTracking = true
```

## Code Style

- All public types and members have XML doc comments (`<summary>`)
- `GenerateDocumentationFile` is enabled on Host, Domain, EntityFrameworkCore, and Contracts projects
- Use file-scoped namespaces
- `required` keyword on entity properties that are non-nullable reference types
