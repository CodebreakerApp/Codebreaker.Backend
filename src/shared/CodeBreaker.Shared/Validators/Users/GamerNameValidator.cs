using FluentValidation;

namespace CodeBreaker.Shared.Validators.Users;

public class GamerNameValidator : AbstractValidator<string>
{
    public GamerNameValidator()
    {
        RuleFor(_ => _)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(50)
            .WithName("GamerName");
    }
}
