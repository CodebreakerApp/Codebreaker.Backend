using Azure.Core;
using Azure.Identity;
using CodeBreaker.Shared.Models.Users.Api;
using CodeBreaker.UserService.Options;
using CodeBreaker.UserService.Services;
using Microsoft.Extensions.Azure;

#if DEBUG
TokenCredential azureCredential = new AzureCliCredential();
#else
TokenCredential azureCredential = new DefaultAzureCredential();
#endif

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<GamerNameCheckOptions>(options =>
{
    options.TenantId = "6a4e85c9-8636-4a94-a31b-894074bd6f9b";
    options.ClientId = "ca68b649-c212-406d-b0d7-5ac7f3ccbcea";
    options.ClientSecret = "eiu8Q~xe5tUUEPG4~nc6JHOWqnr205Hly4KXhbA4";
    options.GamerNameAttributeKey = "extension_dd21590c971e431494da34e2a8d47cce_GamerName";
});

builder.Services.AddAzureClients(builder =>
{
    builder.UseCredential(azureCredential);
    builder.AddBlobServiceClient(new Uri("https://codebreakerstorage.blob.core.windows.net"));
});

builder.Services.AddDistributedMemoryCache();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<IGamerNameService, GamerNameService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/gamer-names/{gamerName}/valid", async (string gamerName, IGamerNameService gamerNameService, CancellationToken cancellationToken) =>
    new GamerNameValidityResponse(await gamerNameService.CheckGamerNameAsync(gamerName, cancellationToken)))
.WithName("CheckGamerName")
.WithDescription("Checks whether the specified gamerName is valid")
.WithOpenApi();

app.MapGet("/gamer-names/suggestions", (int? count, IGamerNameService gamerNameService, CancellationToken cancellationToken) =>
    new GamerNameSuggestionsResponse(gamerNameService.SuggestGamerNamesAsync(count ?? 10, cancellationToken)))
.WithName("SuggestGamerNames")
.WithDescription("Suggessts possible and available gamer names")
.WithOpenApi();

app.Run();
