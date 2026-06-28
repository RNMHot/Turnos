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

Run in Development environment:

```bash
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run
```

## Repository

GitHub: https://github.com/RNMHot/Turnos
