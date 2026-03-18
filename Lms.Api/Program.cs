using System.Text;
using Lms.Api.Data;
using Lms.Api.Extensions;
using Lms.Api.Middleware;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

// ── Core Services ───────────────────────────────────────
builder.Services.AddCoreServices(builder.Configuration);
builder.Services.AddDatabaseServices(builder.Configuration);
builder.Services.AddCacheServices(builder.Configuration);
builder.Services.AddBackgroundJobServices(builder.Configuration);

// ── Security ───────────────────────────────────────────────
builder.Services.AddAuthenticationServices(builder.Configuration);
builder.Services.AddRateLimitingServices();
builder.Services.AddCorsServices();

// ── Application Services ───────────────────────────────────
builder.Services.AddApplicationServices();
builder.Services.AddSwaggerServices();

var app = builder.Build();

// ── Middleware Pipeline ───────────────────────────────────
app.ConfigureMiddleware();

// ── Endpoints ───────────────────────────────────────────────
app = app.MapHealthEndpoints();
app.MapHangfireDashboard("/hangfire");
app.MapControllers();

// ── Database Initialization ───────────────────────────────
await app.InitializeDatabase();

app.Run();

public partial class Program { } // Make Program accessible for tests
