# SecureCadPortal --- Development Changelog

**Project:** SecureCadPortal\
**Workspace folder:** `Secure-CAD-Project-Portal`\
**Stack:** .NET 10, ASP.NET Core Web API, EF Core, Azure SQL, Git, VS
Code\
**Status:** Solution structure, Azure SQL persistence, Projects API endpoints, DTO validation, unit tests, and integration-test setup have been implemented. The migration was applied successfully. Re-run `dotnet build` and `dotnet test` to verify the current working tree before committing.

> This file documents the setup commands and work completed so far.
> Commands marked as checks are safe to rerun; setup commands generally
> only need to be run once.

------------------------------------------------------------------------

## 1. Install and verify tools

### Install .NET SDK

Download the .NET 10 SDK for macOS Apple Silicon (Arm64):
<https://dotnet.microsoft.com/en-us/download/dotnet/10.0>

Verify:

``` bash
dotnet --version
dotnet --info
```

### Install Git

On macOS, install Apple's command-line tools if needed:

``` bash
xcode-select --install
git --version
```

### Verify VS Code CLI

``` bash
code --version
```

Recommended VS Code extension: **C# Dev Kit** (Microsoft).

------------------------------------------------------------------------

## 2. Create the solution and projects

These commands were used to create the solution structure. Run them only
when creating a fresh workspace; do not rerun them in the existing
project folder.

``` bash
mkdir SecureCadPortal
cd SecureCadPortal

dotnet new sln -n SecureCadPortal
mkdir src tests

dotnet new classlib -n SecureCadPortal.Domain -o src/SecureCadPortal.Domain
dotnet new classlib -n SecureCadPortal.Application -o src/SecureCadPortal.Application
dotnet new classlib -n SecureCadPortal.Infrastructure -o src/SecureCadPortal.Infrastructure
dotnet new webapi -n SecureCadPortal.Api -o src/SecureCadPortal.Api
dotnet new xunit -n SecureCadPortal.UnitTests -o tests/SecureCadPortal.UnitTests
dotnet new xunit -n SecureCadPortal.IntegrationTests -o tests/SecureCadPortal.IntegrationTests
```

The actual workspace currently contains `SecureCadPortal.slnx` (the
newer solution format). Use that exact filename in solution commands.

### Add projects to the solution

``` bash
dotnet sln SecureCadPortal.slnx add src/SecureCadPortal.Domain/SecureCadPortal.Domain.csproj
dotnet sln SecureCadPortal.slnx add src/SecureCadPortal.Application/SecureCadPortal.Application.csproj
dotnet sln SecureCadPortal.slnx add src/SecureCadPortal.Infrastructure/SecureCadPortal.Infrastructure.csproj
dotnet sln SecureCadPortal.slnx add src/SecureCadPortal.Api/SecureCadPortal.Api.csproj
dotnet sln SecureCadPortal.slnx add tests/SecureCadPortal.UnitTests/SecureCadPortal.UnitTests.csproj
dotnet sln SecureCadPortal.slnx add tests/SecureCadPortal.IntegrationTests/SecureCadPortal.IntegrationTests.csproj
```

### Add project references

``` bash
dotnet add src/SecureCadPortal.Application reference src/SecureCadPortal.Domain

dotnet add src/SecureCadPortal.Infrastructure reference src/SecureCadPortal.Domain
dotnet add src/SecureCadPortal.Infrastructure reference src/SecureCadPortal.Application

dotnet add src/SecureCadPortal.Api reference src/SecureCadPortal.Application
dotnet add src/SecureCadPortal.Api reference src/SecureCadPortal.Infrastructure

dotnet add tests/SecureCadPortal.UnitTests reference src/SecureCadPortal.Domain
dotnet add tests/SecureCadPortal.UnitTests reference src/SecureCadPortal.Application
dotnet add tests/SecureCadPortal.IntegrationTests reference src/SecureCadPortal.Api
```

### Inspect and build

``` bash
dotnet sln SecureCadPortal.slnx list
dotnet build
dotnet test
```

The solution built successfully. The API was also started successfully
once:

``` bash
dotnet run --project src/SecureCadPortal.Api
```

It listened on `http://localhost:5190` at that time. The port can
change; use the `Now listening on:` URL printed by the application. Stop
it with `Control+C`.

------------------------------------------------------------------------

## 3. Git and `.gitignore`

A Git repository already existed and had an initial commit and an
`origin/main` remote.

Create `.gitignore` in the workspace root. The important rules include:

