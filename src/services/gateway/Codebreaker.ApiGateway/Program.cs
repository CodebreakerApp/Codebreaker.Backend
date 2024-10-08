using Codebreaker.Proxy;
using idunno.Authentication.Basic;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddAzureKeyVaultSecrets("gateway-keyvault");

builder.Services.AddRazorPages();
builder.Services.AddSingleton<TokenService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
// Basic authentication only for Azure ActiveDirectory B2C API connectors
    .AddBasic("BasicScheme", options =>
    {
        options.Events = new BasicAuthenticationEvents()
        {
            OnValidateCredentials = context =>
            {
                var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                var password = config["AADB2C-ApiConnector-Password"];

                if (string.IsNullOrEmpty(password))
                    throw new InvalidOperationException("AADB2C-ApiConnector-Password is not set in configuration.");

                if (context.Username == "AADB2C" && context.Password == password)
                {
                    Claim[] claims = [
                        new (ClaimTypes.Name, context.Username, ClaimValueTypes.String, context.Options.ClaimsIssuer),
                        new (ClaimTypes.Role, "AzureActiveDirectoryB2C", ClaimValueTypes.String, context.Options.ClaimsIssuer)
                    ];
                    context.Principal = new(new ClaimsIdentity(claims, context.Scheme.Name));
                    context.Success();
                }

                return Task.CompletedTask;
            }
        };
    })
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAdB2C"));

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
           .AddAuthenticationSchemes("BasicScheme")
           .RequireRole("AzureActiveDirectoryB2C");
   });

builder.Services.AddCors(setup => setup.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.AddServiceDefaults();

var reverseProxyBuilder = builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

if (!builder.Environment.IsProduction())
{ 
    reverseProxyBuilder.AddServiceDiscoveryDestinationResolver();
}

// builder.Services.AddRazorPages(); // authentication UI in Views

var app = builder.Build();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();

app.MapGet("/", () => "YARP Proxy");

app.Run();

// https://github.com/microsoft/woodgrove-groceries
// https://github.com/davidfowl/AspireYarp

