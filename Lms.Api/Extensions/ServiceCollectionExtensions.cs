using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
using Lms.Api.Services;
using Lms.Api.Services.Interfaces;
using Lms.Api.Data;

namespace Lms.Api.Extensions;

public static class ServiceCollectionExtensions
{
    // ── Core Services ───────────────────────────────────────
    public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        return services;
    }

    // ── Database Services ─────────────────────────────────────
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        string connectionString;

        if (!string.IsNullOrWhiteSpace(databaseUrl))
        {
            var uri = new Uri(databaseUrl);
            var userInfo = uri.UserInfo.Split(':');
            var port = uri.Port > 0 ? uri.Port : 5432;
            connectionString =
                $"Host={uri.Host};" +
                $"Port={port};" +
                $"Database={uri.AbsolutePath.TrimStart('/')};" +
                $"Username={Uri.UnescapeDataString(userInfo[0])};" +
                $"Password={Uri.UnescapeDataString(userInfo[1])};" +
                $"SSL Mode=Require;Trust Server Certificate=true";
        }
        else
        {
            connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Missing connection string. Set DATABASE_URL or ConnectionStrings:DefaultConnection.");
        }

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Health Checks
        var healthChecks = services.AddHealthChecks()
            .AddNpgSql(connectionString, name: "postgresql", tags: ["ready"]);

        return services;
    }

    // ── Cache Services ─────────────────────────────────────────
    public static IServiceCollection AddCacheServices(this IServiceCollection services, IConfiguration configuration)
    {
        var redisUrl = Environment.GetEnvironmentVariable("REDIS_URL");
        var redisConnection = redisUrl ?? configuration.GetConnectionString("Redis");

        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "lms:";
            });

            // Add Redis health check
            services.AddHealthChecks()
                .AddRedis(redisConnection, name: "redis", tags: ["ready"]);
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        return services;
    }

    // ── Background Job Services ─────────────────────────────────
    public static IServiceCollection AddBackgroundJobServices(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        string connectionString;

        if (!string.IsNullOrWhiteSpace(databaseUrl))
        {
            var uri = new Uri(databaseUrl);
            var userInfo = uri.UserInfo.Split(':');
            var port = uri.Port > 0 ? uri.Port : 5432;
            connectionString =
                $"Host={uri.Host};" +
                $"Port={port};" +
                $"Database={uri.AbsolutePath.TrimStart('/')};" +
                $"Username={Uri.UnescapeDataString(userInfo[0])};" +
                $"Password={Uri.UnescapeDataString(userInfo[1])};" +
                $"SSL Mode=Require;Trust Server Certificate=true";
        }
        else
        {
            connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Missing connection string.");
        }

        services.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(opts =>
                    opts.UseNpgsqlConnection(connectionString));
        });
        services.AddHangfireServer();

        return services;
    }

    // ── Authentication Services ─────────────────────────────────
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Google OAuth
        var ggClientId = Environment.GetEnvironmentVariable("GG_CLIENT_ID");
        if (!string.IsNullOrWhiteSpace(ggClientId))
            configuration["Google:ClientId"] = ggClientId;

        // JWT Authentication
        var jwtSettings = configuration.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();

        return services;
    }

    // ── Rate Limiting Services ─────────────────────────────────────
    public static IServiceCollection AddRateLimitingServices(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Auth endpoints: 10 requests per minute
            options.AddFixedWindowLimiter("auth", opt =>
            {
                opt.PermitLimit = 10;
                opt.Window = TimeSpan.FromMinutes(1);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 2;
            });

            // Public endpoints: 200 requests per minute
            options.AddFixedWindowLimiter("public", opt =>
            {
                opt.PermitLimit = 200;
                opt.Window = TimeSpan.FromMinutes(1);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 5;
            });

            // Authenticated user endpoints: 300 requests per minute
            options.AddFixedWindowLimiter("authenticated", opt =>
            {
                opt.PermitLimit = 300;
                opt.Window = TimeSpan.FromMinutes(1);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 10;
            });

            // Admin endpoints: 300 requests per minute
            options.AddFixedWindowLimiter("admin", opt =>
            {
                opt.PermitLimit = 300;
                opt.Window = TimeSpan.FromMinutes(1);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 10;
            });
        });

        return services;
    }

    // ── CORS Services ─────────────────────────────────────────────
    public static IServiceCollection AddCorsServices(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(
                        "https://lms-fe-zs6t.onrender.com",
                        "http://localhost:5173"
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }

    // ── Application Services ───────────────────────────────────────
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Scoped Services
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<ILessonService, LessonService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<ILessonProgressService, LessonProgressService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IQuizService, QuizService>();
        services.AddScoped<IPracticeService, PracticeService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }

    // ── Swagger Services ───────────────────────────────────────────
    public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "LMS API",
                Version = "v1"
            });

            options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Description = "Enter your JWT token"
            });

            options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}
