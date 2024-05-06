using CodeBreaker.Shared.Models.Users;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Localization;

namespace CodeBreaker.UserService.Services;

internal class UserValidationService(IGamerNameService gamerNameService, IValidator<User> userValidator, IStringLocalizer<UserValidationService> localizer) : IUserValidationService
{
    public async Task<ValidationResult> ValidateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        var result = userValidator.Validate(user);

        if (result.IsValid && !await gamerNameService.IsGamerNameTakenAsync(user.GamerName))
            result.Errors.Add(new(nameof(user.GamerName), localizer["GamerNameTakenMessage"], user.GamerName));

        return result;
    }
}
