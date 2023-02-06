using CodeBreaker.UserService.Models.Api;

namespace CodeBreaker.UserService.Services;

internal class AzureActiveDirectoryB2CApiConnectorService : IAzureActiveDirectoryB2CApiConnectorService
{
    private readonly IGamerNameService _gamerNameService;

    public AzureActiveDirectoryB2CApiConnectorService(IGamerNameService gamerNameService)
    {
        _gamerNameService = gamerNameService;
    }

    public async Task<BeforeCreatingUserResponse> ValidateBeforeUserCreationAsync(BeforeCreatingUserRequest request, CancellationToken cancellationToken = default)
    {
        if (request.GivenName.Length < 2 || request.GivenName.Length > 50)
            return new BeforeCreatingUserValidationErrorResponse("given name length");

        if (request.Surname.Length < 2 || request.Surname.Length > 50)
            return new BeforeCreatingUserValidationErrorResponse("surname length");

        if (request.Extension_dd21590c971e431494da34e2a8d47cce_GamerName.Length < 3 || request.Extension_dd21590c971e431494da34e2a8d47cce_GamerName.Length > 50)
            return new BeforeCreatingUserValidationErrorResponse("gamername length");

        if (!await _gamerNameService.IsGamerNameTakenAsync(request.Extension_dd21590c971e431494da34e2a8d47cce_GamerName))
            return new BeforeCreatingUserValidationErrorResponse("gamername taken");

        return new BeforeCreatingUserSuccessResponse();
    }
}
