using Arclight.Api.Endpoints;
using Arclight.Api.Middleware;
using Arclight.Application;
using Arclight.Infrastructure;
using Arclight.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
using Serilog;
using System.IO;

namespace Arclight.Api
{
    public partial class Program
    {
        private const string ArclightFrontendCorsPolicy = "ArclightFrontend";

        public static void Main(string[] args)
        {
            try
            {
                var logDirectory = Environment.GetEnvironmentVariable("ARCLIGHT_LOG_DIRECTORY");
                if (string.IsNullOrWhiteSpace(logDirectory))
                {
                    logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
                }

                try
                {
                    Directory.CreateDirectory(logDirectory);
                }
                catch (Exception ex)
                {
                    var sanitizedLogDirectory = logDirectory.Replace("\r", string.Empty).Replace("\n", string.Empty);
                    throw new InvalidOperationException($"Failed to create log directory at '{sanitizedLogDirectory}'.", ex);
                }

                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.File(
                        Path.Combine(logDirectory, "audit-log-.txt"),
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information
                    )
                .CreateLogger();

                Log.Information("Starting Arclight API");

                var builder = WebApplication.CreateBuilder(args);

                builder.Host.UseSerilog();

                // Mitigation for Threat #7: Anomalous traffic
                builder.Services.AddRateLimiter(options =>
                {
                    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                    options.AddFixedWindowLimiter(policyName: "fixed", options =>
                    {
                        options.PermitLimit = 10;
                        options.Window = TimeSpan.FromSeconds(10);
                        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                        options.QueueLimit = 2;
                    });
                });

                var corsAllowedOrigins = builder.Configuration["Cors:AllowedOrigins"]?
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                if (corsAllowedOrigins is null || corsAllowedOrigins.Length == 0)
                {
                    throw new InvalidOperationException("CORS configuration error: 'Cors:AllowedOrigins' is missing or empty.");
                }

                builder.Services.AddCors(options =>
                {
                    options.AddPolicy(ArclightFrontendCorsPolicy, policy =>
                    {
                        policy.WithOrigins(corsAllowedOrigins)
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
                });

                var jwtIssuer = builder.Configuration["JwtSettings:Issuer"];
                var jwtAudience = builder.Configuration["JwtSettings:Audience"];
                var jwtSecret = builder.Configuration["JwtSettings:Secret"];

                if (string.IsNullOrWhiteSpace(jwtIssuer))
                {
                    throw new InvalidOperationException("JWT configuration error: 'JwtSettings:Issuer' is missing or empty.");
                }

                if (string.IsNullOrWhiteSpace(jwtAudience))
                {
                    throw new InvalidOperationException("JWT configuration error: 'JwtSettings:Audience' is missing or empty.");
                }

                if (string.IsNullOrWhiteSpace(jwtSecret))
                {
                    throw new InvalidOperationException("JWT configuration error: 'JwtSettings:Secret' is missing or empty.");
                }

                builder.Services.AddApplication();

                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    if (builder.Environment.IsEnvironment("Testing"))
                    {
                        connectionString = "Host=localhost;Database=arclight-testing;Username=test;Password=test";
                    }
                    else
                    {
                        throw new InvalidOperationException("Database configuration error: 'ConnectionStrings:DefaultConnection' is missing or empty.");
                    }
                }

                builder.Services.AddInfrastructure(connectionString);

                builder.Services.AddEndpointsApiExplorer();

                builder.Services.AddControllers();

                builder.Services.AddOpenApi();

                builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.MapInboundClaims = false;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),

                        RoleClaimType = "role",
                        NameClaimType = "sub"
                    };
                });

                // Add authorization
                builder.Services.AddAuthorization(options =>
                {
                    // To be RequireContentManager, user must have role Admin or ContentCreator
                    options.AddPolicy("RequireContentManager", policy =>
                        policy.RequireRole("Admin", "ContentCreator"));

                    options.AddPolicy("RequireAdmin", policy =>
                        policy.RequireRole("Admin"));
                });

                builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
                builder.Services.AddProblemDetails();

                var app = builder.Build();

                app.UseRateLimiter();

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.MapOpenApi();
                }

                // Only seed in Development environment
                if (app.Environment.IsDevelopment())
                {
                    using (var scope = app.Services.CreateScope())
                    {
                        var services = scope.ServiceProvider;
                        try
                        {
                            var context = services.GetRequiredService<AppDbContext>();
                            Infrastructure.Data.DbInitializer.Initialize(context);
                        }
                        catch (Exception ex)
                        {
                            var logger = services.GetRequiredService<ILogger<Program>>();
                            logger.LogError(ex, "Something went wrong during seeding");
                        }
                    }
                }

                // Configure Middleware
                app.UseCors(ArclightFrontendCorsPolicy);
                app.UseExceptionHandler();

                app.UseSerilogRequestLogging();

                app.UseHttpsRedirection();

                app.UseAuthentication();
                app.UseAuthorization();

                // Configure Endpoints
                app.MapUserEndpoints().RequireRateLimiting("fixed");
                app.MapArticleEndpoints().RequireRateLimiting("fixed");
                app.MapCategoryEndpoints().RequireRateLimiting("fixed");
                app.MapCommentEndpoints().RequireRateLimiting("fixed");
                app.MapNewsletterEndpoints().RequireRateLimiting("fixed");

                app.MapControllers().RequireRateLimiting("fixed");

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
