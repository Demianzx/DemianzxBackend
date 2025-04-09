using DemianzxBackend.Application.Categories.Queries;
using DemianzxBackend.Application.Categories.Queries.GetCategories;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DemianzxBackend.Web.Endpoints;

public class PublicCategories : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapGet(GetPublicCategories);
    }

    public async Task<Ok<List<CategoryDto>>> GetPublicCategories(ISender sender)
    {
        var result = await sender.Send(new GetCategoriesQuery());
        return TypedResults.Ok(result);
    }
}
