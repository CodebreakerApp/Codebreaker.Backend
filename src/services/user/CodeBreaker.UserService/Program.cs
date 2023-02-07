using Azure.Core;
using Azure.Identity;
using CodeBreaker.Shared.Models.Users.Api;
using CodeBreaker.UserService.Models.Api;
using CodeBreaker.UserService.Options;
using CodeBreaker.UserService.Services;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;

#if DEBUG
TokenCredential azureCredential = new AzureCliCredential();
#else
TokenCredential azureCredential = new DefaultAzureCredential();
#endif

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(new Uri("https://codebreaker.azconfig.io"), azureCredential);
    options.ConfigureKeyVault(keyvaultOptions => keyvaultOptions.SetCredential(azureCredential));
});

builder.Services.AddAzureClients(builder =>
{
    builder.UseCredential(azureCredential);
    builder.AddBlobServiceClient(new Uri("https://codebreakerstorage.blob.core.windows.net"));
});

builder.Services.Configure<GamerNameCheckOptions>(builder.Configuration.GetRequiredSection("UserService:AzureActiveDirectory"));

builder.Services.AddDistributedMemoryCache();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<IAzureActiveDirectoryB2CApiConnectorService, AzureActiveDirectoryB2CApiConnectorService>();
builder.Services.AddTransient<IGamerNameService, GamerNameService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/validate-before-user-creation", async (BeforeCreatingUserRequest request, IAzureActiveDirectoryB2CApiConnectorService apiConnectorService, CancellationToken cancellationToken) =>
{
    var response = await apiConnectorService.ValidateBeforeUserCreationAsync(request, cancellationToken);

    if (response is BeforeCreatingUserValidationErrorResponse)
        return Results.BadRequest(response);    // HTTP code 400 is necessary for a validationErrorResponse

    return Results.Ok(response);
})
.WithName("CheckGamerName")
.WithDescription("Checks whether the specified gamerName is valid")
.WithOpenApi();

app.MapGet("/gamer-names/suggestions", (int? count, IGamerNameService gamerNameService, CancellationToken cancellationToken) =>
    new GamerNameSuggestionsResponse(gamerNameService.SuggestGamerNamesAsync(count ?? 10, cancellationToken)))
.WithName("SuggestGamerNames")
.WithDescription("Suggessts possible and available gamer names")
.WithOpenApi();

app.Run();
