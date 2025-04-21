using DemianzxBackend.Application.Common.Interfaces;
using DemianzxBackend.Application.Common.Models;

namespace DemianzxBackend.Application.Users.Commands.RegisterUser;

public record RegisterUserCommand : IRequest<Result>
{
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result>
{
    private readonly IIdentityService _identityService;

    public RegisterUserCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var (result, _) = await _identityService.CreateUserWithDetailsAsync(
            request.UserName,
            request.Email,
            request.Password);

        return result;
    }
}
