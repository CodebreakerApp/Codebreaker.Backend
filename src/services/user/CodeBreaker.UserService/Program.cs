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

TokenCredential azureCredential = builder.Environment.IsDevelopment()
    ? new AzureCliCredential()
    : new ManagedIdentityCredential();

// Azure
string configEndpoint = builder.Configuration.GetRequired("AzureAppConfigurationEndpoint");
builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(new Uri(configEndpoint), azureCredential)
        .Select(KeyFilter.Any, LabelFilter.Null)
        .Select(KeyFilter.Any, builder.Environment.EnvironmentName)
        .ConfigureKeyVault(vault => vault.SetCredential(azureCredential));
});
builder.Services.AddAzureClients(azureServiceBuilder =>
{
    azureServiceBuilder.UseCredential(azureCredential);
    string storageEndpoint = builder.Configuration.GetRequired("AzureBlobStorageEndpoint");
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
    CultureInfo[] supportedCultures = [german, english];
    QueryStringRequestCultureProvider queryString = new();
    AcceptLanguageHeaderRequestCultureProvider langHeader = new();
    options.RequestCultureProviders = [queryString, langHeader];
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
TypeAdapterConfig<BeforeCreatingUserRequest, User>.NewConfig();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

//
// Username endpoints
//
// POST /validate-before-user-creation
// Request: BeforeCreatingUserRequest
// Response: BeforeCreatingUserSuccessResponse | BeforeCreatingUserValidationErrorResponse
// Description: Checks whether the specified user can be created
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

// GET /gamer-names/suggestions
// Query: int count
// Response: GamerNameSuggestionsResponse
// Description: Suggests possible and available gamer names
app.MapGet("/gamer-names/suggestions", (int? count, IGamerNameService gamerNameService, CancellationToken cancellationToken) =>
    new GamerNameSuggestionsResponse(gamerNameService.SuggestGamerNamesAsync(count ?? 10, cancellationToken)))
.WithName("SuggestGamerNames")
.WithDescription("Suggessts possible and available gamer names")
.WithOpenApi();

// GET /enrich-token
// Query: BeforeIncludingApplicationClaimsRequest
// Response: BeforeIncludingApplicationClaimsResponse
// Description: Enrich and shape the access-token with claims
// Note: This endpoint is used as API-Connector by Azure AD B2C
//       The endpoint is called before the token is issued to the user.
//       The user-groups assigned to the user are received from the configuration (IConfiguration).
app.MapPost("/enrich-token", async (
    BeforeIncludingApplicationClaimsRequest req,
    IConfiguration configuration,
    CancellationToken cancellationToken
) =>
{
    var userGroups = configuration.GetSection($"UserGroupAssignments:{req.ObjectId}").Get<string[]>() ?? [];
    return new BeforeIncludingApplicationClaimsResponse()
    {
        ObjectId = req.ObjectId,
        Email = req.Email,
        //DisplayName = req.DisplayName,
        GivenName = req.GivenName,
        Surname = req.Surname,
        GamerName = req.GamerName,
        UserGroups = userGroups
    };
})
.WithName("EnrichToken")
.WithDescription("Enrich and shape the access-token with claims.")
.WithOpenApi();

app.Run();