``` gitignore
# .NET build output
**/bin/
**/obj/
artifacts/

# Test output
**/TestResults/
**/coverage/

# IDE
.vs/
.idea/
.vscode/*
!.vscode/extensions.json
!.vscode/launch.json
!.vscode/tasks.json
!.vscode/settings.json

# macOS
.DS_Store
.AppleDouble
._*

# Secrets
.env
.env.*
!.env.example
secrets.json
appsettings.*.local.json

# Node / Angular (future frontend)
node_modules/
dist/
.angular/
*.tsbuildinfo

# Local databases and certificates
*.db
*.sqlite
*.sqlite3
*.pfx
*.p12

# Logs and temporary files
*.log
tmp/
temp/
*.tmp
*.temp
```

Because generated files had already been tracked, the following commands
were used to remove ignored files from Git's index and then stage the
remaining project files:

``` bash
git rm -r --cached .
git add .
git status --short
git diff --cached --stat
```

`git rm --cached` removes files from Git tracking but does not delete
them from the computer. Review staged changes carefully before
committing, especially configuration files, and make sure no passwords
or credentials are staged.

Suggested commit message:

``` bash
git commit -m "Set up SecureCadPortal solution and Azure SQL persistence"
```

Check repository status:

``` bash
git status
git log -1 --oneline
```

Only commit after reviewing the staged files. The final commit may still
be pending.

------------------------------------------------------------------------

## 4. Azure SQL database

An Azure SQL logical server and database were created.

-   **Server:** `sqltester1992free.database.windows.net`
-   **Database:** `free-sql-db-5954153`
-   **SQL login:** `sqladminuser`
-   **Region:** Germany West Central

Do not put the SQL password in this file, source code,
`appsettings.json`, or Git. The password was reset after an initial
login failure.

The server firewall initially blocked the client's public IP (SQL error
40615). A rule was added for the current client IP. If the IP changes,
update the firewall rule; do not allow all IPs.

An existing `Products` table was created manually in this database. Its
schema was:

``` sql
CREATE TABLE Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Price DECIMAL(10,2) NOT NULL,
    Stock INT NOT NULL DEFAULT 0
);
```

**Important:** Keep the existing `Products` table. The new EF Core
migration is intended to add `Projects`, not alter `Products`.

------------------------------------------------------------------------

## 5. EF Core and database connection

Install the SQL Server provider and EF Core design package:

``` bash
dotnet add src/SecureCadPortal.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer
dotnet add src/SecureCadPortal.Api package Microsoft.EntityFrameworkCore.Design
```

Install or update the EF Core CLI tool:

``` bash
dotnet tool install --global dotnet-ef
# If already installed, use this instead:
dotnet tool update --global dotnet-ef
```

Initialize .NET User Secrets for the API:

``` bash
dotnet user-secrets init --project src/SecureCadPortal.Api
```

Set the connection string as a local secret. Replace `YOUR_SQL_PASSWORD`
locally; never commit the actual password:

``` bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=tcp:sqltester1992free.database.windows.net,1433;Initial Catalog=free-sql-db-5954153;Persist Security Info=False;User ID=sqladminuser;Password=YOUR_SQL_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" --project src/SecureCadPortal.Api
```

List secret keys/values only in your local terminal (do not paste output
if it exposes credentials):

``` bash
dotnet user-secrets list --project src/SecureCadPortal.Api
```

The connection string was configured in `Program.cs` to register
`AppDbContext` with `UseSqlServer(...)`. The required pattern is:

``` csharp
using Microsoft.EntityFrameworkCore;
using SecureCadPortal.Infrastructure.Data;

// Before builder.Build():
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not configured.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
```

Keep any existing registrations and middleware in `Program.cs`.

------------------------------------------------------------------------

## 6. Domain entity and EF Core context

The intended files are:

### `src/SecureCadPortal.Domain/Entities/Project.cs`

``` csharp
namespace SecureCadPortal.Domain.Entities;

public class Project
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Status { get; set; } = "Active";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
```

### `src/SecureCadPortal.Infrastructure/Data/AppDbContext.cs`

``` csharp
using Microsoft.EntityFrameworkCore;
using SecureCadPortal.Domain.Entities;

namespace SecureCadPortal.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();
}
```

------------------------------------------------------------------------

## 7. Create and apply the migration

Create the migration:

``` bash
dotnet ef migrations add InitialProjects \
  --project src/SecureCadPortal.Infrastructure \
  --startup-project src/SecureCadPortal.Api \
  --output-dir Data/Migrations
```

