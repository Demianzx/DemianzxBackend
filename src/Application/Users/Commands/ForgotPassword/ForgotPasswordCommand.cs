using DemianzxBackend.Application.Common.Interfaces;

namespace DemianzxBackend.Application.Users.Commands.ForgotPassword;

public record ForgotPasswordCommand : IRequest
{
    public string Email { get; init; } = string.Empty;

    public override string ToString() => nameof(ForgotPasswordCommand);
}

public class ForgotPasswordCommandHandler(IPasswordRecoveryService passwordRecovery)
    : IRequestHandler<ForgotPasswordCommand>
{
    public Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        => passwordRecovery.RequestResetAsync(request.Email.Trim(), cancellationToken);
}
