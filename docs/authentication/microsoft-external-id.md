# Microsoft External ID Configuration Guide

This guide provides comprehensive instructions for configuring Microsoft External ID (formerly Azure AD B2C) authentication across the Codebreaker platform components.

## Table of Contents

- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Gateway Configuration](#gateway-configuration)
- [Token Flow Architecture](#token-flow-architecture)
- [Game APIs Service Configuration](#game-apis-service-configuration)
- [Blazor Client Configuration](#blazor-client-configuration)
- [Desktop Client Configuration](#desktop-client-configuration)
- [Security Best Practices](#security-best-practices)
- [Troubleshooting](#troubleshooting)

## Overview

Microsoft External ID provides a comprehensive identity and access management solution for external-facing applications. The Codebreaker platform uses External ID to:

- Authenticate users across web and native clients
- Provide secure token-based API access
- Enable social identity provider integration
- Support anonymous and authenticated user scenarios

### Architecture Components

- **Gateway (YARP Reverse Proxy)**: Entry point for all API requests, handles JWT validation
- **Game APIs**: Backend services protected by JWT authentication
- **Identity Service**: Manages anonymous user creation and promotion
- **Clients**: Web (Blazor), Desktop (WPF, MAUI, Uno, WinUI)

## Prerequisites

Before configuring Microsoft External ID, ensure you have:

1. **Azure Subscription**: Active Azure subscription with permissions to create resources
2. **Microsoft Entra ID Tenant**: Either create a new tenant or use existing
3. **App Registrations**: Create separate app registrations for:
   - Gateway/API (backend)
   - Each client platform (web, WPF, MAUI, etc.)

### Setting Up Microsoft External ID

1. Navigate to [Azure Portal](https://portal.azure.com)
2. Go to **Microsoft Entra ID** → **External Identities**
3. Create or configure your External ID tenant
4. Configure sign-up/sign-in user flows (e.g., `B2C_1_SUSI`)

## Gateway Configuration

The Gateway service (YARP reverse proxy) acts as the authentication entry point for all backend services.

### 1. Configure appsettings.json

Add the following configuration to your Gateway's `appsettings.json`:

```json
{
  "AzureAdB2C": {
    "Instance": "https://<your-tenant-name>.b2clogin.com",
    "Domain": "<your-tenant-name>.onmicrosoft.com",
    "ClientId": "<gateway-app-client-id>",
    "SignedOutCallbackPath": "/signout/B2C_1_SUSI",
    "SignUpSignInPolicyId": "B2C_1_SUSI"
  }
}
```

**Configuration Parameters:**

| Parameter | Description | Example |
|-----------|-------------|---------|
| `Instance` | B2C login endpoint | `https://codebreaker3000.b2clogin.com` |
| `Domain` | Your B2C tenant domain | `codebreaker3000.onmicrosoft.com` |
| `ClientId` | Application (client) ID from app registration | `f528866c-c051-4e1e-8309-91831d52d8b5` |
| `SignedOutCallbackPath` | Callback path after sign-out | `/signout/B2C_1_SUSI` |
| `SignUpSignInPolicyId` | User flow name | `B2C_1_SUSI` |

### 2. Configure Program.cs

The Gateway uses Microsoft.Identity.Web for JWT bearer authentication:

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Add Azure Key Vault for secrets management
builder.Configuration.AddAzureKeyVaultSecrets("gateway-keyvault");

// Configure JWT Bearer authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAdB2C"));

// Define authorization policies
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("playPolicy", config =>
    {
        config.RequireAuthenticatedUser();
        // Optional: Add scope requirements
        // config.RequireScope("Games.Play");
    })
    .AddPolicy("rankingPolicy", config =>
    {
        config.RequireAuthenticatedUser();
    })
    .AddPolicy("botPolicy", config =>
    {
        config.RequireAuthenticatedUser();
    })
    .AddPolicy("livePolicy", config =>
    {
        config.RequireAuthenticatedUser();
    });

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();
```

### 3. Configure Reverse Proxy Routes

Update `appsettings.json` to apply authorization policies to routes:

```json
{
  "ReverseProxy": {
    "Routes": {
      "gamesRoute": {
        "ClusterId": "gamesapicluster",
        "AuthorizationPolicy": "playPolicy",
        "Match": {
          "Path": "/games/{*any}"
        }
      },
      "rankingRoute": {
        "ClusterId": "rankingcluster",
        "AuthorizationPolicy": "rankingPolicy",
        "Match": {
          "Path": "/ranking/{*any}"
        }
      }
    },
    "Clusters": {
      "gamesapicluster": {
        "Destinations": {
          "gamescluster/destination1": {
            "Address": "http://gameapis"
          }
        }
      }
    }
  }
}
```

### 4. Configure API Connectors (Optional)

For B2C API connectors that validate or enrich user data during sign-up:

```csharp
// Add Basic Authentication for B2C API connectors
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddBasic("BasicScheme", options =>
    {
        options.Events = new BasicAuthenticationEvents()
        {
            OnValidateCredentials = context =>
            {
                var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                var password = config["AADB2C-ApiConnector-Password"];

                if (context.Username == "AADB2C" && context.Password == password)
                {
                    Claim[] claims = [
                        new(ClaimTypes.Name, context.Username, ClaimValueTypes.String),
                        new(ClaimTypes.Role, "AzureActiveDirectoryB2C", ClaimValueTypes.String)
                    ];
                    context.Principal = new(new ClaimsIdentity(claims, context.Scheme.Name));
                    context.Success();
                }
                return Task.CompletedTask;
            }
        };
    })
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAdB2C"));

// Add policy for API connectors
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("usersApiConnectorsPolicy", config =>
    {
        config.RequireAuthenticatedUser()
              .AddAuthenticationSchemes("BasicScheme")
              .RequireRole("AzureActiveDirectoryB2C");
    });
```

**Important**: Store the `AADB2C-ApiConnector-Password` in Azure Key Vault, not in configuration files.

### 5. Key Vault Configuration

Store sensitive configuration in Azure Key Vault:

```bash
# Add secrets to Key Vault
az keyvault secret set --vault-name gateway-keyvault \
  --name "AADB2C-ApiConnector-Password" \
  --value "<secure-password>"
```

In your app registration:
1. Enable managed identity for the Gateway service
2. Grant Key Vault access policy to the managed identity
3. Use `builder.Configuration.AddAzureKeyVaultSecrets("gateway-keyvault")` to load secrets

## Token Flow Architecture

Understanding the token flow is crucial for proper authentication implementation.

### Authentication Flow Diagram

```
┌─────────────┐         ┌──────────────┐         ┌─────────────┐         ┌─────────────┐
│   Client    │         │   Gateway    │         │  Game APIs  │         │  External   │
│ (Web/Native)│         │    (YARP)    │         │  Service    │         │     ID      │
└──────┬──────┘         └──────┬───────┘         └──────┬──────┘         └──────┬──────┘
       │                       │                        │                        │
       │ 1. Initiate Login     │                        │                        │
       ├──────────────────────>│                        │                        │
       │                       │                        │                        │
       │ 2. Redirect to B2C    │                        │                        │
       │<──────────────────────┤                        │                        │
       │                       │                        │                        │
       │ 3. User authenticates │                        │                        │
       ├───────────────────────┼────────────────────────┼───────────────────────>│
       │                       │                        │                        │
       │ 4. Return JWT token   │                        │                        │
       │<──────────────────────┼────────────────────────┼────────────────────────┤
       │                       │                        │                        │
       │ 5. API Request + JWT  │                        │                        │
       ├──────────────────────>│                        │                        │
       │                       │ 6. Validate JWT        │                        │
       │                       ├──────┐                 │                        │
       │                       │<─────┘                 │                        │
       │                       │                        │                        │
       │                       │ 7. Forward Request + JWT│                       │
       │                       ├───────────────────────>│                        │
       │                       │                        │ 8. Optional: Extract   │
       │                       │                        │    claims from JWT     │
       │                       │                        ├──────┐                 │
       │                       │                        │<─────┘                 │
       │                       │                        │                        │
       │                       │ 9. Return Response     │                        │
       │                       │<───────────────────────┤                        │
       │                       │                        │                        │
       │ 10. Return to Client  │                        │                        │
       │<──────────────────────┤                        │                        │
```

### Token Propagation from Gateway to APIs

The Gateway automatically forwards the JWT token to backend services. Here's how it works:

#### 1. YARP Token Forwarding (Default)

By default, YARP forwards the `Authorization` header with the JWT token:

```csharp
// In Gateway's appsettings.json - YARP automatically forwards headers
{
  "ReverseProxy": {
    "Routes": {
      "gamesRoute": {
        "ClusterId": "gamesapicluster",
        "AuthorizationPolicy": "playPolicy",
        "Match": {
          "Path": "/games/{*any}"
        }
        // No special transform needed - Authorization header forwarded automatically
      }
    }
  }
}
```

#### 2. Custom Token Forwarding (Advanced)

For custom token handling or transformation:

```csharp
// In Gateway's TokenService.cs
public class TokenService
{
    public Task<string?> GetAuthTokenAsync(ClaimsPrincipal user)
    {
        // Extract claims from the authenticated user
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = user.FindFirst(ClaimTypes.Email)?.Value;
        
        // The original JWT is already in the Authorization header
        // This service can be used for additional token operations
        
        return Task.FromResult<string?>(userId);
    }
}
```

#### 3. Downstream API Configuration

To explicitly configure token forwarding:

```csharp
// If using Microsoft.Identity.Web.DownstreamApi
builder.Services.AddDownstreamApi("GamesApi", builder.Configuration.GetSection("GamesApi"))
    .AddMicrosoftIdentityWebApiAuthentication(builder.Configuration.GetSection("AzureAdB2C"));
```

## Game APIs Service Configuration

The Game APIs service receives and validates JWT tokens forwarded from the Gateway.

### Configuration Options

#### Option 1: No Authentication (Gateway Handles All)

If the Gateway is the only entry point and handles all authentication:

```csharp
// In Game APIs Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// No authentication middleware needed
// Gateway validates tokens before forwarding requests

builder.AddApplicationServices();

var app = builder.Build();

app.MapGameEndpoints();
app.Run();
```

**Use Case**: When Game APIs are not exposed publicly and only accessible through the Gateway.

#### Option 2: Token Validation (Defense in Depth)

For additional security, validate tokens at the API level:

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Add JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAdB2C"));

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGameEndpoints()
   .RequireAuthorization(); // Require authentication for all game endpoints

app.Run();
```

Add to `appsettings.json`:

```json
{
  "AzureAdB2C": {
    "Instance": "https://<your-tenant-name>.b2clogin.com",
    "Domain": "<your-tenant-name>.onmicrosoft.com",
    "ClientId": "<api-app-client-id>",
    "SignUpSignInPolicyId": "B2C_1_SUSI"
  }
}
```

### Accessing User Claims in Game APIs

```csharp
// In your endpoint handlers
app.MapPost("/games", async (HttpContext context, IGameService gameService) =>
{
    var user = context.User;
    
    // Extract user information from claims
    var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var email = user.FindFirst(ClaimTypes.Email)?.Value;
    var name = user.FindFirst(ClaimTypes.Name)?.Value;
    
    // Use user information in your business logic
    var game = await gameService.CreateGameAsync(userId, name);
    
    return Results.Ok(game);
});
```

## Blazor Client Configuration

Blazor applications can be hosted as Server or WebAssembly (WASM). Each has different authentication patterns.

### Blazor Server Configuration

Blazor Server uses server-side rendering with SignalR for interactivity.

#### 1. Install NuGet Packages

```bash
dotnet add package Microsoft.Identity.Web
dotnet add package Microsoft.Identity.Web.UI
```

#### 2. Configure appsettings.json

```json
{
  "AzureAdB2C": {
    "Instance": "https://<your-tenant-name>.b2clogin.com",
    "Domain": "<your-tenant-name>.onmicrosoft.com",
    "ClientId": "<blazor-server-client-id>",
    "CallbackPath": "/signin-oidc",
    "SignedOutCallbackPath": "/signout-callback-oidc",
    "SignUpSignInPolicyId": "B2C_1_SUSI"
  }
}
```

#### 3. Configure Program.cs

```csharp
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

var builder = WebApplication.CreateBuilder(args);

// Add authentication
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAdB2C"));

builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});

// Add Blazor Server
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor()
    .AddMicrosoftIdentityConsentHandler();

// Configure HttpClient to call APIs
builder.Services.AddHttpClient<IGameClient, GameClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["GatewayUrl"] ?? "https://localhost:7000");
})
.AddHttpMessageHandler<AuthorizationMessageHandler>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
```

#### 4. Configure AuthorizationMessageHandler

Create a custom message handler to attach tokens to API calls:

```csharp
public class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthorizationMessageHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        
        if (httpContext != null)
        {
            // Get the access token from the authenticated user
            var accessToken = await httpContext.GetTokenAsync("access_token");
            
            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
```

#### 5. Use Authentication in Components

```razor
@page "/"
@using Microsoft.AspNetCore.Components.Authorization
@inject AuthenticationStateProvider AuthenticationStateProvider
@inject IGameClient GameClient

<AuthorizeView>
    <Authorized>
        <h3>Welcome, @context.User.Identity?.Name!</h3>
        <button @onclick="StartGame">Start Game</button>
    </Authorized>
    <NotAuthorized>
        <h3>Please log in to play</h3>
        <a href="MicrosoftIdentity/Account/SignIn">Log in</a>
    </NotAuthorized>
</AuthorizeView>

@code {
    private async Task StartGame()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        
        // Call API with authentication
        var game = await GameClient.StartGameAsync("Game6x4", user.Identity?.Name ?? "Player");
    }
}
```

### Blazor WebAssembly (WASM) Configuration

Blazor WASM runs entirely in the browser and requires MSAL.js for authentication.

#### 1. Install NuGet Packages

```bash
dotnet add package Microsoft.Authentication.WebAssembly.Msal
```

#### 2. Configure wwwroot/appsettings.json

```json
{
  "AzureAdB2C": {
    "Authority": "https://<your-tenant-name>.b2clogin.com/<your-tenant-name>.onmicrosoft.com/B2C_1_SUSI",
    "ClientId": "<blazor-wasm-client-id>",
    "ValidateAuthority": false
  },
  "GatewayUrl": "https://your-gateway-url.azurecontainerapps.io"
}
```

#### 3. Configure Program.cs

```csharp
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Authentication.WebAssembly.Msal;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure MSAL authentication
builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAdB2C", options.ProviderOptions.Authentication);
    options.ProviderOptions.DefaultAccessTokenScopes.Add("openid");
    options.ProviderOptions.DefaultAccessTokenScopes.Add("profile");
    // Add API scopes
    options.ProviderOptions.DefaultAccessTokenScopes.Add("https://<your-tenant>.onmicrosoft.com/<api-client-id>/Games.Play");
});

// Configure HttpClient with authentication
builder.Services.AddHttpClient<IGameClient, GameClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["GatewayUrl"] ?? "https://localhost:7000");
})
.AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

// Configure the handler to attach tokens
builder.Services.AddScoped<BaseAddressAuthorizationMessageHandler>();

await builder.Build().RunAsync();
```

#### 4. Configure App.razor

```razor
<CascadingAuthenticationState>
    <Router AppAssembly="@typeof(App).Assembly">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)">
                <NotAuthorized>
                    @if (context.User.Identity?.IsAuthenticated != true)
                    {
                        <RedirectToLogin />
                    }
                    else
                    {
                        <p>You are not authorized to access this resource.</p>
                    }
                </NotAuthorized>
            </AuthorizeRouteView>
        </Found>
    </Router>
</CascadingAuthenticationState>
```

#### 5. Add Authentication UI

Create `Pages/Authentication.razor`:

```razor
@page "/authentication/{action}"
@using Microsoft.AspNetCore.Components.WebAssembly.Authentication

<RemoteAuthenticatorView Action="@Action" />

@code {
    [Parameter] public string? Action { get; set; }
}
```

### Blazor Hybrid (MAUI Blazor) Configuration

For Blazor Hybrid apps running in MAUI, see the [.NET MAUI Configuration](#net-maui-configuration) section.

## Desktop Client Configuration

Desktop clients (WPF, MAUI, Uno Platform, WinUI) use MSAL.NET for authentication.

### WPF Configuration

#### 1. Install NuGet Packages

```bash
dotnet add package Microsoft.Identity.Client
dotnet add package Microsoft.Identity.Client.Desktop
```

#### 2. Create Authentication Service

```csharp
using Microsoft.Identity.Client;
using System.Linq;
using System.Threading.Tasks;

public class AuthenticationService
{
    private readonly IPublicClientApplication _app;
    private readonly string[] _scopes;

    public AuthenticationService(string clientId, string authority, string[] scopes)
    {
        _scopes = scopes;
        
        _app = PublicClientApplicationBuilder
            .Create(clientId)
            .WithAuthority(authority)
            .WithDefaultRedirectUri() // Uses http://localhost for WPF
            .WithParentActivityOrWindow(() => System.Windows.Application.Current.MainWindow)
            .Build();

        // Enable token cache serialization
        TokenCacheHelper.EnableSerialization(_app.UserTokenCache);
    }

    public async Task<AuthenticationResult> SignInAsync()
    {
        AuthenticationResult result;
        
        try
        {
            // Try to acquire token silently first
            var accounts = await _app.GetAccountsAsync();
            result = await _app.AcquireTokenSilent(_scopes, accounts.FirstOrDefault())
                .ExecuteAsync();
        }
        catch (MsalUiRequiredException)
        {
            // Interactive authentication required
            result = await _app.AcquireTokenInteractive(_scopes)
                .ExecuteAsync();
        }

        return result;
    }

    public async Task<AuthenticationResult?> AcquireTokenSilentAsync()
    {
        try
        {
            var accounts = await _app.GetAccountsAsync();
            var account = accounts.FirstOrDefault();
            
            if (account == null)
                return null;

            return await _app.AcquireTokenSilent(_scopes, account)
                .ExecuteAsync();
        }
        catch (MsalException)
        {
            return null;
        }
    }

    public async Task SignOutAsync()
    {
        var accounts = await _app.GetAccountsAsync();
        
        foreach (var account in accounts)
        {
            await _app.RemoveAsync(account);
        }
    }
}
```

#### 3. Token Cache Helper

```csharp
using Microsoft.Identity.Client;
using System.IO;
using System.Security.Cryptography;

public static class TokenCacheHelper
{
    private static readonly string CacheFilePath = 
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Codebreaker", "msal_cache.dat");

    public static void EnableSerialization(ITokenCache tokenCache)
    {
        tokenCache.SetBeforeAccess(BeforeAccessNotification);
        tokenCache.SetAfterAccess(AfterAccessNotification);
    }

    private static void BeforeAccessNotification(TokenCacheNotificationArgs args)
    {
        if (File.Exists(CacheFilePath))
        {
            var data = File.ReadAllBytes(CacheFilePath);
            var decryptedData = ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
            args.TokenCache.DeserializeMsalV3(decryptedData);
        }
    }

    private static void AfterAccessNotification(TokenCacheNotificationArgs args)
    {
        if (args.HasStateChanged)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(CacheFilePath)!);
            var data = args.TokenCache.SerializeMsalV3();
            var encryptedData = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(CacheFilePath, encryptedData);
        }
    }
}
```

#### 4. Configure in App.xaml.cs

```csharp
public partial class App : Application
{
    private AuthenticationService? _authService;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Configuration
        var clientId = "<wpf-client-id>";
        var authority = "https://<your-tenant-name>.b2clogin.com/tfp/<your-tenant-name>.onmicrosoft.com/B2C_1_SUSI";
        var scopes = new[] 
        { 
            "openid", 
            "profile",
            "https://<your-tenant>.onmicrosoft.com/<api-client-id>/Games.Play"
        };

        _authService = new AuthenticationService(clientId, authority, scopes);

        // Configure HttpClient for API calls
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://your-gateway-url.azurecontainerapps.io")
        };

        // Register services
        ServiceLocator.RegisterAuthService(_authService);
        ServiceLocator.RegisterGameClient(new GameClient(httpClient, _authService));
    }
}
```

#### 5. Use in MainWindow

```csharp
public partial class MainWindow : Window
{
    private readonly AuthenticationService _authService;
    private readonly IGameClient _gameClient;

    public MainWindow()
    {
        InitializeComponent();
        
        _authService = ServiceLocator.GetAuthService();
        _gameClient = ServiceLocator.GetGameClient();
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var result = await _authService.SignInAsync();
            
            // Update UI
            UsernameText.Text = result.Account.Username;
            LoginButton.Visibility = Visibility.Collapsed;
            LogoutButton.Visibility = Visibility.Visible;
            
            // Store access token for API calls
            ServiceLocator.SetAccessToken(result.AccessToken);
        }
        catch (MsalException ex)
        {
            MessageBox.Show($"Login failed: {ex.Message}");
        }
    }

    private async void StartGameButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Ensure we have a valid token
            var result = await _authService.AcquireTokenSilentAsync();
            
            if (result == null)
            {
                MessageBox.Show("Please sign in first");
                return;
            }

            // Make API call
            var game = await _gameClient.StartGameAsync("Game6x4", result.Account.Username);
            
            // Update UI with game info
            GameIdText.Text = game.Id.ToString();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error starting game: {ex.Message}");
        }
    }

    private async void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        await _authService.SignOutAsync();
        
        // Update UI
        UsernameText.Text = string.Empty;
        LoginButton.Visibility = Visibility.Visible;
        LogoutButton.Visibility = Visibility.Collapsed;
    }
}
```

#### 6. Configure HttpClient with Token

```csharp
public class GameClient : IGameClient
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationService _authService;

    public GameClient(HttpClient httpClient, AuthenticationService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
    }

    private async Task<HttpClient> GetAuthenticatedClientAsync()
    {
        var result = await _authService.AcquireTokenSilentAsync();
        
        if (result != null)
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.AccessToken);
        }

        return _httpClient;
    }

    public async Task<GameInfo> StartGameAsync(string gameType, string playerName)
    {
        var client = await GetAuthenticatedClientAsync();
        var response = await client.PostAsJsonAsync("/games/start", new { gameType, playerName });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GameInfo>();
    }
}
```

### .NET MAUI Configuration

.NET MAUI supports multiple platforms (iOS, Android, Windows, macOS) with a single codebase.

#### 1. Install NuGet Packages

```bash
dotnet add package Microsoft.Identity.Client
```

#### 2. Platform-Specific Configuration

##### Android (Platforms/Android/MainActivity.cs)

```csharp
using Android.App;
using Android.Content;
using Android.Content.PM;
using Microsoft.Identity.Client;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, 
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
[IntentFilter(new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault },
    DataHost = "auth",
    DataScheme = "msauth")]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(requestCode, resultCode, data);
    }
}
```

##### iOS (Platforms/iOS/AppDelegate.cs)

```csharp
using Foundation;
using Microsoft.Identity.Client;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool OpenUrl(UIApplication application, NSUrl url, NSDictionary options)
    {
        if (AuthenticationContinuationHelper.IsBrokerResponse(null))
        {
            AuthenticationContinuationHelper.SetBrokerContinuationEventArgs(url);
            return true;
        }

        return false;
    }
}
```

##### Windows

No additional platform-specific configuration required.

#### 3. Create MAUI Authentication Service

```csharp
using Microsoft.Identity.Client;

public class MauiAuthenticationService
{
    private readonly IPublicClientApplication _app;
    private readonly string[] _scopes;

    public MauiAuthenticationService(string clientId, string tenantId, string[] scopes)
    {
        _scopes = scopes;

        var builder = PublicClientApplicationBuilder
            .Create(clientId)
            .WithB2CAuthority($"https://<your-tenant>.b2clogin.com/tfp/{tenantId}/B2C_1_SUSI")
            .WithRedirectUri($"msauth.com.codebreakerapp.maui://auth");

#if ANDROID
        builder = builder.WithParentActivityOrWindow(() => Platform.CurrentActivity);
#elif IOS
        builder = builder.WithIosKeychainSecurityGroup("com.microsoft.adalcache");
#endif

        _app = builder.Build();
    }

    public async Task<AuthenticationResult> SignInAsync()
    {
        AuthenticationResult result;

        try
        {
            var accounts = await _app.GetAccountsAsync();
            result = await _app.AcquireTokenSilent(_scopes, accounts.FirstOrDefault())
                .ExecuteAsync();
        }
        catch (MsalUiRequiredException)
        {
            result = await _app.AcquireTokenInteractive(_scopes)
#if ANDROID
                .WithParentActivityOrWindow(Platform.CurrentActivity)
#endif
                .ExecuteAsync();
        }

        return result;
    }

    public async Task<AuthenticationResult?> GetTokenSilentlyAsync()
    {
        try
        {
            var accounts = await _app.GetAccountsAsync();
            return await _app.AcquireTokenSilent(_scopes, accounts.FirstOrDefault())
                .ExecuteAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task SignOutAsync()
    {
        var accounts = await _app.GetAccountsAsync();
        foreach (var account in accounts)
        {
            await _app.RemoveAsync(account);
        }
    }
}
```

#### 4. Configure in MauiProgram.cs

```csharp
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Authentication configuration
        var clientId = "<maui-client-id>";
        var tenantId = "<your-tenant>.onmicrosoft.com";
        var scopes = new[]
        {
            "openid",
            "profile",
            "https://<your-tenant>.onmicrosoft.com/<api-client-id>/Games.Play"
        };

        builder.Services.AddSingleton(new MauiAuthenticationService(clientId, tenantId, scopes));

        // HTTP client for API calls
        builder.Services.AddHttpClient<IGameClient, GameClient>(client =>
        {
            client.BaseAddress = new Uri("https://your-gateway-url.azurecontainerapps.io");
        });

        return builder.Build();
    }
}
```

#### 5. Use in MAUI Page

```csharp
public partial class MainPage : ContentPage
{
    private readonly MauiAuthenticationService _authService;
    private readonly IGameClient _gameClient;

    public MainPage(MauiAuthenticationService authService, IGameClient gameClient)
    {
        InitializeComponent();
        _authService = authService;
        _gameClient = gameClient;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await _authService.SignInAsync();
            
            UsernameLabel.Text = $"Welcome, {result.Account.Username}";
            LoginButton.IsVisible = false;
            LogoutButton.IsVisible = true;
            StartGameButton.IsEnabled = true;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Login failed: {ex.Message}", "OK");
        }
    }

    private async void OnStartGameClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await _authService.GetTokenSilentlyAsync();
            
            if (result == null)
            {
                await DisplayAlert("Error", "Please sign in first", "OK");
                return;
            }

            var game = await _gameClient.StartGameAsync("Game6x4", result.Account.Username);
            await DisplayAlert("Success", $"Game started: {game.Id}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to start game: {ex.Message}", "OK");
        }
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await _authService.SignOutAsync();
        
        UsernameLabel.Text = string.Empty;
        LoginButton.IsVisible = true;
        LogoutButton.IsVisible = false;
        StartGameButton.IsEnabled = false;
    }
}
```

#### 6. Configure App Redirect URIs

In Azure Portal app registration:

- **Android**: `msauth://com.codebreakerapp.maui/<PACKAGE_SIGNATURE_HASH>`
- **iOS**: `msauth.com.codebreakerapp.maui://auth`
- **Windows**: `https://login.microsoftonline.com/common/oauth2/nativeclient`

### Uno Platform Configuration

Uno Platform supports multiple platforms with a single codebase using WinUI/UWP APIs.

#### 1. Install NuGet Packages

```bash
dotnet add package Microsoft.Identity.Client
dotnet add package Uno.Extensions.Authentication.MSAL
```

#### 2. Configure Authentication

```csharp
// In App.xaml.cs or host initialization
public class App : Application
{
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Configure authentication
        var host = new HostBuilder()
            .ConfigureServices(services =>
            {
                // Configure MSAL authentication
                services.AddSingleton<IAuthenticationService>(sp =>
                {
                    var clientId = "<uno-client-id>";
                    var authority = "https://<your-tenant>.b2clogin.com/tfp/<your-tenant>.onmicrosoft.com/B2C_1_SUSI";
                    
                    var app = PublicClientApplicationBuilder
                        .Create(clientId)
                        .WithB2CAuthority(authority)
                        .WithRedirectUri("msal<client-id>://auth")
                        .Build();

                    return new MsalAuthenticationService(app);
                });

                // Configure HttpClient
                services.AddHttpClient<IGameClient, GameClient>(client =>
                {
                    client.BaseAddress = new Uri("https://your-gateway-url.azurecontainerapps.io");
                });
            })
            .Build();

        // Store host for service resolution
        ServiceProvider = host.Services;
    }

    public static IServiceProvider? ServiceProvider { get; private set; }
}
```

#### 3. Authentication Service Wrapper

```csharp
public class MsalAuthenticationService : IAuthenticationService
{
    private readonly IPublicClientApplication _app;
    private readonly string[] _scopes = 
    { 
        "openid", 
        "profile",
        "https://<your-tenant>.onmicrosoft.com/<api-client-id>/Games.Play"
    };

    public MsalAuthenticationService(IPublicClientApplication app)
    {
        _app = app;
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        try
        {
            var accounts = await _app.GetAccountsAsync();
            var result = await _app.AcquireTokenSilent(_scopes, accounts.FirstOrDefault())
                .ExecuteAsync();
            return result.AccessToken;
        }
        catch (MsalUiRequiredException)
        {
            var result = await _app.AcquireTokenInteractive(_scopes)
                .ExecuteAsync();
            return result.AccessToken;
        }
    }

    public async Task SignOutAsync()
    {
        var accounts = await _app.GetAccountsAsync();
        foreach (var account in accounts)
        {
            await _app.RemoveAsync(account);
        }
    }
}
```

#### 4. Use in Page

```xml
<!-- MainPage.xaml -->
<Page x:Class="CodebreakerApp.MainPage"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <StackPanel>
        <TextBlock x:Name="UsernameText" Text="Not signed in" />
        <Button x:Name="LoginButton" Content="Login" Click="OnLoginClick" />
        <Button x:Name="StartGameButton" Content="Start Game" Click="OnStartGameClick" IsEnabled="False" />
        <Button x:Name="LogoutButton" Content="Logout" Click="OnLogoutClick" Visibility="Collapsed" />
    </StackPanel>
</Page>
```

```csharp
// MainPage.xaml.cs
public sealed partial class MainPage : Page
{
    private readonly IAuthenticationService _authService;
    private readonly IGameClient _gameClient;

    public MainPage()
    {
        InitializeComponent();
        
        _authService = (IAuthenticationService)App.ServiceProvider!.GetService(typeof(IAuthenticationService))!;
        _gameClient = (IGameClient)App.ServiceProvider!.GetService(typeof(IGameClient))!;
    }

    private async void OnLoginClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var token = await _authService.GetAccessTokenAsync();
            
            if (!string.IsNullOrEmpty(token))
            {
                UsernameText.Text = "Signed in";
                LoginButton.Visibility = Visibility.Collapsed;
                LogoutButton.Visibility = Visibility.Visible;
                StartGameButton.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            await new ContentDialog
            {
                Title = "Error",
                Content = $"Login failed: {ex.Message}",
                CloseButtonText = "OK"
            }.ShowAsync();
        }
    }

    private async void OnStartGameClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var game = await _gameClient.StartGameAsync("Game6x4", "Player");
            
            await new ContentDialog
            {
                Title = "Success",
                Content = $"Game started: {game.Id}",
                CloseButtonText = "OK"
            }.ShowAsync();
        }
        catch (Exception ex)
        {
            await new ContentDialog
            {
                Title = "Error",
                Content = $"Failed to start game: {ex.Message}",
                CloseButtonText = "OK"
            }.ShowAsync();
        }
    }

    private async void OnLogoutClick(object sender, RoutedEventArgs e)
    {
        await _authService.SignOutAsync();
        
        UsernameText.Text = "Not signed in";
        LoginButton.Visibility = Visibility.Visible;
        LogoutButton.Visibility = Visibility.Collapsed;
        StartGameButton.IsEnabled = false;
    }
}
```

### WinUI 3 Configuration

WinUI 3 is the native Windows UI framework.

#### 1. Install NuGet Packages

```bash
dotnet add package Microsoft.Identity.Client
dotnet add package Microsoft.Identity.Client.Desktop
```

#### 2. Configure in App.xaml.cs

```csharp
using Microsoft.Identity.Client;
using Microsoft.UI.Xaml;

public partial class App : Application
{
    private IPublicClientApplication? _app;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Configure MSAL
        _app = PublicClientApplicationBuilder
            .Create("<winui-client-id>")
            .WithB2CAuthority("https://<your-tenant>.b2clogin.com/tfp/<your-tenant>.onmicrosoft.com/B2C_1_SUSI")
            .WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
            .Build();

        m_window = new MainWindow(_app);
        m_window.Activate();
    }

    private Window? m_window;
}
```

#### 3. Use in MainWindow

```csharp
using Microsoft.Identity.Client;
using Microsoft.UI.Xaml;
using System;
using System.Linq;
using System.Threading.Tasks;

public sealed partial class MainWindow : Window
{
    private readonly IPublicClientApplication _app;
    private readonly string[] _scopes = 
    { 
        "openid", 
        "profile",
        "https://<your-tenant>.onmicrosoft.com/<api-client-id>/Games.Play"
    };

    public MainWindow(IPublicClientApplication app)
    {
        InitializeComponent();
        _app = app;
        Title = "Codebreaker";
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var result = await AcquireTokenAsync();
            
            UsernameText.Text = $"Welcome, {result.Account.Username}";
            LoginButton.Visibility = Visibility.Collapsed;
            LogoutButton.Visibility = Visibility.Visible;
            StartGameButton.IsEnabled = true;
        }
        catch (Exception ex)
        {
            await ShowErrorDialog($"Login failed: {ex.Message}");
        }
    }

    private async Task<AuthenticationResult> AcquireTokenAsync()
    {
        try
        {
            var accounts = await _app.GetAccountsAsync();
            return await _app.AcquireTokenSilent(_scopes, accounts.FirstOrDefault())
                .ExecuteAsync();
        }
        catch (MsalUiRequiredException)
        {
            // Get window handle for WinUI
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            
            return await _app.AcquireTokenInteractive(_scopes)
                .WithParentActivityOrWindow(hwnd)
                .ExecuteAsync();
        }
    }

    private async void StartGameButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var result = await AcquireTokenAsync();
            
            // Use token to call API
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.AccessToken);
            httpClient.BaseAddress = new Uri("https://your-gateway-url.azurecontainerapps.io");

            var response = await httpClient.PostAsJsonAsync("/games/start", 
                new { gameType = "Game6x4", playerName = result.Account.Username });
            
            if (response.IsSuccessStatusCode)
            {
                await ShowSuccessDialog("Game started successfully!");
            }
        }
        catch (Exception ex)
        {
            await ShowErrorDialog($"Failed to start game: {ex.Message}");
        }
    }

    private async void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        var accounts = await _app.GetAccountsAsync();
        foreach (var account in accounts)
        {
            await _app.RemoveAsync(account);
        }
        
        UsernameText.Text = string.Empty;
        LoginButton.Visibility = Visibility.Visible;
        LogoutButton.Visibility = Visibility.Collapsed;
        StartGameButton.IsEnabled = false;
    }

    private async Task ShowErrorDialog(string message)
    {
        var dialog = new ContentDialog
        {
            Title = "Error",
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = Content.XamlRoot
        };
        await dialog.ShowAsync();
    }

    private async Task ShowSuccessDialog(string message)
    {
        var dialog = new ContentDialog
        {
            Title = "Success",
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = Content.XamlRoot
        };
        await dialog.ShowAsync();
    }
}
```

## Security Best Practices

### 1. Token Security

- **Never store tokens in plain text**: Use platform-specific secure storage
  - Windows: DPAPI (Data Protection API)
  - iOS: Keychain
  - Android: Keystore
  - Web: HTTPOnly cookies or secure session storage

- **Use token caching**: MSAL.NET provides built-in token caching with encryption

- **Implement token refresh**: Always try silent token acquisition before interactive

### 2. Secrets Management

- **Use Azure Key Vault**: Store sensitive configuration (client secrets, connection strings)
- **Enable Managed Identity**: Avoid storing credentials in code or configuration
- **Rotate secrets regularly**: Set up secret rotation policies

```csharp
// Good: Load from Key Vault
builder.Configuration.AddAzureKeyVault("gateway-keyvault");

// Bad: Hardcode in configuration
// "ClientSecret": "super-secret-value-123"
```

### 3. API Security

- **Validate tokens at multiple layers**: Gateway AND API services
- **Use authorization policies**: Don't just check authentication
- **Implement scope-based access**: Use OAuth scopes for fine-grained permissions

```csharp
// Good: Scope-based authorization
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("playPolicy", config =>
    {
        config.RequireAuthenticatedUser();
        config.RequireScope("Games.Play");
    });

// Better: Role and scope based
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("adminPolicy", config =>
    {
        config.RequireAuthenticatedUser();
        config.RequireRole("Admin");
        config.RequireScope("Games.Admin");
    });
```

### 4. CORS Configuration

- **Be specific with origins**: Avoid `AllowAnyOrigin()` in production

```csharp
// Development
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// Production
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(
            "https://blazor.codebreaker.app",
            "https://codebreaker.app"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});
```

### 5. Redirect URI Security

- **Use HTTPS in production**: HTTP only for local development
- **Register exact URIs**: Don't use wildcards
- **Platform-specific URIs**: Different URIs for each platform

```
Web: https://blazor.codebreaker.app/signin-oidc
Desktop: https://login.microsoftonline.com/common/oauth2/nativeclient
iOS: msauth.com.codebreakerapp://auth
Android: msauth://com.codebreakerapp/<SIGNATURE_HASH>
```

### 6. Token Validation Configuration

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(options =>
    {
        builder.Configuration.Bind("AzureAdB2C", options);
        
        // Token validation parameters
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    }, 
    options => { builder.Configuration.Bind("AzureAdB2C", options); });
```

### 7. Logging and Monitoring

- **Log authentication events**: Track sign-ins, sign-outs, failures
- **Monitor token usage**: Track API calls with invalid tokens
- **Set up alerts**: Alert on suspicious authentication patterns

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILogger<Program>>();
                logger.LogWarning("Authentication failed: {Error}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Token validated for user: {User}", 
                    context.Principal?.Identity?.Name);
                return Task.CompletedTask;
            }
        };
    }, 
    options => { builder.Configuration.Bind("AzureAdB2C", options); });
```

## Troubleshooting

### Common Issues and Solutions

#### 1. "The access token provided is not valid" Error

**Cause**: Token validation failure

**Solutions**:
- Verify `ClientId` matches the app registration
- Check `Instance` and `Domain` are correct
- Ensure token hasn't expired
- Verify audience claim matches expected value

```csharp
// Enable detailed token validation logging
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Auth failed: {context.Exception}");
                return Task.CompletedTask;
            }
        };
    }, 
    options => { builder.Configuration.Bind("AzureAdB2C", options); });
```

#### 2. CORS Errors in Browser

**Cause**: Cross-Origin Resource Sharing not configured

**Solution**:
```csharp
// In Gateway/API Program.cs
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("https://your-blazor-app.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

var app = builder.Build();
app.UseCors(); // Must be before UseAuthentication()
app.UseAuthentication();
app.UseAuthorization();
```

#### 3. "AADB2C-ApiConnector-Password is not set" Error

**Cause**: API connector password not configured

**Solution**:
- Add secret to Azure Key Vault
- Configure Key Vault reference in app settings
- Verify managed identity has access to Key Vault

```bash
az keyvault secret set --vault-name gateway-keyvault \
  --name "AADB2C-ApiConnector-Password" \
  --value "<secure-password>"
```

#### 4. Token Not Forwarded to Backend APIs

**Cause**: Authorization header not being forwarded by YARP

**Solution**:
```json
{
  "ReverseProxy": {
    "Routes": {
      "gamesRoute": {
        "ClusterId": "gamesapicluster",
        "AuthorizationPolicy": "playPolicy",
        "Match": {
          "Path": "/games/{*any}"
        }
        // YARP forwards Authorization header by default
        // Verify AuthorizationPolicy is set
      }
    }
  }
}
```

#### 5. MsalUiRequiredException in Desktop Apps

**Cause**: Token cache expired or empty

**Solution**:
```csharp
try
{
    // Try silent acquisition first
    var result = await _app.AcquireTokenSilent(_scopes, account).ExecuteAsync();
}
catch (MsalUiRequiredException)
{
    // Fall back to interactive
    var result = await _app.AcquireTokenInteractive(_scopes).ExecuteAsync();
}
```

#### 6. Redirect URI Mismatch

**Cause**: Redirect URI in app doesn't match registration

**Solution**:
- Verify redirect URI in Azure Portal app registration
- Platform-specific URIs must match exactly
- Check for typos in scheme or host

```
Registered in Portal: msauth.com.codebreakerapp.maui://auth
Code must use: msauth.com.codebreakerapp.maui://auth
❌ msauth.com.codebreakerapp://auth (missing .maui)
```

#### 7. "Audience validation failed" Error

**Cause**: Token audience doesn't match expected value

**Solution**:
```csharp
// Check expected audience in token validation
options.TokenValidationParameters = new TokenValidationParameters
{
    ValidAudiences = new[]
    {
        "<client-id>",
        $"api://<client-id>"
    }
};
```

#### 8. Blazor WASM Authentication State Not Persisting

**Cause**: Token not being stored or retrieved correctly

**Solution**:
```csharp
// Ensure proper scopes are requested
builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAdB2C", options.ProviderOptions.Authentication);
    options.ProviderOptions.DefaultAccessTokenScopes.Add("openid");
    options.ProviderOptions.DefaultAccessTokenScopes.Add("profile");
    options.ProviderOptions.DefaultAccessTokenScopes.Add("<api-scope>");
});
```

### Debugging Tips

1. **Enable MSAL Logging**:
```csharp
var app = PublicClientApplicationBuilder
    .Create(clientId)
    .WithLogging((level, message, containsPii) =>
    {
        Console.WriteLine($"MSAL {level}: {message}");
    }, LogLevel.Verbose, enablePiiLogging: false, enableDefaultPlatformLogging: true)
    .Build();
```

2. **Use JWT Decoder**: Decode tokens at [jwt.ms](https://jwt.ms) to inspect claims

3. **Check Network Traffic**: Use browser dev tools or Fiddler to inspect tokens

4. **Verify Token Claims**: Log claims received in your application
```csharp
foreach (var claim in context.User.Claims)
{
    Console.WriteLine($"{claim.Type}: {claim.Value}");
}
```

## Additional Resources

### Official Documentation

- [Microsoft Identity Platform](https://learn.microsoft.com/en-us/azure/active-directory/develop/)
- [Microsoft External ID Documentation](https://learn.microsoft.com/en-us/azure/active-directory-b2c/)
- [MSAL.NET Documentation](https://learn.microsoft.com/en-us/azure/active-directory/develop/msal-overview)
- [Microsoft.Identity.Web Documentation](https://learn.microsoft.com/en-us/azure/active-directory/develop/microsoft-identity-web)
- [YARP Documentation](https://microsoft.github.io/reverse-proxy/)

### Sample Applications

- [Microsoft Identity Samples](https://github.com/Azure-Samples?q=active-directory)
- [Blazor External ID Sample](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2)
- [MAUI External ID Sample](https://github.com/Azure-Samples/ms-identity-mobile-apple-swift-objc)

### Related Codebreaker Documentation

- [Identity Service README](../../src/services/identity/README.md)
- [Gateway Configuration](../../src/services/gateway/Codebreaker.ApiGateway/README.md)
- [Game APIs Documentation](../../src/services/gameapis/README.md)

## Contributing

If you find issues with this documentation or have suggestions for improvement, please:

1. Open an issue in the repository
2. Submit a pull request with corrections
3. Contact the maintainers

## License

This documentation is part of the Codebreaker project and follows the same license terms.
