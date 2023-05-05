using System.Threading.RateLimiting;
using Azure.Identity;
using Azure.Messaging.EventHubs.Consumer;
using CodeBreaker.LiveService;
using CodeBreaker.LiveService.Options;
using CodeBreaker.LiveService.Utilities;
using CodeBreaker.Shared.Exceptions;
using LiveService;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Options;


DefaultAzureCredential azureCredential = new();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// AppConfiguration
builder.Configuration.AddAzureAppConfiguration(options =>
{
    string endpoint = builder.Configuration["AzureAppConfigurationEndpoint"] ?? throw new InvalidOperationException("AzureAppConfigurationEndpoint not found in configuration");
    options.Connect(new Uri(endpoint), azureCredential)
        .Select("LiveService*", LabelFilter.Null)
        .Select("LiveService*", builder.Environment.EnvironmentName)
        .ConfigureKeyVault(vault => vault.SetCredential(azureCredential));
});
builder.Services.AddAzureAppConfiguration();
builder.Services.Configure<LiveServiceOptions>(builder.Configuration.GetRequiredSection("LiveService"));
builder.Services.AddSingleton(x => x.GetRequiredService<IOptions<LiveServiceOptions>>().Value);

// ApplicationInsights
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddSingleton<ITelemetryInitializer, ApplicationInsightsTelemetryInitializer>();

// Swagger and EndpointDocumentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// EventHub
builder.Services.AddSingleton(x =>
{
    LiveServiceOptions options = x.GetRequiredService<LiveServiceOptions>();
    return new EventHubConsumerClient(
        options.EventHub.ConsumerGroupName ?? EventHubConsumerClient.DefaultConsumerGroupName,
        options.EventHub.FullyQualifiedNamespace ?? throw new ConfigurationNotFoundException(),
        options.EventHub.Name ?? throw new ConfigurationNotFoundException(),
        azureCredential
    );
});

// SignalR
#if DEBUG
builder.Services.AddSignalR();
#else
string? signalRConnectionString = builder.Configuration["LiveService:ConnectionStrings:SignalR"];
builder.Services.AddSignalR().AddAzureSignalR(signalRConnectionString);
#endif

// Others
builder.Services.AddHostedService<EventHandlingService>();
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

// Application Services
builder.Services.AddSingleton<ILiveHubSender, LiveHubSender>();
builder.Services.AddSingleton<IEventSourceService, EventSourceService>();

WebApplication app = builder.Build();

app.UseRequestDecompression();
app.UseRateLimiter();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAzureAppConfiguration();
app.MapHub<LiveHub>("/live");
app.MapGet("/update-config", (
    [FromServices] IConfigurationRoot c,
    [FromServices] ILogger<Program> logger
) =>
{
    c.Reload();
    logger.LogInformation("Configuration reloaded");
    return Results.NoContent();
})
.Produces(StatusCodes.Status204NoContent)
.WithName("TriggerConfigUpdate")
.WithSummary("Triggers the reload of the configuration.")
.WithOpenApi()
.RequireRateLimiting("standardLimiter");

app.Run();
