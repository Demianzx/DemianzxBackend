using DemianzxBackend.Application.Users.Commands.RegisterUser;
using DemianzxBackend.Infrastructure.Identity;

namespace DemianzxBackend.Web.Endpoints;

public class Users : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapIdentityApi<ApplicationUser>();

        app.MapGroup(this)
            .MapPost(RegisterUser, "public/register");
    }

    public async Task<IResult> RegisterUser(ISender sender, RegisterUserCommand command)
    {
        var result = await sender.Send(command);

        if (result.Succeeded)
            return Results.Ok();

        return Results.BadRequest(result.Errors);
    }
}
