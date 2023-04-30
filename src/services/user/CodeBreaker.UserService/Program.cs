using System.Globalization;
using Azure.Core;
using Azure.Identity;
using CodeBreaker.Shared.Models.Users;
using CodeBreaker.Shared.Models.Users.Api;
using CodeBreaker.Shared.Validators.Users;
using CodeBreaker.UserService.Models.Api;
using CodeBreaker.UserService.Options;
using CodeBreaker.UserService.Services;
using FastExpressionCompiler;
using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

DefaultAzureCredential azureCredential = new();

// Azure
string configEndpoint = builder.Configuration.GetRequired("AzureAppConfigurationEndpoint");
builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(new Uri(configEndpoint), azureCredential)
        .Select("UserService*", LabelFilter.Null)
        .Select("UserService*", builder.Environment.EnvironmentName)
        .ConfigureKeyVault(vault => vault.SetCredential(azureCredential));
});
builder.Services.AddAzureClients(azureServiceBuilder =>
{
    azureServiceBuilder.UseCredential(azureCredential);
    string storageEndpoint = builder.Configuration.GetRequired("UserService:Storage:Blob:Endpoint");
    azureServiceBuilder.AddBlobServiceClient(new Uri(storageEndpoint));
});

// Config
builder.Services.Configure<GamerNameCheckOptions>(builder.Configuration.GetRequiredSection("UserService:AzureActiveDirectory"));

// Cache
builder.Services.AddDistributedMemoryCache();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    CultureInfo german = new("de");
    CultureInfo english = new("en-US");
    CultureInfo[] supportedCultures = new[] { german, english };
    QueryStringRequestCultureProvider queryString = new();
    AcceptLanguageHeaderRequestCultureProvider langHeader = new();
    options.RequestCultureProviders = new IRequestCultureProvider[2] { queryString, langHeader };
    options.SupportedUICultures = supportedCultures;
    options.SupportedCultures = supportedCultures;
    options.DefaultRequestCulture = new RequestCulture(english);
});

// Custom services
builder.Services.AddTransient<IUserValidationService, UserValidationService>();
builder.Services.AddTransient<IGamerNameService, GamerNameService>();

// Validation
builder.Services.AddScoped<IValidator<User>, UserValidator>();

// Mapping
TypeAdapterConfig.GlobalSettings.Compiler = exp => exp.CompileFast();
TypeAdapterConfig<BeforeCreatingUserRequest, User>
    .NewConfig()
    .Map(dest => dest.GamerName, src => src.Extension_dd21590c971e431494da34e2a8d47cce_GamerName);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

app.MapPost("/validate-before-user-creation", async (BeforeCreatingUserRequest request, IUserValidationService userValidationService, CancellationToken cancellationToken) =>
{
    var user = request.Adapt<User>();
    var result = await userValidationService.ValidateUserAsync(user, cancellationToken);

    if (result.IsValid)
        return Results.Ok(new BeforeCreatingUserSuccessResponse());

    return Results.BadRequest(new BeforeCreatingUserValidationErrorResponse(result.ToString())); // ValidationError needs HTTP 400
    
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
