using MongoDB.Driver;
using StackExchange.Redis; 
using UrlManagement.Api.Settings;
using UrlManagement.Api.Services;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// RabbitMQ Configuration
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetValue<string>("RabbitMq:Host") ?? "localhost", "/", h =>
        {
            h.Username(builder.Configuration.GetValue<string>("RabbitMq:Username") ?? "guest");
            h.Password(builder.Configuration.GetValue<string>("RabbitMq:Password") ?? "guest");
        });
    });
});

var mongoDbSettings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
builder.Services.AddSingleton(mongoDbSettings!);
builder.Services.AddSingleton<IMongoClient>(new MongoClient(mongoDbSettings!.ConnectionString));


var redisConnectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString") ?? "localhost:6379,abortConnect=false";
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services.AddScoped<IUrlService, UrlService>();
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment()) { app.MapOpenApi(); }
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
