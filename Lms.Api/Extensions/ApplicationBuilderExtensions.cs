using Hangfire;
using Lms.Api.Data;
using Lms.Api.Middleware;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Routing;

namespace Lms.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    // ── Middleware Pipeline Configuration ─────────────────────────────────
    public static IApplicationBuilder ConfigureMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<GlobalExceptionMiddleware>();
        
        // Swagger enabled in all environments (for Railway demo)
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "LMS API v1");
            options.RoutePrefix = "swagger";
        });

        // app.UseHttpsRedirection(); // Disabled for Railway (handled by proxy)
        app.UseCors();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }

    // ── Health Endpoints ─────────────────────────────────────────────────
    public static WebApplication MapHealthEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false // No checks, just confirms app is running
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });

        app.MapHealthChecks("/health");

        return app;
    }

    // ── Database Initialization ───────────────────────────────────────────
    public static async Task InitializeDatabase(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        // Only migrate if not using InMemory database
        if (db.Database.IsRelational())
        {
            await db.Database.MigrateAsync();
        }
        
        await DbInitializer.SeedAsync(db);
    }
}
