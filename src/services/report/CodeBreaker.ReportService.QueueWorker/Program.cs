using Azure.Identity;
using CodeBreaker.Data.ReportService.DbContexts;
using CodeBreaker.Data.ReportService.Repositories;
using CodeBreaker.Queuing.ReportService.Options;
using CodeBreaker.Queuing.ReportService.Services;
using CodeBreaker.ReportService.QueueWorker.Options;
using CodeBreaker.ReportService.QueueWorker.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

DefaultAzureCredential azureCredential = new();
Host.CreateDefaultBuilder()
    .ConfigureHostConfiguration(configuration =>
    {
        configuration.AddJsonFile("appsettings.json", false, true);
    })
    .ConfigureAppConfiguration((context, configurationBuilder) =>
    {
        configurationBuilder.AddAzureAppConfiguration(options =>
        {
            string endpoint = context.Configuration["AzureAppConfigurationEndpoint"] ?? throw new InvalidOperationException("AzureAppConfigurationEndpoint");
            options.Connect(new Uri(endpoint), azureCredential)
                .Select("ReportService*", LabelFilter.Null)
                .Select("ReportService*", context.HostingEnvironment.EnvironmentName);
                // no refresh needed here, because this application is short lived
                // no keyvault needed
        });
    })
    .ConfigureServices((context, services) =>
    {
        services.AddAzureClients(options =>
        {
            Uri queueUri = new(context.Configuration["ReportService:Storage:Queue:ServiceUri"] ?? throw new InvalidOperationException("not found"));
            options.AddQueueServiceClient(queueUri);
            options.UseCredential(azureCredential);
        });

        services.AddDbContext<GameContext>(options =>
        {
            string accountEndpoint = context.Configuration["ReportService:Cosmos:AccountEndpoint"] ?? throw new InvalidOperationException();
            string databaseName = context.Configuration["ReportService:Cosmos:DatabaseName"] ?? throw new InvalidOperationException();
            options
                .UseCosmos(accountEndpoint, azureCredential, databaseName)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
        });

        services.AddSingleton<IGameRepository, GameRepository>();
        services.Configure<GameQueueOptions>(context.Configuration.GetRequiredSection("ReportService:Storage:Queue:GamesQueue"));
        services.AddSingleton<IGameQueueReceiverService, GameQueueService>();
        services.Configure<QueueServiceOptions>(context.Configuration.GetRequiredSection("ReportService:QueueWorker"));
        services.AddHostedService<QueueService>();
    })
    .Build()
    .Start();
