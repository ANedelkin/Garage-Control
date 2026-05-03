using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using GarageControl.Infrastructure.Data;
using GarageControl.Infrastructure.Data.Models;
using GarageControl.Infrastructure.Data.Common;
using GarageControl.Infrastructure.Interceptors;
using GarageControl.Core.Contracts;
using GarageControl.Core.Services;
using GarageControl.Core.Services.Jobs;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

// Render provides connection strings as postgres:// URLs; Npgsql needs key=value format.
// Detect and convert automatically so local dev and Render both work.
var rawConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? throw new InvalidOperationException("No database connection string configured.");

string connectionString = rawConnectionString;
if (rawConnectionString.StartsWith("postgres://") || rawConnectionString.StartsWith("postgresql://"))
{
    var uri = new Uri(rawConnectionString);
    var userInfo = uri.UserInfo.Split(':');
    var username = Uri.UnescapeDataString(userInfo[0]);
    var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
    var database = uri.AbsolutePath.TrimStart('/');
    var port = uri.Port == -1 ? 5432 : uri.Port; // default PostgreSQL port if not in URL
    connectionString = $"Host={uri.Host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
}

builder.Services.AddDbContext<GarageControlDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services.AddScoped<IRepository, Repository>();

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IAdminService, AdminService>();

builder.Services.AddScoped<IDashboardService, DashboardService>();

builder.Services.AddScoped<IWorkshopService, WorkshopService>();
builder.Services.AddScoped<IJobTypeService, JobTypeService>();

builder.Services.AddScoped<IMakeService, MakeService>();
builder.Services.AddScoped<IModelService, ModelService>();

builder.Services.AddScoped<IWorkerService, WorkerService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();

builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IJobService, JobService>();

builder.Services.AddScoped<IPartService, PartService>();
builder.Services.AddScoped<IFolderService, FolderService>();
builder.Services.AddScoped<IDeficitService, DeficitService>();

builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();

builder.Services.AddScoped<IPDFGeneratorService, PDFGeneratorService>();
builder.Services.AddScoped<IExcelExportService, ExcelExportService>();
builder.Services.AddScoped<IPdfExportService, PdfExportService>();

builder.Services.AddHostedService<GarageControl.BackgroundServices.NotificationCleanupService>();
// builder.Services.AddHostedService<GarageControl.BackgroundServices.AvailabilityRecalculationService>();


builder.Services.AddIdentity<User, IdentityRole>(o => o.SignIn.RequireConfirmedAccount = false)
               .AddEntityFrameworkStores<GarageControlDbContext>()
               .AddDefaultTokenProviders();

var authBuilder = builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultSignInScheme = IdentityConstants.ExternalScheme;
});

// Only register Microsoft OAuth if all required config values are present
var msClientId = builder.Configuration["Microsoft:ClientId"];
var msClientSecret = builder.Configuration["Microsoft:ClientSecret"];
var msCallbackPath = builder.Configuration["Microsoft:CallbackPath"];
var msTenantId = builder.Configuration["Microsoft:TenantId"];
if (!string.IsNullOrEmpty(msClientId) && !string.IsNullOrEmpty(msClientSecret)
    && !string.IsNullOrEmpty(msCallbackPath) && !string.IsNullOrEmpty(msTenantId))
{
    authBuilder.AddOpenIdConnect("Microsoft", options =>
    {
        options.SignInScheme = IdentityConstants.ExternalScheme;
        options.ClientId = msClientId;
        options.ClientSecret = msClientSecret;
        options.CallbackPath = msCallbackPath;
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.Authority = $"https://login.microsoftonline.com/{msTenantId}/v2.0";
        options.ResponseType = "code";
    });
}

// Only register Google OAuth if all required config values are present
var googleClientId = builder.Configuration["Google:ClientId"];
var googleClientSecret = builder.Configuration["Google:ClientSecret"];
if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
        options.SignInScheme = IdentityConstants.ExternalScheme;
    });
}

authBuilder.AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateAudience = true,
        ValidateIssuer = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    o.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies["AccessToken"];
            return Task.CompletedTask;
        }
    };
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
    options.HttpsPort = 5173;
});

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Only redirect to HTTPS in development - Render handles SSL termination at the edge
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapFallbackToFile("index.html");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<GarageControlDbContext>();
        await context.Database.MigrateAsync();
        await GarageControl.Infrastructure.Data.Seeding.DbSeeder.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}

app.Run();