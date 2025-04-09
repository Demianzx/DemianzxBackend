using DemianzxBackend.Application.Tags.Queries;
using DemianzxBackend.Application.Tags.Queries.GetTags;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DemianzxBackend.Web.Endpoints;

public class PublicTags : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapGet(GetPublicTags);
    }

    public async Task<Ok<List<TagDto>>> GetPublicTags(ISender sender)
    {
        var result = await sender.Send(new GetTagsQuery());
        return TypedResults.Ok(result);
    }
}
