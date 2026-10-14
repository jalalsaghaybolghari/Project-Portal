# SecureCadPortal --- Development Changelog

**Project:** SecureCadPortal\
**Workspace folder:** `Secure-CAD-Project-Portal`\
**Stack:** .NET 10, ASP.NET Core Web API, EF Core, Azure SQL, Git, VS
Code\
**Status:** Solution builds; EF Core migration applied to Azure SQL. The
Projects API endpoints still need to be added/tested.

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

## 8. Current status and next steps

### Completed

-   .NET solution and six projects created.
-   Project references configured.
-   Build succeeded.
-   `.gitignore` added.
-   Azure SQL server and database created.
-   EF Core provider/design package configured.
-   Local User Secrets connection string configured.
-   `AppDbContext` registered.
-   `InitialProjects` migration generated and successfully applied (as
    confirmed by the user).

### Still to do

1.  Confirm that `Project.cs` and `AppDbContext.cs` exist and match the
    code above.
2.  Confirm the `Projects` table exists in Azure SQL.
3.  Create `src/SecureCadPortal.Api/Controllers/ProjectsController.cs`.
4.  Add `builder.Services.AddControllers();` before `builder.Build()`
    and `app.MapControllers();` after it in `Program.cs`, preserving
    other setup.
5.  Build and run the API.
6.  Test `GET /api/projects` and `POST /api/projects`.
7.  Replace direct entity binding with DTOs and validation.
8.  Add meaningful unit and integration tests.
9.  Implement authentication and resource-level authorization.
10. Build the Angular frontend and add CI/CD.

### Future API tests

Once the controller is implemented and the API is running, use the
actual port printed by `dotnet run`.

``` bash
curl -i http://localhost:5190/api/projects
```

Create a test project:

``` bash
curl -i -X POST http://localhost:5190/api/projects \
  -H "Content-Type: application/json" \
  -d '{
    "name": "CAD Integration Project",
    "description": "Engineering document management pilot",
    "status": "Active"
  }'
```

Then run GET again and verify the row in Azure SQL.

> The controller and endpoint commands above are the next planned steps,
> not confirmed as completed yet. The project should not be considered
> production-secure until authentication, authorization, validation, and
> tests are implemented.
