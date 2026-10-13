using DemianzxBackend.Application.Common.Interfaces;
using DemianzxBackend.Application.Common.Models;

namespace DemianzxBackend.Application.Users.Commands.ResetPassword;

public record ResetPasswordCommand : IRequest<Result>
{
    public string Email { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;

    public override string ToString() => nameof(ResetPasswordCommand);
}

public class ResetPasswordCommandHandler(IPasswordRecoveryService passwordRecovery)
    : IRequestHandler<ResetPasswordCommand, Result>
{
    public Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        => passwordRecovery.ResetPasswordAsync(request.Email.Trim(), request.Token, request.NewPassword);
}
