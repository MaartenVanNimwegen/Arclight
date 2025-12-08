using Arclight.Application;
using Arclight.Application.Services;
using Arclight.Infrastructure;
using Arclight.Api.Endpoints;

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
