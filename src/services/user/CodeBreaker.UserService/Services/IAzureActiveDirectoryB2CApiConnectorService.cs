using CodeBreaker.UserService.Models.Api;

namespace CodeBreaker.UserService.Services;
internal interface IAzureActiveDirectoryB2CApiConnectorService
{
    Task<BeforeCreatingUserResponse> ValidateBeforeUserCreationAsync(BeforeCreatingUserRequest request, CancellationToken cancellationToken = default);
}