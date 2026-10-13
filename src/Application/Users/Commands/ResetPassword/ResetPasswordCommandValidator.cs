namespace DemianzxBackend.Application.Users.Commands.ResetPassword;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(100)
            .WithMessage("Ingresa un correo electrónico válido de hasta 100 caracteres.");
        RuleFor(x => x.Token).NotEmpty().MaximumLength(4096)
            .WithMessage("El enlace de recuperación no es válido. Solicita uno nuevo.");
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8)
            .WithMessage("La contraseña debe tener al menos 8 caracteres.");
    }
}
