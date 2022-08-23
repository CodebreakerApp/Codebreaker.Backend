using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace CodeBreaker.Services;

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

    private static string[] ApiScopes = {
        $"https://{Tenant}/39a665a4-54ce-44cd-b581-0f654a31dbcf/Games.Play",
        $"https://{Tenant}/39a665a4-54ce-44cd-b581-0f654a31dbcf/Reports.Read",
    };

    private static string[] LiveScopes =
    {
        $"https://{Tenant}/77d424b0-f92c-4f00-bd88-c6645f0d11e7/Messages.ReadFromAllGroups",
        $"https://{Tenant}/77d424b0-f92c-4f00-bd88-c6645f0d11e7/Messages.ReadFromSameGroup",
    };

    private static string AuthorityBase = $"https://{AzureAdB2CHostname}/tfp/{Tenant}/";
    public static string AuthoritySignUpSignIn = $"{AuthorityBase}{PolicySignUpSignIn}";
    //public static string AuthorityEditProfile = $"{AuthorityBase}{PolicyEditProfile}";
    //public static string AuthorityResetPassword = $"{AuthorityBase}{PolicyResetPassword}";

    private readonly ILogger _logger;

    private IPublicClientApplication PublicClientApplication { get; }

    public AuthService(ILogger<AuthService> logger)
    {
        _logger = logger;
        PublicClientApplication = PublicClientApplicationBuilder.Create(ClientId)
            .WithB2CAuthority(AuthoritySignUpSignIn)
            .WithRedirectUri(RedirectUri)
            //.WithLogging(Log, LogLevel.Info, false) // don't log P(ersonally) I(dentifiable) I(nformation) details on a regular basis
            .Build();
    }

    public async Task<AuthenticationResult> LoginAsync(CancellationToken cancellation = default)
    {
        IAccount? account = (await GetAccountsAsync()).FirstOrDefault();

        try
        {
            return await PublicClientApplication.AcquireTokenSilent(ApiScopes, account).ExecuteAsync(cancellation);
        }
        catch (MsalUiRequiredException)
        {
            _logger.LogInformation("Unable to aquire token silently. Continueing interactively...");

            try
            {
                return await PublicClientApplication
                    .AcquireTokenInteractive(ApiScopes)
                    //.WithUseEmbeddedWebView(false)
                    //.WithSystemWebViewOptions(new()
                    //{
                    //    OpenBrowserAsync = OpenInteractiveAsync
                    //})
                    .ExecuteAsync(cancellation);
            }
            catch (MsalException msalEx)
            {
                _logger.LogError("Unable to aquire token interactively.", msalEx);
                throw;
            }
        }
        catch (Exception)
        {
            _logger.LogError("Unable to aquire token silently.");
            throw;
        }
    }

    public async Task<bool> IsAuthenticatedAsync(CancellationToken cancellation = default)
    {
        cancellation.ThrowIfCancellationRequested();
        return (await PublicClientApplication.GetAccountsAsync()).Any();
    }

    private Task OpenInteractiveAsync(Uri uri)
    {
        return Task.CompletedTask;
    }

    private Task<IEnumerable<IAccount>> GetAccountsAsync() =>
        PublicClientApplication.GetAccountsAsync(PolicySignUpSignIn);
}
