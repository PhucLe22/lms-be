using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;
using Testcontainers.PostgreSql;
using Npgsql;
using Respawn;
using Lms.Api.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace Lms.IntegrationTests;

public class LmsWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private DbConnection _connection = null!;
    private Respawner _respawner = null!;

    public LmsWebApplicationFactory()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithDatabase("lms_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    public string GetConnectionString() => _postgresContainer.GetConnectionString();

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(AppDbContext));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add DbContext using test database
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(GetConnectionString());
            });

            var sp = services.BuildServiceProvider();

            // Create a scope to get DbContext
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<AppDbContext>();

            db.Database.EnsureCreated();
        });
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        
        _connection = new NpgsqlConnection(GetConnectionString());
        await _connection.OpenAsync();

        // Run migrations first to create tables
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(GetConnectionString())
            .Options;

        using var dbContext = new AppDbContext(options);
        await dbContext.Database.MigrateAsync();

        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "public" }
        });
    }

    public new async Task DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.DisposeAsync();
        }
        await _postgresContainer.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        if (_respawner != null)
        {
            await _respawner.ResetAsync(_connection);
        }
    }
}
