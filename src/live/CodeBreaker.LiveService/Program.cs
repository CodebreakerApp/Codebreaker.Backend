using Azure.Identity;
using Azure.Messaging.EventHubs.Consumer;
using CodeBreaker.LiveService;
using CodeBreaker.LiveService.Options;
using CodeBreaker.Shared.Exceptions;
using LiveService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Options;

#if DEBUG
AzureCliCredential azureCredential = new();
#else
DefaultAzureCredential azureCredential = new();
#endif
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddAzureAppConfiguration(options =>
{
    string endpoint = builder.Configuration["AzureAppConfigurationEndpoint"] ?? throw new ConfigurationNotFoundException("AzureAppConfigurationEndpoint");
    options.Connect(new Uri(endpoint), azureCredential)
        .Select(KeyFilter.Any, LabelFilter.Null)
        .Select(KeyFilter.Any, builder.Environment.EnvironmentName)
        .ConfigureKeyVault(vault => vault.SetCredential(azureCredential));
});
builder.Services.AddAzureAppConfiguration();
builder.Services.Configure<LiveServiceOptions>(builder.Configuration.GetRequiredSection("LiveService"));
builder.Services.AddSingleton(x => x.GetRequiredService<IOptions<LiveServiceOptions>>().Value);
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
builder.Services.AddSingleton<ILiveHubSender, LiveHubSender>();
builder.Services.AddSingleton<IEventSourceService, EventSourceService>();
string? signalRConnectionString = builder.Configuration["LiveService:ConnectionStrings:SignalR"];

#if DEBUG
builder.Services.AddSignalR();
#else
builder.Services.AddSignalR().AddAzureSignalR(signalRConnectionString);
#endif

builder.Services.AddHostedService<EventHandlingService>();

WebApplication app = builder.Build();

app.UseAzureAppConfiguration();
app.MapHub<LiveHub>("/live");
app.MapGet("/update-config", (
    [FromServices] IConfigurationRoot c,
    [FromServices] ILogger<Program> logger
) =>
{
    c.Reload();
    logger.LogInformation("Configuration reloaded");
});
app.Run();
