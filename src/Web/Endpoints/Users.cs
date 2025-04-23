using DemianzxBackend.Application.Users.Commands.RegisterUser;
using DemianzxBackend.Application.Users.Commands.LoginUser;
using DemianzxBackend.Infrastructure.Identity;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DemianzxBackend.Web.Endpoints;

public class Users : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapPost(RegisterUser, "register")
            .MapPost(LoginUser, "login");

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
}
