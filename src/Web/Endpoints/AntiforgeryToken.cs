using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DemianzxBackend.Web.Endpoints;

public class AntiforgeryToken : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapGet(GetAntiforgeryToken);
    }

    public IResult GetAntiforgeryToken(IAntiforgery antiforgery, HttpContext httpContext)
    {
        var tokens = antiforgery.GetAndStoreTokens(httpContext);
        return TypedResults.Ok(new { token = tokens.RequestToken });
    }
}
