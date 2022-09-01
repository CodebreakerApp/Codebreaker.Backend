using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

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

    public UserInformation? LastUserInformation { get; private set; }

    public bool IsAuthenticated => LastUserInformation != null;

    public AuthService(ILogger<AuthService> logger)
    {
        _logger = logger;
        PublicClientApplication = PublicClientApplicationBuilder.Create(ClientId)
            .WithB2CAuthority(AuthoritySignUpSignIn)
            .WithRedirectUri(RedirectUri)
            //.WithLogging(Log, LogLevel.Info, false) // don't log P(ersonally) I(dentifiable) I(nformation) details on a regular basis
            .Build();
    }

    public async Task<AuthenticationResult> AquireTokenAsync(CancellationToken cancellation = default)
    {
        IAccount? account = (await GetAccountsAsync(cancellation)).FirstOrDefault();

        try
        {
            AuthenticationResult result = await PublicClientApplication.AcquireTokenSilent(ApiScopes, account).ExecuteAsync(cancellation);
            LastUserInformation = UserInformation.FromAuthenticationResult(result);
            return result;
        }
        catch (MsalUiRequiredException)
        {
            _logger.LogInformation("Unable to aquire token silently. Continueing interactively...");

            try
            {
                AuthenticationResult result = await PublicClientApplication
                    .AcquireTokenInteractive(ApiScopes)
                    .ExecuteAsync(cancellation);
                LastUserInformation = UserInformation.FromAuthenticationResult(result);
                return result;
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

    private Task<IEnumerable<IAccount>> GetAccountsAsync(CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        return PublicClientApplication.GetAccountsAsync(PolicySignUpSignIn);
    }
}
