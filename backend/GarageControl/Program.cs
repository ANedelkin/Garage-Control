using GarageControl.Extensions;
using GarageControl.Core.Utilities;
using Microsoft.AspNetCore.HttpOverrides;
using PdfSharpCore.Fonts;

var builder = WebApplication.CreateBuilder(args);

// External Library Configuration
GlobalFontSettings.FontResolver = new EmbeddedFontResolver();

// Core Services
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddAuthorization();

// Project Extensions
builder.Services.AddApplicationDbContext(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddIdentityAndAuthentication(builder.Configuration);
builder.Services.ConfigureProjectCookies();

builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
    if (builder.Environment.IsDevelopment())
    {
        options.HttpsPort = 5173;
    }
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

// Middleware Pipeline
app.UseProjectMiddleware(app.Environment);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.MapFallbackToFile("index.html");

// Database Initialization
await app.Services.SeedDatabaseAsync();

app.Run();