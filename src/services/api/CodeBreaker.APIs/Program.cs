using Azure.Identity;
using Azure.Messaging.EventHubs.Producer;

using CodeBreaker.APIs.Options;
using CodeBreaker.APIs.Services;
using CodeBreaker.APIs.Services.Cache;
using CodeBreaker.APIs.Utilities;

using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Options;

#if USEPROMETHEUS
using OpenTelemetry;
using OpenTelemetry.Metrics;

using System.Configuration;
#endif

using System.Runtime.CompilerServices;
using System.Threading.RateLimiting;

using CodeBreaker.APIs.Factories.GameTypeFactories;
using CodeBreaker.APIs.Grpc;
using CodeBreaker.APIs.Endpoints;
using Microsoft.AspNetCore.Http.Json;

[assembly: InternalsVisibleTo("CodeBreaker.APIs.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]


// ASP.NET Core registers the ASP.NET Core ActivitySource as singleton with the DI container.
// To keep this instance active, and activities are only started from the API endpoints, create
// one here, and pass it to the Map method
ActivitySource activitySource = new("CNinnovation.CodeBreaker.API");

#if USEPROMETHEUS
using MeterProvider meterProvider = Sdk.CreateMeterProviderBuilder()
    .AddMeter("CodeBreaker.APIs")
    .AddPrometheusExporter(opt =>
    {
        opt.StartHttpListener = true;
        opt.HttpListenerPrefixes = new string[] { "http://localhost:9184/" };
    }).Build();
#endif

#if DEBUG
AzureCliCredential azureCredential = new();
#else
DefaultAzureCredential azureCredential = new();
#endif
var builder = WebApplication.CreateBuilder(args);

// AppConfiguration
builder.Configuration.AddAzureAppConfiguration(options =>
{
    string endpoint = builder.Configuration["AzureAppConfigurationEndpoint"] ?? throw new ConfigurationErrorsException("AzureAppConfigurationEndpoint");
    options.Connect(new Uri(endpoint), azureCredential)
        .Select(KeyFilter.Any, LabelFilter.Null)
        .Select(KeyFilter.Any, builder.Environment.EnvironmentName)
        .ConfigureKeyVault(vault => vault.SetCredential(azureCredential));
});
builder.Services.AddAzureAppConfiguration();
builder.Services.Configure<ApiServiceOptions>(builder.Configuration.GetSection("ApiService"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<ApiServiceOptions>>().Value);

#if NET8_0_OR_GREATER
// JSON Serialization - do not enable this before .NET 8
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.AddContext<GamesJsonSerializerContext>();
});
#endif

// ApplicationInsights
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddSingleton<ITelemetryInitializer, ApplicationInsightsTelemetryInitializer>();

// Swagger/EndpointDocumentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddGrpc();

// Database
builder.Services.AddDbContext<ICodeBreakerContext, CodeBreakerContext>(options =>
{
    string accountEndpoint = builder.Configuration["ApiService:Cosmos:AccountEndpoint"]
        ?? throw new InvalidOperationException("ApiService:Cosmos:AccountEndpoint configuration is not available");
    string databaseName = builder.Configuration["ApiService:Cosmos:DatabaseName"]
        ?? throw new InvalidOperationException("ApiService:Cosmos:DatabaseName configuration is not availabile");
    options.UseCosmos(accountEndpoint, azureCredential, databaseName);
});

// EventHub
builder.Services.AddSingleton<EventHubProducerClient>(builder =>
{
    var options = builder.GetRequiredService<ApiServiceOptions>();
    return new(options.EventHub.FullyQualifiedNamespace, options.EventHub.Name, azureCredential);
});

// Cache
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IGameCache, GameCache>();

// Application Services
builder.Services.AddSingleton<IGameTypeFactoryMapper<string>, GameTypeFactoryMapper<string>>(x => new GameTypeFactoryMapper<string>().Initialize(
    new GameType6x4Factory(),
    new GameType6x4MiniFactory(),
    new GameType8x5Factory()
));

builder.Services.AddSingleton<IPublishEventService, EventService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IMoveService, MoveService>();

// CORS
const string AllowCodeBreakerOrigins = "_allowCodeBreakerOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowCodeBreakerOrigins,
        builder =>
        {
            builder.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});

builder.Services.AddRequestDecompression();
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

app.UseCors(AllowCodeBreakerOrigins);
app.UseRequestDecompression();

app.UseRateLimiter();

app.UseSwagger();
app.UseSwaggerUI();

// -------------------------
// Endpoints
// -------------------------

app.MapGrpcService<GrpcGameController>();

app.MapGameEndpoints(app.Logger, activitySource);

app.Run();
