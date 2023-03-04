using CodeBreaker.Shared.Models.Users;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Localization;

namespace CodeBreaker.UserService.Services;

internal class UserValidationService : IUserValidationService
{
    private readonly IGamerNameService _gamerNameService;

    private readonly IValidator<User> _userValidator;

    private readonly IStringLocalizer _localizer;

    public UserValidationService(IGamerNameService gamerNameService, IValidator<User> userValidator, IStringLocalizer<UserValidationService> localizer)
    {
        _gamerNameService = gamerNameService;
        _userValidator = userValidator;
        _localizer = localizer;
    }

    public async Task<ValidationResult> ValidateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        var result = _userValidator.Validate(user);

        if (result.IsValid && !await _gamerNameService.IsGamerNameTakenAsync(user.GamerName))
            result.Errors.Add(new(nameof(user.GamerName), _localizer["GamerNameTakenMessage"], user.GamerName));

        return result;
    }
}
