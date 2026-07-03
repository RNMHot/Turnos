using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MudBlazor.Services;
using Turnos.Components;
using Turnos.Data;
using Turnos.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (builder.Environment.IsDevelopment() &&
    !string.IsNullOrWhiteSpace(connectionString) &&
    connectionString.Contains("YOUR_AZURE_SQL_SERVER", StringComparison.OrdinalIgnoreCase))
{
    connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=TurnosDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
}

builder.Services.AddDbContextFactory<AppDbContext>(o =>
    o.UseSqlServer(connectionString, sql =>
        sql.EnableRetryOnFailure(maxRetryCount: 5))
     .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

// ASP.NET Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account/login";
    options.LogoutPath = "/account/logout";
    options.AccessDeniedPath = "/account/login";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("AppAccess", policy =>
        policy.RequireClaim(TurnosClaimTypes.AppAccess, "true"));
});

// MudBlazor
builder.Services.AddMudServices();

// MVC controllers (for CalendarController and AccountController)
builder.Services.AddControllersWithViews();

// HTTP client for WhatsApp API
builder.Services.AddHttpClient();

// Required for IAntiforgery + IHttpContextAccessor in SSR login page
builder.Services.AddHttpContextAccessor();

// Application services
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<AccessControlService>();
builder.Services.AddScoped<IUserClaimsPrincipalFactory<IdentityUser>, TurnosUserClaimsPrincipalFactory>();
builder.Services.AddScoped<PersonService>();
builder.Services.AddScoped<CompanyService>();
builder.Services.AddScoped<LocationService>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<AssignmentService>();
builder.Services.AddScoped<AttendanceService>();
builder.Services.AddScoped<AvailabilityService>();
builder.Services.AddScoped<WhatsAppService>();
builder.Services.AddScoped<CalendarService>();
builder.Services.AddScoped<ExcelExportService>();
builder.Services.AddScoped<AppSettingService>();
builder.Services.AddSingleton<AppSettingsState>();

var app = builder.Build();

var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapControllers();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Run migrations and seed on startup
try
{
    await DbSeeder.SeedAsync(app.Services);
}
catch (Exception ex)
{
    startupLogger.LogError(ex,
    "Database initialization failed. Verify ConnectionStrings:DefaultConnection for the current environment.");
}

app.Run();
