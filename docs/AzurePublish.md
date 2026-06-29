# Publishing Turnos to Azure

Turnos is already set up for Azure App Service + Azure SQL in production. The app runs migrations and seeds data on startup, so once the Azure connection string is correct the database will initialize itself.

## Recommended Azure setup

- Web hosting: Azure App Service
- Runtime stack: ASP.NET Core 9.0
- Database: Azure SQL Database
- App Service plan: Basic or higher so the app can stay warm and keep Identity sessions stable

## Required app settings

Set these in the App Service Configuration blade or as connection strings in the portal:

- `ConnectionStrings__DefaultConnection`
- `WhatsApp__AccessToken`
- `WhatsApp__PhoneNumberId`
- `CalendarToken__Secret`
- `ASPNETCORE_ENVIRONMENT=Production`

Use a full SQL Server connection string for production, for example:

```text
Server=tcp:<server-name>.database.windows.net,1433;Initial Catalog=TurnosDb;Persist Security Info=False;User ID=<sql-admin>;Password=<password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

## Fast publish path

1. Create an Azure SQL Database and an App Service web app in your subscription.
2. Configure the App Service settings above.
3. Publish the app from Visual Studio or VS Code using the App Service publish target.
4. Open the site and sign in with the seeded admin account:
   - Email: `admin@turnos.local`
   - Password: `Admin@123!`

## Notes

- Development uses SQLite, but production uses SQL Server by design.
- If the connection string is missing or invalid, the app will fail during startup because it applies EF Core migrations immediately.
- WhatsApp and calendar features are optional; if the secrets are empty, those features simply do not send messages.

## Troubleshooting generic "Error" page on Azure

If the site loads an error page with a request ID (without stack trace), check these first:

1. In App Service > Configuration > Application settings, set:
   - `ConnectionStrings__DefaultConnection`
   - `ASPNETCORE_ENVIRONMENT=Production`
2. Verify the SQL connection string is valid (server, database, user, password).
3. In Azure SQL, allow App Service outbound access in SQL firewall rules.
4. In App Service > Monitoring > Log stream, look for:
   - `Database initialization failed...`
   - `Could not build app-specific claims...`

These log entries indicate the app is running but cannot reach Azure SQL or authenticate with it.

## If you want CLI deployment later

Azure CLI is not installed in this workspace, so I could not run a direct subscription deploy from here. If you install it, the next step is usually to create the App Service, set the connection string, and deploy the published output or a zip package.