using CodeBreaker.Shared.Models.Users;
using FluentValidation.Results;

namespace CodeBreaker.UserService.Services;
internal interface IUserValidationService
{
    Task<ValidationResult> ValidateUserAsync(User user, CancellationToken cancellationToken = default);
}
