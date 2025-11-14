using FluentValidation;

namespace eMeetup.Modules.Users.Application.Users.RegisterUser;

internal sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(c => c.Username).NotEmpty();        
        RuleFor(c => c.Email).EmailAddress();
        RuleFor(c => c.Password).MinimumLength(6);
    }
}
