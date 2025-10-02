using Azure.Provisioning.AppContainers;
using Azure.Provisioning.EventHubs;
using Azure.Provisioning.Sql;

var builder = DistributedApplication.CreateBuilder(args);

string dataStore = builder.Configuration["DataStore"] ?? "InMemory";
string startupMode = builder.Configuration["STARTUP_MODE"] ?? "Azure";
string botLoop = builder.Configuration.GetSection("Bot")["Loop"] ?? "false";
string botDelay = builder.Configuration.GetSection("Bot")["Delay"] ?? "1000";

var redis = builder.AddRedis("redis")
    .WithRedisInsight()
    .PublishAsContainer();

if (startupMode == "OnPremises")
{
    var kafka = builder.AddKafka("kafkamessaging")
        .PublishAsContainer();

    var sqlServer = builder.AddSqlServer("sql")
        .WithDataVolume()
        .PublishAsContainer()
        .AddDatabase("CodebreakerSql");

    var cosmos = builder.AddAzureCosmosDB("codebreakercosmos")
        .AddCosmosDatabase("codebreaker");

    var gameAPIs = builder.AddProject<Projects.Codebreaker_GameAPIs>("gameapis")
        .WithExternalHttpEndpoints()
        .WithReference(sqlServer)
        .WithReference(redis)
        .WithReference(kafka)
        .WithEnvironment("DataStore", dataStore)
        .WithEnvironment("StartupMode", startupMode);

    builder.AddProject<Projects.CodeBreaker_Bot>("bot")
        .WithExternalHttpEndpoints()
        .WithReference(gameAPIs)
        .WithEnvironment("Bot__Loop", botLoop)
        .WithEnvironment("Bot__Delay", botDelay);

    var live = builder.AddProject<Projects.Codebreaker_Live>("live")
        .WithExternalHttpEndpoints()
        .WithReference(kafka)
        .WithEnvironment("StartupMode", startupMode);

    builder.AddProject<Projects.Codebreaker_Ranking>("ranking")
        .WithExternalHttpEndpoints()
        .WithReference(cosmos)
        .WithReference(kafka)
        .WithEnvironment("StartupMode", startupMode);

    builder.AddProject<Projects.CodeBreaker_Blazor>("blazor")
        .WithExternalHttpEndpoints()
        .WithReference(gameAPIs);
}
else
{
    builder.AddAzureContainerAppEnvironment("codebreaker-environment");

    var logs = builder.AddAzureLogAnalyticsWorkspace("logs");
    var insights = builder.AddAzureApplicationInsights("insights", logs);
    var signalR = builder.AddAzureSignalR("signalr");
    var storage = builder.AddAzureStorage("storage");

    var botQueue = storage.AddQueues("botqueue");
    var blob = storage.AddBlobs("checkpoints");

    var eventHub = builder.AddAzureEventHubs("codebreakerevents")
        .ConfigureInfrastructure(infrastructure =>
        {
            var eventHubsNamespace = infrastructure.GetProvisionableResources().OfType<EventHubsNamespace>().Single();
            eventHubsNamespace.Sku = new EventHubsSku()
            {
                Name = EventHubsSkuName.Basic,
                Tier = EventHubsSkuTier.Basic
            };
        });

    eventHub.AddHub("games");

    var cosmos = builder.AddAzureCosmosDB("codebreakercosmos")
        .AddCosmosDatabase("codebreaker");

    cosmos.AddContainer("GamesV3", "/PartitionKey");

    var gatewayKeyvault = builder.AddAzureKeyVault("gateway-keyvault");
    var userServiceKeyvault = builder.AddAzureKeyVault("users-keyvault");

    // TODO: fix new eventhub namings
    var gameAPIs = builder.AddProject<Projects.Codebreaker_GameAPIs>("gameapis")
        .WithReference(cosmos).WaitFor(cosmos)
        .WithReference(redis).WaitFor(redis)
        .WithReference(insights).WaitFor(insights)
        .WithReference(eventHub).WaitFor(eventHub)
        .WithEnvironment("DataStore", dataStore)
        .WithEnvironment("StartupMode", startupMode)
        .PublishAsAzureContainerApp((module, app) =>
        {
            app.Template.Scale.MinReplicas = 0;
            app.Template.Scale.MaxReplicas = 2;
        });

    var bot = builder.AddProject<Projects.CodeBreaker_Bot>("bot")
        .WithReference(insights).WaitFor(insights)
        .WithReference(botQueue).WaitFor(botQueue)
        .WithReference(gameAPIs).WaitFor(gameAPIs)
        .WithEnvironment("Bot__Loop", botLoop)
        .WithEnvironment("Bot__Delay", botDelay)
        .PublishAsAzureContainerApp((module, app) =>
        {
            app.Template.Scale.MinReplicas = 0;
            app.Template.Scale.MaxReplicas = 1;
        });

    // TODO: change to use BotQ with Container App Jobs
#pragma warning disable ASPIREAZURE002 // PublishAsAzureContainerAppJob is for evaluation purposes
    var botq = builder.AddProject<Projects.Codebreaker_BotQ>("botq")
        .WithReference(insights).WaitFor(insights)
        .WithReference(botQueue).WaitFor(botQueue)
        .WithReference(gameAPIs).WaitFor(gameAPIs)
        .WithEnvironment("Bot__Loop", botLoop)
        .WithEnvironment("Bot__Delay", botDelay)
        .PublishAsAzureContainerAppJob((_, job) =>
        {
            job.Configuration.TriggerType = ContainerAppJobTriggerType.Event;
            job.Configuration.EventTriggerConfig.Scale.MinExecutions = 1;
            job.Configuration.EventTriggerConfig.Scale.MaxExecutions = 10;
            job.Configuration.EventTriggerConfig.Parallelism = 1;
            job.Configuration.EventTriggerConfig.ReplicaCompletionCount = 1;
            // TODO: specify scale rule on queue trigger
            //job.Configuration.EventTriggerConfig.Scale.Rules.Add(new ContainerAppJobScaleRule()
            //{
            //    QueueName = botQueue.Resource.Name,
            //    MessageCount = 1
            //});
        });
#pragma warning restore ASPIREAZURE002

    var live = builder.AddProject<Projects.Codebreaker_Live>("live")
        .WithReference(insights)
        .WithReference(eventHub)
        .WithReference(signalR)
        .WaitFor(eventHub)
        .WaitFor(gameAPIs)
        .PublishAsAzureContainerApp((module, app) =>
        {
            app.Template.Scale.MinReplicas = 0;
            app.Template.Scale.MaxReplicas = 1;
        });

    var ranking = builder.AddProject<Projects.Codebreaker_Ranking>("ranking")
        .WithReference(cosmos)
        .WithReference(insights)
        .WithReference(eventHub).WaitFor(eventHub)
        .WithReference(blob)
        .WaitFor(insights)
        .WaitFor(gameAPIs)
        .PublishAsAzureContainerApp((module, app) =>
        {
            app.Template.Scale.MinReplicas = 0;
            app.Template.Scale.MaxReplicas = 1;
        });

    var users = builder.AddProject<Projects.CodeBreaker_UserService>("users")
        .WithReference(insights)
        .WithReference(userServiceKeyvault)
        .WaitFor(insights)
        .WaitFor(userServiceKeyvault)
        .PublishAsAzureContainerApp((module, app) =>
        {
            app.Template.Scale.MinReplicas = 0;
            app.Template.Scale.MaxReplicas = 1;
        });

    var gateway = builder.AddProject<Projects.Codebreaker_ApiGateway>("gateway")
        .WithExternalHttpEndpoints()
        .WithReference(gameAPIs).WaitFor(gameAPIs)
        .WithReference(live).WaitFor(live)
        .WithReference(ranking).WaitFor(ranking)
        .WithReference(users).WaitFor(users)
        .WithReference(gatewayKeyvault).WaitFor(gatewayKeyvault)
        .WithReference(insights).WaitFor(insights)
        .PublishAsAzureContainerApp((module, app) =>
        {
            app.Template.Scale.MinReplicas = 0;
            app.Template.Scale.MaxReplicas = 2;
        });

    builder.AddProject<Projects.CodeBreaker_Blazor>("blazor")
        .WithExternalHttpEndpoints()
        .WithReference(gateway).WaitFor(gateway)
        .WithReference(insights).WaitFor(insights)
        .PublishAsAzureContainerApp((module, app) =>
        {
            app.Template.Scale.MinReplicas = 0;
            app.Template.Scale.MaxReplicas = 1;
        });
}

builder.Build().Run();