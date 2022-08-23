using Microsoft.Identity.Client;

namespace CodeBreaker.Services;

public interface IAuthService
{
    Task<AuthenticationResult> LoginAsync(CancellationToken cancellation = default);

    Task<AuthenticationResult> RegisterAsync(CancellationToken cancellation = default);

    Task<bool> IsAuthenticatedAsync(CancellationToken cancellation = default);
}
