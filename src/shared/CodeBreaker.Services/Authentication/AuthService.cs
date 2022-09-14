using CodeBreaker.Services.Authentication.Definitions;
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
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        PublicClientApplication = PublicClientApplicationBuilder.Create(ClientId)
            .WithB2CAuthority(AuthoritySignUpSignIn)
            .WithRedirectUri(RedirectUri)
            //.WithLogging(Log, LogLevel.Info, false) // don't log P(ersonally) I(dentifiable) I(nformation) details on a regular basis
            .Build();
    }

    public async Task<AuthenticationResult> AquireTokenAsync(IAuthDefinition authHandler, CancellationToken cancellation = default)
    {
        IAccount? account = (await GetAccountsAsync(cancellation)).FirstOrDefault();

        try
        {
            AuthenticationResult result = await PublicClientApplication.AcquireTokenSilent(authHandler.Claims, account).ExecuteAsync(cancellation);
            LastUserInformation = UserInformation.FromAuthenticationResult(result);
            return result;
        }
        catch (MsalUiRequiredException)
        {
            _logger.LogInformation("Unable to aquire token silently. Continueing interactively...");

            try
            {
                AuthenticationResult result = await PublicClientApplication
                    .AcquireTokenInteractive(authHandler.Claims)
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

    private Task<IEnumerable<IAccount>> GetAccountsAsync(CancellationToken cancellation = default)
    {
        cancellation.ThrowIfCancellationRequested();
        return PublicClientApplication.GetAccountsAsync(PolicySignUpSignIn);
    }
}
