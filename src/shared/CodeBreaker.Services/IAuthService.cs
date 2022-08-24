using Microsoft.Identity.Client;

namespace CodeBreaker.Services;

public interface IAuthService
{
    UserInformation? LastUserInformation { get; }

    Task<AuthenticationResult> LoginAsync(CancellationToken cancellation = default);
}
