using System.Globalization;
using CodeBreaker.UserService.Extensions;
using CodeBreaker.UserService.Models.Api;
using CodeBreaker.UserService.Options;
using CodeBreaker.UserService.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Aspire
builder.AddServiceDefaults();

// Config
builder.Services.Configure<GamerNameCheckOptions>(builder.Configuration.GetRequiredSection("AzureActiveDirectory"));
builder.Services.Configure<GamerNameSuggestionOptions>(builder.Configuration.GetRequiredSection("GamerNameSuggestion"));

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

// Validation
builder.Services.AddScoped<IValidator<BeforeCreatingUserRequest>, BeforeCreatingUserRequestValidator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

// Aspire endpoints
app.MapDefaultEndpoints();

//
// Username endpoints
//
// POST /validate-before-user-creation
// Request: BeforeCreatingUserRequest
// Response: BeforeCreatingUserSuccessResponse | BeforeCreatingUserValidationErrorResponse
// Description: Checks whether the specified user can be created
app.MapPost("/validate-before-user-creation", async (BeforeCreatingUserRequest request, IValidator<BeforeCreatingUserRequest> requestValidator, CancellationToken cancellationToken) =>
{
    var result = await requestValidator.ValidateAsync(request);

    if (result.IsValid)
        return Results.Ok(new BeforeCreatingUserSuccessResponse());

    return Results.BadRequest(new BeforeCreatingUserValidationErrorResponse(result.ToString())); // ValidationError needs HTTP 400
})
.WithName("CheckGamerName")
.WithDescription("Checks whether the specified gamerName is valid")
.WithOpenApi();

// GET /gamer-names/suggestions
// Query: int? count
// Response: GamerNameSuggestionsResponse
// Description: Suggests possible and available gamer names
app.MapGet("/gamer-names/suggestions", (int? count, IOptions<GamerNameSuggestionOptions> gamerNameSuggestionOptions, CancellationToken cancellationToken) =>
{
    if (count == 0 || count > 100)
        return Results.BadRequest("Count must be between 1 and 100");

    count ??= 10;

    (string[] First, string[] Second) parts = (gamerNameSuggestionOptions.Value.GamerNameParts.First, gamerNameSuggestionOptions.Value.GamerNameParts.Second);
    (int, int, int)[] combinations = new (int, int, int)[count.Value];
    string[] suggestions = new string[count.Value];

    for (int i = 0; i < combinations.Length; i++)
    {
        (int First, int Second, int Third) candidate = (parts.First.GetRandomIndex(), parts.Second.GetRandomIndex(), Random.Shared.Next(gamerNameSuggestionOptions.Value.MinimumNumber, gamerNameSuggestionOptions.Value.MaximumNumber));

        if (combinations.Contains(candidate))
        {
            i--;
            continue;
        }

        combinations[i] = candidate;
        suggestions[i] = $"{parts.First[candidate.First].ToUpperFirstChar()}{parts.Second[candidate.Second]}{candidate.Third}";
    }

    return Results.Ok(new GamerNameSuggestionsResponse(suggestions));
})
.WithName("SuggestGamerNames")
.WithDescription("Suggessts possible and available gamer names")
.WithOpenApi();


if (builder.Configuration["StartupMode"] == "Azure")
{
    // GET /enrich-token
    // Query: BeforeIncludingApplicationClaimsRequest
    // Response: BeforeIncludingApplicationClaimsResponse
    // Description: Enrich and shape the access-token with claims
    // Note: This endpoint is used as API-Connector by Azure AD B2C
    //       The endpoint is called before the token is issued to the user.
    //       The user-groups assigned to the user are received from the configuration (IConfiguration).
    app.MapPost("/enrich-token", (
        BeforeIncludingApplicationClaimsRequest req,
        IConfiguration configuration
    ) =>
    {
        var userGroups = configuration.GetSection($"UserGroupAssignments:{req.ObjectId}").Get<string[]>() ?? [];
        return new BeforeIncludingApplicationClaimsResponse()
        {
            ObjectId = req.ObjectId,
            Email = req.Email,
            GivenName = req.GivenName,
            Surname = req.Surname,
            DisplayName = $"{req.GivenName} {req.Surname}",
            GamerName = req.GamerName,
            UserGroups = userGroups
        };
    })
    .WithName("EnrichToken")
    .WithDescription("Enrich and shape the access-token with claims.")
    .WithOpenApi();
}

app.Run();
