using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Turnos.Components;
using Turnos.Data;
using Turnos.Services;

var builder = WebApplication.CreateBuilder(args);

// Database — SQLite in dev, SQL Server in production
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<AppDbContext>(o =>
        o.UseSqlite(connectionString ?? "Data Source=turnos.db"));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(o =>
        o.UseSqlServer(connectionString));
}

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
builder.Services.AddAuthorizationCore();

// MVC controllers (for CalendarController and AccountController)
builder.Services.AddControllersWithViews();

// HTTP client for WhatsApp API
builder.Services.AddHttpClient();

// Required for IAntiforgery + IHttpContextAccessor in SSR login page
builder.Services.AddHttpContextAccessor();

// Application services
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<PersonService>();
builder.Services.AddScoped<CompanyService>();
builder.Services.AddScoped<LocationService>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<AssignmentService>();
builder.Services.AddScoped<AvailabilityService>();
builder.Services.AddScoped<WhatsAppService>();
builder.Services.AddScoped<CalendarService>();
builder.Services.AddScoped<ExcelExportService>();

var app = builder.Build();

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
await DbSeeder.SeedAsync(app.Services);

app.Run();
