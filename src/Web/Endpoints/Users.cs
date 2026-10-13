using DemianzxBackend.Application.Users.Commands.RegisterUser;
using DemianzxBackend.Application.Users.Commands.LoginUser;
using DemianzxBackend.Infrastructure.Identity;
using Microsoft.AspNetCore.Http.HttpResults;
using DemianzxBackend.Application.Users.Commands.ChangePassword;
using DemianzxBackend.Application.Users.Commands.ForgotPassword;
using DemianzxBackend.Application.Users.Commands.ResetPassword;

namespace DemianzxBackend.Web.Endpoints;

public class Users : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapPost(RegisterUser, "register")
            .MapPost(LoginUser, "login");

        var recoveryGroup = app.MapGroup(this)
            .AllowAnonymous()
            .RequireRateLimiting("PasswordRecovery");
        recoveryGroup.MapPost(ForgotPassword, "forgot-password");
        recoveryGroup.MapPost(ResetPassword, "reset-password");

        app.MapGroup(this)
            .RequireAuthorization()
            .MapPost(ChangePassword, "change-password");
    }

    public async Task<Ok> ForgotPassword(ISender sender, ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return TypedResults.Ok();
    }

    public async Task<Results<Ok, BadRequest<string[]>>> ResetPassword(ISender sender, ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.Succeeded ? TypedResults.Ok() : TypedResults.BadRequest(result.Errors);
    }

    public async Task<Results<Ok<LoginResponse>, UnauthorizedHttpResult>> LoginUser(ISender sender, LoginUserCommand command)
    {
        var result = await sender.Send(command);

        if (result.Success)
            return TypedResults.Ok(result);

        return TypedResults.Unauthorized();
    }

    public async Task<IResult> RegisterUser(ISender sender, RegisterUserCommand command)
    {
        var result = await sender.Send(command);

        if (result.Succeeded)
            return Results.Ok();

        return Results.BadRequest(result.Errors);
    }

    public async Task<Results<Ok, UnauthorizedHttpResult, BadRequest<string[]>>> ChangePassword(ISender sender, ChangePasswordCommand command)
    {
        try
        {
            var result = await sender.Send(command);

            if (result.Succeeded)
                return TypedResults.Ok();

            return TypedResults.BadRequest(result.Errors);
        }
        catch (UnauthorizedAccessException)
        {
            return TypedResults.Unauthorized();
        }
    }
}
