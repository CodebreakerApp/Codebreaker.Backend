using Codebreaker.Proxy;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddSingleton<TokenService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAdB2C"));
    //.EnableTokenAcquisitionToCallDownstreamApi()
    //.AddDownstreamApi("", "")
    //.AddInMemoryTokenCache();

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddMicrosoftIdentityWebApi(bearerOptions =>
//    {
//        builder.Configuration.Bind("AzureAdB2C", bearerOptions);
//        //        bearerOptions.TokenValidationParameters.NameClaimType = "name";
//    }, identityOptions =>
//    {
//        builder.Configuration.Bind("AzureAdB2C", identityOptions);
//    });

builder.Services.AddAuthorizationBuilder()
    .AddFallbackPolicy("fallbackPolicy", config =>
    {
        config.RequireAuthenticatedUser();
    })
    .AddPolicy("playPolicy", config =>
    {
        // TODO: Define scopes
        config.RequireAuthenticatedUser();
    })
   .AddPolicy("rankingPolicy", config =>
   {
       // TODO: Define scopes
        config.RequireAuthenticatedUser();
   })
   .AddPolicy("botPolicy", config =>
   {
       // TODO: Define scopes
       config.RequireAuthenticatedUser();
   })
   .AddPolicy("livePolicy", config =>
   {
       // TODO: Define scopes
       config.RequireAuthenticatedUser();
   })
   .AddPolicy("usersPublicPolicy", config =>
   {
       // Allow anonymous access
   })
   .AddPolicy("usersApiConnectorsPolicy", config =>
   {
       // Basic authentication only for Azure ActiveDirectory B2C API connectors
       config
           .RequireAuthenticatedUser()
           .RequireRole("AzureActiveDirectoryB2C");
   });

builder.AddServiceDefaults();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();


// builder.Services.AddRazorPages(); // authentication UI in Views

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();

app.MapGet("/", () => "YARP Proxy");

app.Run();

// https://github.com/microsoft/woodgrove-groceries
// https://github.com/davidfowl/AspireYarp

