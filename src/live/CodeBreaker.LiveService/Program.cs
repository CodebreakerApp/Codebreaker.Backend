using Azure.Identity;
using Azure.Messaging.EventHubs.Consumer;
using CodeBreaker.LiveService.Options;
using CodeBreaker.Shared.Exceptions;
using LiveService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Options;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddAzureAppConfiguration(options =>
{
    string endpoint = builder.Configuration["AzureAppConfigurationEndpoint"] ?? throw new ConfigurationNotFoundException("AzureAppConfigurationEndpoint");
    //DefaultAzureCredential credential = new();
    AzureCliCredential credential = new();
    options.Connect(new Uri(endpoint), credential)
        .Select(KeyFilter.Any, LabelFilter.Null)
        .Select(KeyFilter.Any, builder.Environment.EnvironmentName)
        .ConfigureKeyVault(vault => vault.SetCredential(credential));
});
builder.Services.AddAzureAppConfiguration();
builder.Services.Configure<LiveServiceOptions>(builder.Configuration.GetRequiredSection("LiveService"));
builder.Services.AddSingleton(x => x.GetRequiredService<IOptions<LiveServiceOptions>>().Value);
builder.Services.AddScoped(x =>
{
    LiveServiceOptions options = x.GetRequiredService<LiveServiceOptions>();
    return new EventHubConsumerClient(
        options.EventHub.ConsumerGroupName ?? EventHubConsumerClient.DefaultConsumerGroupName,
        options.EventHub.ConnectionString ?? throw new ConfigurationNotFoundException(),
        options.EventHub.Name ?? throw new ConfigurationNotFoundException(),
        new DefaultAzureCredential()
    );
});
string? signalRConnectionString = builder.Configuration["LiveService:ConnectionStrings:SignalR"];
builder.Services.AddSignalR().AddAzureSignalR(signalRConnectionString);
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
