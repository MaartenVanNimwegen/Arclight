using Arclight.Api.Endpoints;
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
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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
                    ValidIssuer = "ArclightApi",
                    ValidAudience = "ArclightClient",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]!)),

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
            });

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

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // Configure Endpoints
            app.MapUserEndpoints();
            app.MapArticleEndpoints();

            app.Run();
        }
    }
}