The migration was generated successfully.

Review its `Up()` method before applying it. It should create the
`Projects` table and must not drop or alter the existing `Products`
table.

Apply the migration:

``` bash
dotnet ef database update \
  --project src/SecureCadPortal.Infrastructure \
  --startup-project src/SecureCadPortal.Api
```

The database update initially failed due to: 1. Azure SQL firewall error
40615. 2. SQL login failure for `sqladminuser`.

After the firewall and password were corrected, the user confirmed that
the update succeeded.

Verify the tables in the Azure SQL connection in VS Code:

``` sql
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME IN ('Products', 'Projects');
```

Expected: both `Products` and `Projects`.

------------------------------------------------------------------------

## 8. Projects API and request validation

### `CreateProjectRequest`

File: `src/SecureCadPortal.Application/Projects/CreateProjectRequest.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace SecureCadPortal.Application.Projects;

public class CreateProjectRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(Active|Archived)$")]
    public string Status { get; set; } = "Active";
}
```

The request DTO keeps API input separate from the database entity. Data annotations express basic validation rules: name is required and 3–100 characters, description is at most 1,000 characters, and status must be `Active` or `Archived`.

### `ProjectsController`

File: `src/SecureCadPortal.Api/Controllers/ProjectsController.cs`

The controller uses constructor injection for `AppDbContext` and implements:

- `GET /api/projects` — retrieve projects.
- `GET /api/projects/{id:int}` — retrieve one project by ID.
- `POST /api/projects` — validate input, create a project, save it, and return `201 Created` with a location for the new resource.

The POST action maps the request DTO to a `Project` entity, trims text fields, sets `CreatedAtUtc` using `DateTime.UtcNow`, calls `SaveChangesAsync()`, and returns `CreatedAtAction(...)`.

`Program.cs` includes `builder.Services.AddControllers()` and `app.MapControllers()`. It also includes `public partial class Program { }` so the integration-test project can create a `WebApplicationFactory<Program>` test host.

### Try the endpoints

Start the API in one terminal:

```bash
dotnet run --project src/SecureCadPortal.Api
```

Use the actual local URL printed by the app (the API previously listened on `http://localhost:5190`; the port may differ).

```bash
curl -i http://localhost:5190/api/projects
```

Create a project:

```bash
curl -i -X POST http://localhost:5190/api/projects \
  -H "Content-Type: application/json" \
  -d '{
    "name": "CAD Integration Project",
    "description": "Engineering document management pilot",
    "status": "Active"
  }'
```

Then run the GET request again. A successful POST should return `201 Created`. If the local port differs, replace `5190` with the port printed by `dotnet run`.

------------------------------------------------------------------------

## 9. Dependency Injection and AppDbContext

### Dependency Injection (DI)

DI means a class receives the services it needs rather than creating those services itself. In this project, `ProjectsController` declares its dependency in the constructor:

```csharp
private readonly AppDbContext _context;

public ProjectsController(AppDbContext context)
{
    _context = context;
}
```

ASP.NET Core resolves `AppDbContext` from its dependency-injection container. This reduces coupling and makes the controller easier to test.

### Registering the database context

The API registers the EF Core context using the SQL Server provider and the local User Secrets connection string. The standard configuration pattern is:

```csharp
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));
}
```

**Configuration note:** this conditional registration was introduced so the test host could start without requiring the Azure SQL secret. For a production-ready setup, prefer making the behavior explicit by environment: skip SQL Server registration only in `Testing`, and require a valid connection string in Development/Production. The integration-test factory should replace the SQL Server context registration with its test provider.

`AddDbContext` uses the `Scoped` lifetime by default, normally giving each HTTP request its own context. `DbContext` is not thread-safe and should not be shared across concurrent operations.

### What AppDbContext does

`AppDbContext` inherits from EF Core's `DbContext` and exposes `DbSet<Project> Projects`. EF Core translates LINQ queries such as `await _context.Projects.ToListAsync()` into database queries. Calling `_context.Projects.Add(project)` marks an entity for insertion; `await _context.SaveChangesAsync()` persists pending changes.

------------------------------------------------------------------------

## 10. Unit and integration tests

### Unit tests

File: `tests/SecureCadPortal.UnitTests/Projects/CreateProjectRequestTests.cs`

Four validation scenarios were added for the request DTO: valid input, too-short name, invalid status, and description longer than 1,000 characters. The tests use `Validator.TryValidateObject` to exercise the Data Annotations rules.

### Integration tests

