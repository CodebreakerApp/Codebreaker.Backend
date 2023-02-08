using CodeBreaker.Shared.Models.Users;
using FluentValidation;
using FluentValidation.Results;

namespace CodeBreaker.UserService.Services;

internal class UserValidationService : IUserValidationService
{
    private readonly IGamerNameService _gamerNameService;

    private readonly IValidator<User> _userValidator;

    public UserValidationService(IGamerNameService gamerNameService, IValidator<User> userValidator)
    {
        _gamerNameService = gamerNameService;
        _userValidator = userValidator;
    }

    public async Task<ValidationResult> ValidateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        var result = _userValidator.Validate(user);

        if (result.IsValid && !await _gamerNameService.IsGamerNameTakenAsync(user.GamerName))
            result.Errors.Add(new(nameof(user.GamerName), "The gamername is already taken.", user.GamerName)); // TODO Localize

        return result;
    }
}
