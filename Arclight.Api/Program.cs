using Arclight.Application;
using Arclight.Application.Services;
using Arclight.Infrastructure;
using Arclight.Api.Endpoints;
using Arclight.Infrastructure.Persistence;

namespace Arclight.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("DefaultConnection")!);

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddControllers();
          
            builder.Services.AddOpenApi();

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

            app.UseAuthorization();

            app.MapControllers();

            // Configure Endpoints
            app.MapUserEndpoints();

            app.Run();
        }
    }
}
