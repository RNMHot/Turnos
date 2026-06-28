# Turnos

Turnos is a Blazor Server application for planning and managing event staffing (people/ushers), assignments, availability, companies, locations, audit logs, and exports.

## Tech Stack

- ASP.NET Core Blazor Server (.NET 9)
- Entity Framework Core
- SQLite (development)
- SQL Server (production)
- ASP.NET Core Identity

## Prerequisites

- .NET SDK 9.0+
- Visual Studio Community 2022 (17.10+) with ASP.NET and web development workload (optional)

## Open In Either IDE

- VS Code: open the folder and run `dotnet run` from the integrated terminal.
- Visual Studio Community: open `Turnos.sln` and start with F5 or Ctrl+F5.

You can switch back and forth between both IDEs. Keep using the same Git branch/repository and avoid committing user-specific IDE files (`.vs/`, `.vscode/`, `*.user`, `*.suo`).

## Getting Started

1. Restore dependencies:

   ```bash
   dotnet restore
   ```

2. Run the app:

   ```bash
   dotnet run
   ```

3. Open the URL shown in terminal (typically `https://localhost:xxxx`).

The app applies migrations and seeds initial data at startup.

## Configuration

- Development settings: `appsettings.Development.json`
- Default settings: `appsettings.json`
- Connection string key: `ConnectionStrings:DefaultConnection`

Behavior by environment:
- Development uses SQLite.
- Non-development uses SQL Server.

## Useful Commands

Build:

```bash
dotnet build
```

Build solution (Visual Studio compatible):

```bash
dotnet build Turnos.sln
```

Run in Development environment:

```bash
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run
```

## Repository

GitHub: https://github.com/RNMHot/Turnos
