using DemianzxBackend.Application.Common.Interfaces;
using DemianzxBackend.Application.Common.Models;

namespace DemianzxBackend.Application.Users.Commands.ChangePassword;

public record ChangePasswordCommand : IRequest<Result>
{
    public string CurrentPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IUser _currentUser;

    public ChangePasswordCommandHandler(
        IIdentityService identityService,
        IUser currentUser)
    {
        _identityService = identityService;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.Id == null)
        {
            throw new UnauthorizedAccessException();
        }

        return await _identityService.ChangePasswordAsync(
            _currentUser.Id,
            request.CurrentPassword,
            request.NewPassword);
    }
}
