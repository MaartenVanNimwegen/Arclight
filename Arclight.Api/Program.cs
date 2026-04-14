using Arclight.Api.Endpoints;
using Arclight.Api.Middleware;
using Arclight.Application;
using Arclight.Infrastructure;
using Arclight.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Arclight.Api
{
    public partial class Program
    {
        private const string ArclightFrontendCorsPolicy = "ArclightFrontend";

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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
            builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("DefaultConnection")!);

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
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // Configure Endpoints
            app.MapUserEndpoints();
            app.MapArticleEndpoints();
            app.MapCategoryEndpoints();
            app.MapCommentEndpoints();

            app.Run();
        }
    }
}
