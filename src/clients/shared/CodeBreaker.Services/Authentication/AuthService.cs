using CodeBreaker.Services.Authentication.Definitions;
using CodeBreaker.Services.EventArguments;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;

namespace CodeBreaker.Services.Authentication;

public class AuthService : IAuthService
{
    private const string TenantName = "codebreaker3000";
    private const string Tenant = $"{TenantName}.onmicrosoft.com";
    private const string AzureAdB2CHostname = $"{TenantName}.b2clogin.com";

    private const string ClientId = "cda69052-a9ff-4263-a630-2786e89e5075";

    private const string RedirectUri = $"https://{AzureAdB2CHostname}/oauth2/nativeclient";
    private const string PolicySignUpSignIn = "B2C_1_SUSI";
    //private const string PolicyEditProfile = "";
    //private const string PolicyResetPassword = "";

    private readonly static string AuthorityBase = $"https://{AzureAdB2CHostname}/tfp/{Tenant}/";
    public readonly static string AuthoritySignUpSignIn = $"{AuthorityBase}{PolicySignUpSignIn}";
    //public readonly static string AuthorityEditProfile = $"{AuthorityBase}{PolicyEditProfile}";
    //public readonly static string AuthorityResetPassword = $"{AuthorityBase}{PolicyResetPassword}";

    private readonly static string PersistentTokenCacheDirectory = Path.Combine(MsalCacheHelper.UserRootDirectory, ".codebreaker");

    private readonly static string PersistentTokenCacheFileName = "codebreaker_token_cache.txt";

    private readonly ILogger _logger;

    private readonly IPublicClientApplication _publicClientApplication;

    private UserInformation? _lastUserInformation;

    public event EventHandler<OnAuthenticationStateChangedEventArgs>? OnAuthenticationStateChanged;

    public AuthService(ILogger<AuthService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _publicClientApplication = PublicClientApplicationBuilder.Create(ClientId)
            .WithB2CAuthority(AuthoritySignUpSignIn)
            .WithRedirectUri(RedirectUri)
            //.WithLogging(Log, LogLevel.Info, false) // don't log P(ersonally) I(dentifiable) I(nformation) details on a regular basis
            .Build();
    }

    public async Task RegisterPersistentTokenCacheAsync()
    {
        var storageProperties = new StorageCreationPropertiesBuilder(PersistentTokenCacheFileName, PersistentTokenCacheDirectory).Build();
        var cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties);
        cacheHelper.RegisterCache(_publicClientApplication.UserTokenCache);
    }

    public UserInformation? LastUserInformation
    {
        get => _lastUserInformation;
        private set
        {
            _lastUserInformation = value;
            OnAuthenticationStateChanged?.Invoke(this, new());
        }
    }

    public bool IsAuthenticated => LastUserInformation is not null;

    public async Task<bool> TryAquireTokenSilentlyAsync(IAuthDefinition authDefinition, CancellationToken cancellationToken = default)
    {
        IAccount? account = (await GetAccountsAsync(cancellationToken)).FirstOrDefault();

        try
        {
            AuthenticationResult result = await _publicClientApplication.AcquireTokenSilent(authDefinition.Claims, account).ExecuteAsync(cancellationToken);
            LastUserInformation = UserInformation.FromAuthenticationResult(result);
            return true;
        }
        catch (MsalUiRequiredException)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to aquire token silently.");
            throw;
        }
    }

    public async Task<AuthenticationResult> AquireTokenAsync(IAuthDefinition authDefinition, CancellationToken cancellationToken = default)
    {
        IAccount? account = (await GetAccountsAsync(cancellationToken)).FirstOrDefault();

        try
        {
            AuthenticationResult result = await _publicClientApplication.AcquireTokenSilent(authDefinition.Claims, account).ExecuteAsync(cancellationToken);
            LastUserInformation = UserInformation.FromAuthenticationResult(result);
            return result;
        }
        catch (MsalUiRequiredException)
        {
            _logger.LogInformation("Unable to aquire token silently. Continueing interactively...");

            try
            {
                AuthenticationResult result = await _publicClientApplication
                    .AcquireTokenInteractive(authDefinition.Claims)
                    .ExecuteAsync(cancellationToken);
                LastUserInformation = UserInformation.FromAuthenticationResult(result);
                return result;
            }
            catch (MsalException msalEx)
            {
                _logger.LogError(msalEx, "Unable to aquire token interactively.");
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to aquire token silently.");
            throw;
        }
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        var accounts = await GetAccountsAsync(cancellationToken);

        foreach (var account in accounts)
            await _publicClientApplication.RemoveAsync(account);

        LastUserInformation = null;
    }

    private Task<IEnumerable<IAccount>> GetAccountsAsync(CancellationToken cancellation = default)
    {
        cancellation.ThrowIfCancellationRequested();
        return _publicClientApplication.GetAccountsAsync(PolicySignUpSignIn);
    }
}
