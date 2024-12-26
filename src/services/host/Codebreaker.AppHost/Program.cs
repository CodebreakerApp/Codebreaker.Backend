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
        .AddDatabase("codebreaker");

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
    var logs = builder.AddAzureLogAnalyticsWorkspace("logs");
    var insights = builder.AddAzureApplicationInsights("insights", logs);
    var signalR = builder.AddAzureSignalR("signalr");
    var storage = builder.AddAzureStorage("storage");

    var botQueue = storage.AddQueues("botqueue");
    var blob = storage.AddBlobs("checkpoints");

    var eventHub = builder.AddAzureEventHubs("codebreakerevents")
        .AddEventHub("games");

    var cosmos = builder.AddAzureCosmosDB("codebreakercosmos")
        .AddDatabase("codebreaker");

    var gatewayKeyvault = builder.AddAzureKeyVault("gateway-keyvault");
    var userServiceKeyvault = builder.AddAzureKeyVault("users-keyvault");

    var cosmoscreate = builder.AddProject<Projects.Codebreaker_CosmosCreate>("cosmoscreate")
        .WithReference(cosmos)
        .WithReference(insights)
        .WaitFor(cosmos)
        .WaitFor(insights);

    var gameAPIs = builder.AddProject<Projects.Codebreaker_GameAPIs>("gameapis")
        .WithReference(cosmos)
        .WithReference(redis)
        .WithReference(insights)
        .WithReference(eventHub)
        .WithEnvironment("DataStore", dataStore)
        .WithEnvironment("StartupMode", startupMode)
        .WaitForCompletion(cosmoscreate);  // we use cosmos with the Azure option

    // TODO: change to use BotQ with Container App Jobs
    var botq = builder.AddProject<Projects.Codebreaker_BotQ>("botq")
        .WithReference(insights)
        .WithReference(botQueue)
        .WithReference(gameAPIs)
        .WithEnvironment("Bot__Loop", botLoop)
        .WithEnvironment("Bot__Delay", botDelay)
        .WaitFor(gameAPIs);

    var live = builder.AddProject<Projects.Codebreaker_Live>("live")
        .WithReference(insights)
        .WithReference(eventHub)
        .WithReference(signalR)
        .WaitFor(eventHub)
        .WaitFor(gameAPIs);

    var ranking = builder.AddProject<Projects.Codebreaker_Ranking>("ranking")
        .WithReference(cosmos)
        .WithReference(insights)
        .WithReference(eventHub)
        .WithReference(blob)
        .WaitFor(eventHub)
        .WaitFor(insights)
        .WaitFor(gameAPIs);

    var users = builder.AddProject<Projects.CodeBreaker_UserService>("users")
        .WithReference(insights)
        .WithReference(userServiceKeyvault)
        .WaitFor(insights)
        .WaitFor(userServiceKeyvault);

    var gateway = builder.AddProject<Projects.Codebreaker_ApiGateway>("gateway")
        .WithExternalHttpEndpoints()
        .WithReference(gameAPIs)
        .WithReference(live)
        .WithReference(ranking)
        .WithReference(users)
        .WithReference(gatewayKeyvault)
        .WithReference(insights)
        .WaitFor(gameAPIs)
        .WaitFor(live)
        .WaitFor(ranking)
        .WaitFor(users)
        .WaitFor(gatewayKeyvault)
        .WaitFor(insights);


    builder.AddProject<Projects.CodeBreaker_Blazor>("blazor")
        .WithExternalHttpEndpoints()
        .WithReference(gateway)
        .WithReference(insights)
        .WaitFor(gateway)
        .WaitFor(insights);
}

builder.Build().Run();