using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using UserManagement.Api.Models;
using UserManagement.Api.Settings;

namespace UserManagement.Api.Tests.IntegrationTests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MongoDbSettings:DatabaseName"] = "UserShortenerDb_Test",
                ["MongoDbSettings:CollectionName"] = "Users_Test"
            });
        });

        builder.ConfigureServices(services =>
        {
            // The services will use the overridden configuration above
            // ensuring they connect to the test database.
        });
    }

    // Clean up the test database after tests
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            var mongoClient = Services.GetRequiredService<IMongoClient>();
            mongoClient.DropDatabase("UserShortenerDb_Test");
        }
        base.Dispose(disposing);
    }
}
