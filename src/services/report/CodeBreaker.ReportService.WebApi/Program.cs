using System.Threading.RateLimiting;
using Azure.Identity;
using CodeBreaker.Data.ReportService.DbContexts;
using CodeBreaker.Data.ReportService.Repositories;
using CodeBreaker.ReportService.Services;
using FastExpressionCompiler;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OData;
using CodeBreaker.ReportService.OData;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

AzureCliCredential azureCredential = new();

var builder = WebApplication.CreateBuilder(args);

// AppConfiguration
builder.Configuration.AddAzureAppConfiguration(options =>
{
    string endpoint = builder.Configuration.GetRequired("AzureAppConfigurationEndpoint");
    options.Connect(new Uri(endpoint), azureCredential)
        .Select("ReportService*", LabelFilter.Null)
        .Select("ReportService*", builder.Environment.EnvironmentName)
        .ConfigureRefresh(refreshOptions =>
        {
            refreshOptions.Register("ReportService:Sentinel", true);
            refreshOptions.SetCacheExpiration(TimeSpan.FromMinutes(5));
        });
        // no keyvault needed
});

builder.Services.AddAzureAppConfiguration();

// Database
builder.Services.AddDbContext<GameContext>(options =>
{
    string accountEndpoint = builder.Configuration.GetRequired("ReportService:Cosmos:AccountEndpoint");
    string databaseName = builder.Configuration.GetRequired("ReportService:Cosmos:DatabaseName");
    options
        .UseCosmos(accountEndpoint, azureCredential, databaseName)
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
});

// Application services
builder.Services.AddScoped<IGameRepository, IQueryableGameRepository, GameRepository>();
builder.Services.AddScoped<IGameService, GameService>();

builder.Services
    .AddControllers()
    .AddOData(options =>
    {
        options.RouteOptions.EnableQualifiedOperationCall = false;
        options.EnableAttributeRouting = false;
        options
            .EnableQueryFeatures(10000)
            .Filter()
            .Select()
            .OrderBy()
            .Expand();
        options.AddRouteComponents("/odata", ODataEdm.Model);
    });

// OpenAPI & Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Mapster
TypeAdapterConfig.GlobalSettings.Compiler = e => e.CompileFast();

// RateLimiting
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("standardLimiter", context =>
        RateLimitPartition.GetConcurrencyLimiter("standardLimiter", key => new ConcurrencyLimiterOptions
        {
            PermitLimit = 10,
            QueueLimit = 5,
            QueueProcessingOrder = QueueProcessingOrder.NewestFirst
        }));
});

var app = builder.Build();

app.UseAzureAppConfiguration(); // For sentinel updates
app.UseRateLimiter();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
