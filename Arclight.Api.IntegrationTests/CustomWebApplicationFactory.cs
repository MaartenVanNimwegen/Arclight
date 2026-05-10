using Arclight.Api;
using Arclight.Application.Interfaces;
using Arclight.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System;

namespace Arclight.Api.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"IntegrationTestsDb-{Guid.NewGuid()}";

    public CustomWebApplicationFactory()
    {
        // Provide the required configuration values before Program.Main is executed,
        // since WebApplication.CreateBuilder reads these from env vars before Build() is called.
        Environment.SetEnvironmentVariable("Cors__AllowedOrigins", "http://localhost:3000");
        Environment.SetEnvironmentVariable("JwtSettings__Issuer", "test-issuer");
        Environment.SetEnvironmentVariable("JwtSettings__Audience", "test-audience");
        Environment.SetEnvironmentVariable("JwtSettings__Secret", "test-secret-key-for-integration-tests-only");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(IEmailService));
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.RemoveAll(typeof(AppDbContext));
            services.RemoveAll(typeof(System.Data.Common.DbConnection));

            var emailServiceMock = new Mock<IEmailService>();
            emailServiceMock
                .Setup(e => e.SendEmailAsync(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            services.AddScoped<IEmailService>(_ => emailServiceMock.Object);

            services.AddSingleton<DbContextOptions<AppDbContext>>(provider =>
            {
                return new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(_databaseName)
                    .Options;
            });

            services.Configure<Microsoft.AspNetCore.Authentication.AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = "TestAuth";
                options.DefaultChallengeScheme = "TestAuth";
            });

            services.AddAuthentication("TestAuth")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestAuth", options => { });

            services.AddScoped<AppDbContext>();
        });
    }
}