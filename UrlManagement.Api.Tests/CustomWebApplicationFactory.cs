using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UrlManagement.Api.Data;
using MassTransit;

namespace UrlManagement.Api.Tests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<UrlDbContext>));

            services.AddDbContext<UrlDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryUrlDbForTesting");
            });

            services.AddMassTransitTestHarness();
        });
    }
}
