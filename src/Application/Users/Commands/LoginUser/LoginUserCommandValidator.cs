using DemianzxBackend.Application.Users.Commands.LoginUser;

namespace DemianzxBackend.Application.Users.Commands.LoginUser;

public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(v => v.UserName)
            .NotEmpty().WithMessage("Username is required.");

        RuleFor(v => v.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
