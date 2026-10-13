namespace DemianzxBackend.Application.Users.Commands.ForgotPassword;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(100)
            .WithMessage("Ingresa un correo electrónico válido de hasta 100 caracteres.");
    }
}
