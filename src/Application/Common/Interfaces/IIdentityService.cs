using DemianzxBackend.Application.Common.Models;
using DemianzxBackend.Application.Users.Commands.LoginUser;

namespace DemianzxBackend.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password);

    Task<Result> DeleteUserAsync(string userId);

    Task<(Result Result, string UserId)> CreateUserWithDetailsAsync(string userName, string email, string password);

    Task<LoginResponse> LoginAsync(string userName, string password);
}
