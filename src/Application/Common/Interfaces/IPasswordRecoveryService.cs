using DemianzxBackend.Application.Common.Models;

namespace DemianzxBackend.Application.Common.Interfaces;

public interface IPasswordRecoveryService
{
    Task RequestResetAsync(string email, CancellationToken cancellationToken);
    Task<Result> ResetPasswordAsync(string email, string token, string newPassword);
}
