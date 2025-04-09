using DemianzxBackend.Application.Comments.Commands.CreateComment;
using DemianzxBackend.Application.Comments.Commands.DeleteComment;
using DemianzxBackend.Application.Comments.Queries;
using DemianzxBackend.Application.Comments.Queries.GetPostComments;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DemianzxBackend.Web.Endpoints;

public class Comments : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetPostComments, "post/{postId}")
            .MapPost(CreateComment)
            .MapDelete(DeleteComment, "{id}");
    }

    public async Task<Ok<List<CommentDto>>> GetPostComments(ISender sender, int postId)
    {
        var result = await sender.Send(new GetPostCommentsQuery(postId));
        return TypedResults.Ok(result);
    }

    public async Task<Created<int>> CreateComment(ISender sender, CreateCommentCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/Comments/{id}", id);
    }

    public async Task<Results<NoContent, NotFound>> DeleteComment(ISender sender, int id)
    {
        try
        {
            await sender.Send(new DeleteCommentCommand(id));
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.NoContent();
    }
}
