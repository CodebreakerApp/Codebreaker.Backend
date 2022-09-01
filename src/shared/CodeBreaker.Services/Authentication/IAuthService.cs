using Microsoft.Identity.Client;

namespace CodeBreaker.Services.Authentication;

public interface IAuthService
{
    UserInformation? LastUserInformation { get; }

    bool IsAuthenticated { get; }

    Task<AuthenticationResult> AquireTokenAsync(CancellationToken cancellation = default);
}
