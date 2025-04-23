using DemianzxBackend.Application.Common.Interfaces;
using DemianzxBackend.Application.Common.Models;

namespace DemianzxBackend.Application.Users.Commands.LoginUser;

public record LoginUserCommand : IRequest<LoginResponse>
{
    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public record LoginResponse
{
    public string Token { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public List<string> Roles { get; init; } = new();
    public bool Success { get; init; }
    public string[] Errors { get; init; } = Array.Empty<string>();
}

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginResponse>
{
    private readonly IIdentityService _identityService;

    public LoginUserCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<LoginResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.LoginAsync(request.UserName, request.Password);
    }
}
