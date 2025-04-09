using DemianzxBackend.Application.Tags.Commands.CreateTag;
using DemianzxBackend.Application.Tags.Commands.DeleteTag;
using DemianzxBackend.Application.Tags.Queries;
using DemianzxBackend.Application.Tags.Queries.GetTags;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DemianzxBackend.Web.Endpoints;

public class Tags : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetTags)
            .MapPost(CreateTag)
            .MapDelete(DeleteTag, "{id}");
    }

    public async Task<Ok<List<TagDto>>> GetTags(ISender sender)
    {
        var result = await sender.Send(new GetTagsQuery());
        return TypedResults.Ok(result);
    }

    public async Task<Created<int>> CreateTag(ISender sender, CreateTagCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/Tags/{id}", id);
    }

    public async Task<Results<NoContent, NotFound>> DeleteTag(ISender sender, int id)
    {
        try
        {
            await sender.Send(new DeleteTagCommand(id));
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.NoContent();
    }
}
