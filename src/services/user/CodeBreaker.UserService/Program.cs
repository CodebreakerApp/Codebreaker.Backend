using CodeBreaker.Shared.Models.Users.Api;
using CodeBreaker.UserService.Options;
using CodeBreaker.UserService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MicrosoftGraphOptions>(options =>
{
    options.TenantId = "6a4e85c9-8636-4a94-a31b-894074bd6f9b";
    options.ClientId = "ca68b649-c212-406d-b0d7-5ac7f3ccbcea";
    options.ClientSecret = "eiu8Q~xe5tUUEPG4~nc6JHOWqnr205Hly4KXhbA4";
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<IGamerNameService, GamerNameService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/gamer-names/{gamerName}/check", async (string gamerName, IGamerNameService gamerNameService) =>
    new GamerNameValidityResponse(await gamerNameService.CheckGamerNameAsync(gamerName)))
.WithName("CheckGamerName")
.WithDescription("Checks whether the specified gamerName is valid")
.WithOpenApi();

app.MapGet("/gamer-names/suggested", (int? count, IGamerNameService gamerNameService) =>
    new GamerNameSuggestionsResponse(gamerNameService.SuggestGamerNamesAsync(count ?? 10)))
.WithName("SuggestGamerNames")
.WithDescription("Suggessts possible and available gamer names")
.WithOpenApi();

app.Run();
