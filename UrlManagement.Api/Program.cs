using MongoDB.Driver;
using StackExchange.Redis; 
using UrlManagement.Api.Settings;
using UrlManagement.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var mongoDbSettings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
builder.Services.AddSingleton(mongoDbSettings!);
builder.Services.AddSingleton<IMongoClient>(new MongoClient(mongoDbSettings!.ConnectionString));


builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost:6379"));

builder.Services.AddScoped<IUrlService, UrlService>();
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment()) { app.MapOpenApi(); }
app.UseHttpsRedirection();
app.MapControllers();
app.Run();