var builder = DistributedApplication.CreateBuilder(args);

string dataStore = builder.Configuration["DataStore"] ?? "InMemory";
string startupMode = builder.Configuration["STARTUP_MODE"] ?? "Azure";
string botLoop = builder.Configuration.GetSection("Bot")["Loop"] ?? "false";
string botDelay = builder.Configuration.GetSection("Bot")["Delay"] ?? "1000";

var redis = builder.AddRedis("redis")
    .WithRedisCommander()
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

    var weather = builder.AddProject<Projects.TestWeather>("testweather")
        .WithReference(cosmos)
        .WithReference(redis)
        .WithReference(insights)
        .WithEnvironment("DataStore", dataStore)
        .WithEnvironment("StartupMode", startupMode);

    var gameAPIs = builder.AddProject<Projects.Codebreaker_GameAPIs>("gameapis")
        .WithReference(cosmos)
        .WithReference(redis)
        .WithReference(insights)
        .WithReference(eventHub)
        .WithEnvironment("DataStore", dataStore)
        .WithEnvironment("StartupMode", startupMode)
        .WithExternalHttpEndpoints();

    builder.AddProject<Projects.Codebreaker_BotQ>("bot")
        .WithReference(insights)
        .WithReference(botQueue)
        .WithReference(gameAPIs)
        .WithEnvironment("Bot__Loop", botLoop)
        .WithEnvironment("Bot__Delay", botDelay);

    var live = builder.AddProject<Projects.Codebreaker_Live>("live")
        .WithReference(insights)
        .WithReference(eventHub)
        .WithReference(signalR);

    var ranking = builder.AddProject<Projects.Codebreaker_Ranking>("ranking")
        .WithReference(cosmos)
        .WithReference(insights)
        .WithReference(eventHub)
        .WithReference(blob);

    var users = builder.AddProject<Projects.CodeBreaker_UserService>("users")
        .WithReference(insights)
        .WithReference(userServiceKeyvault);

    var gateway = builder.AddProject<Projects.Codebreaker_ApiGateway>("gateway")
        .WithExternalHttpEndpoints()
        .WithReference(gameAPIs)
        .WithReference(live)
        .WithReference(ranking)
        .WithReference(users)
        .WithReference(gatewayKeyvault);

    builder.AddProject<Projects.CodeBreaker_Blazor>("blazor")
        .WithExternalHttpEndpoints()
        .WithReference(gateway);
}

builder.Build().Run();