File: `tests/SecureCadPortal.IntegrationTests/Projects/ProjectsApiTests.cs`

The integration-test project uses `Microsoft.AspNetCore.Mvc.Testing` and `Microsoft.EntityFrameworkCore.InMemory`. A `ProjectsApiFactory : WebApplicationFactory<Program>` configures a test host and replaces the normal EF Core database registration with an InMemory provider named `ProjectsApiTests`.

Scenarios added:

- `PostProject_ThenGetProjects_ReturnsSavedProject` — POST a project and verify it appears in the collection GET response.
- `PostProject_WithShortName_ReturnsBadRequest` — verify invalid input is rejected.

Troubleshooting recorded during implementation:

1. The test host initially failed because `DefaultConnection` was missing. The application startup configuration was adjusted to allow the test host to start without the Azure SQL secret; make the environment-specific configuration stricter before production use.
2. The test host initially registered both SQL Server and InMemory EF Core providers. The factory now removes the existing `AppDbContext`, `DbContextOptions<AppDbContext>`, and `IDbContextOptionsConfiguration<AppDbContext>` registrations before adding InMemory.
3. The POST returned `201 Created`, but the following GET did not see the new project when each test factory used a different database name. Using the stable database name `ProjectsApiTests` resolved that issue for the current factory/test setup.
4. A warning about not determining the HTTPS port for redirect may appear in tests. It is separate from the endpoint assertions; configure or disable HTTPS redirection for the Testing environment if desired.

Run all tests from the repository root:

```bash
dotnet build
dotnet test
```

Run only unit tests:

```bash
dotnet test tests/SecureCadPortal.UnitTests/SecureCadPortal.UnitTests.csproj
```

Run only integration tests:

```bash
dotnet test tests/SecureCadPortal.IntegrationTests/SecureCadPortal.IntegrationTests.csproj
```

The most recent conversation checkpoint reported that the integration-test fix was working, but a fresh full `dotnet test` run is still recommended to confirm the current state.

------------------------------------------------------------------------

## 11. Git review and checkpoint

Before committing, inspect the staged and unstaged changes and verify that no secrets are included:

```bash
git status --short
git diff --stat
git diff --cached --stat
git diff --cached
```

Do not print or paste the actual connection-string secret. Confirm that `appsettings.json` and other tracked files contain no passwords or access tokens. User Secrets should remain local and outside Git.

Suggested commit message for the API and tests, if these changes are not committed yet:

```bash
git add .
git commit -m "Add Projects API and integration tests"
```

Do not assume that this commit has been created; check `git log -1 --oneline` first. Do not push unless you intend to publish the changes to `origin/main`.

------------------------------------------------------------------------

## 12. Current status and next steps

### Implemented so far

- .NET 10 solution and six projects created.
- Project references configured for Domain, Application, Infrastructure, API, UnitTests, and IntegrationTests.
- `.gitignore` added and generated build artifacts removed from the Git index.
- Azure SQL server/database created; local connection string configured with User Secrets.
- `Project` domain entity and `AppDbContext` added.
- `InitialProjects` EF Core migration generated and successfully applied to Azure SQL.
- Existing `Products` table is intended to be preserved; migration should only add `Projects`.
- `CreateProjectRequest` DTO and Data Annotations validation added.
- Projects API GET collection, GET by ID, and POST endpoints added.
- Four DTO validation unit-test scenarios and two API integration-test scenarios added.
- Integration tests configured to use EF Core InMemory instead of Azure SQL.

### Recommended next steps

1. Run `dotnet build` and `dotnet test` and resolve any failures.
2. Remove temporary `Console.WriteLine` diagnostics from integration tests if they are still present; keep useful assertion messages where appropriate.
3. Refine `Program.cs` so a missing connection string is tolerated only in the `Testing` environment and is a clear startup error in Development/Production.
4. Review `git status`, staged diffs, and `.gitignore`; confirm no secrets are tracked.
5. Commit the verified API/test changes if they are not already committed.
6. Refactor business logic from the controller into an Application service/use-case layer, keeping the controller thin.
7. Add stronger integration tests against SQL Server where database-specific behavior matters; EF Core InMemory is not a full SQL Server substitute.
8. Add authentication and authorization before exposing the API beyond local development.
9. Add logging, centralized error handling, and API documentation as appropriate.
10. Build the Angular frontend and later configure CI/CD.

> This changelog records the state described in the development conversation. It does not claim that a fresh build/test run, a Git commit, authentication, or production hardening has been completed unless explicitly verified.
