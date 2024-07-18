using Codebreaker.Proxy;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddAzureKeyVaultSecrets("gateway-keyvault");

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

// Basic authentication only for Azure ActiveDirectory B2C API connectors
builder.Services.AddAuthentication()
    .AddBasic(options =>
    {
        options.Events.OnValidateCredentials = context =>
        {
            var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            if (context.Username == "AADB2C" && context.Password == config["AADB2C-ApiConnector-Password"])
            {
                Claim[] claims = [
                    new (ClaimTypes.Name, context.Username, ClaimValueTypes.String, context.Options.ClaimsIssuer),
                    new (ClaimTypes.Role, "AzureActiveDirectoryB2C", ClaimValueTypes.String, context.Options.ClaimsIssuer)
                ];
                context.Principal = new(new ClaimsIdentity(claims, context.Scheme.Name));
                context.Success();
            }

            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorizationBuilder()
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

