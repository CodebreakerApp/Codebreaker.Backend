using CodeBreaker.Shared.Models.Users;
using FluentValidation;

namespace CodeBreaker.Shared.Validators.Users;

public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(_ => _.Email)
            .EmailAddress()
            .MaximumLength(100);
        RuleFor(_ => _.GivenName)
            .MinimumLength(2)
            .MaximumLength(50);
        RuleFor(_ => _.Surname)
            .MinimumLength(2)
            .MaximumLength(50);
        RuleFor(_ => _.GamerName)
            .SetValidator(new GamerNameValidator());
    }
}
