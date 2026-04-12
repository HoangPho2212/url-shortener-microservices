using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace UrlManagement.Api.Tests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MongoDbSettings:DatabaseName"] = "UrlShortenerDb_Test",
                ["MongoDbSettings:CollectionName"] = "Urls_Test"
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            var mongoClient = Services.GetRequiredService<IMongoClient>();
            mongoClient.DropDatabase("UrlShortenerDb_Test");
        }
        base.Dispose(disposing);
    }
}
