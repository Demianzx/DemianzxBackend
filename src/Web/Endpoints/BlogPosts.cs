using DemianzxBackend.Application.BlogPosts.Commands.CreateBlogPost;
using DemianzxBackend.Application.BlogPosts.Commands.DeleteBlogPost;
using DemianzxBackend.Application.BlogPosts.Commands.UpdateBlogPost;
using DemianzxBackend.Application.BlogPosts.Queries.GetBlogPostBySlug;
using DemianzxBackend.Application.BlogPosts.Queries.GetBlogPosts;
using DemianzxBackend.Application.Common.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DemianzxBackend.Web.Endpoints;

public class BlogPosts : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetBlogPosts)
            .MapGet(GetBlogPostBySlug, "{slug}")
            .MapPost(CreateBlogPost)
            .MapPut(UpdateBlogPost, "{id}")
            .MapDelete(DeleteBlogPost, "{id}");
    }

    public async Task<Ok<PaginatedList<BlogPostDto>>> GetBlogPosts(ISender sender, [AsParameters] GetBlogPostsQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    public async Task<Results<Ok<BlogPostDto>, NotFound>> GetBlogPostBySlug(ISender sender, string slug)
    {
        var query = new GetBlogPostBySlugQuery { Slug = slug };
        var result = await sender.Send(query);

        if (result == null)
            return TypedResults.NotFound();

        return TypedResults.Ok(result);
    }

    public async Task<Created<int>> CreateBlogPost(ISender sender, CreateBlogPostCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/BlogPosts/{id}", id);
    }

    public async Task<Results<NoContent, BadRequest, NotFound>> UpdateBlogPost(ISender sender, int id, UpdateBlogPostCommand command)
    {
        if (id != command.Id)
            return TypedResults.BadRequest();

        try
        {
            await sender.Send(command);
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.NoContent();
    }

    public async Task<Results<NoContent, NotFound>> DeleteBlogPost(ISender sender, int id)
    {
        try
        {
            await sender.Send(new DeleteBlogPostCommand(id));
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.NoContent();
    }
}
