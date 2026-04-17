using Arclight.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace Arclight.Api.IntegrationTests;

public abstract class BaseIntegrationTest : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly HttpClient Client;
    protected readonly CustomWebApplicationFactory Factory;

    protected BaseIntegrationTest(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    protected HttpClient CreateClientWithRoles(params string[] roles)
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, string.Join(",", roles));
        return client;
    }

    protected HttpClient CreateAnonymousClient()
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Skip-Test-Auth", "true");
        return client;
    }

    protected async Task ExecuteDbContextAsync(Func<AppDbContext, Task> action)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await action(context);
    }
}