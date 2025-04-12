using DemianzxBackend.Application.BlogPosts.Commands.CreateBlogPost;
using DemianzxBackend.Application.BlogPosts.Commands.DeleteBlogPost;
using DemianzxBackend.Application.BlogPosts.Commands.IncrementPostViewCount;
using DemianzxBackend.Application.BlogPosts.Commands.UpdateBlogPost;
using DemianzxBackend.Application.BlogPosts.Queries.GetBlogPostBySlug;
using DemianzxBackend.Application.BlogPosts.Queries.GetBlogPosts;
using DemianzxBackend.Application.BlogPosts.Queries.GetFeaturedBlogPosts;
using DemianzxBackend.Application.BlogPosts.Queries.GetBlogPostsSimplified;
using DemianzxBackend.Application.Common.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DemianzxBackend.Web.Endpoints;

public class BlogPosts : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);

        // Public endpoints (no authorization required)

        app.MapGroup(this)
            .MapGet(GetBlogPosts)
            .MapGet(GetPublicBlogPostBySlug, "public/{slug}")
            .MapGet(GetFeaturedBlogPosts, "featured")
            .MapGet(IncrementPostViewCount, "\"{id}/view\"")
            .MapGet(GetBlogPostsSimplified, "simplified");

        // Protected endpoints (require authorization)
        app.MapGroup(this)
            .RequireAuthorization()            
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
    
    public async Task<Results<Ok<BlogPostDto>, NotFound>> GetPublicBlogPostBySlug(ISender sender, string slug)
    {
        var query = new GetBlogPostBySlugQuery { Slug = slug };
        var result = await sender.Send(query);

        if (result == null || !result.IsPublished)
            return TypedResults.NotFound();

        return TypedResults.Ok(result);
    }

    public async Task<Ok<List<BlogPostDto>>> GetFeaturedBlogPosts(ISender sender, [AsParameters] GetFeaturedBlogPostsQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    public async Task<NoContent> IncrementPostViewCount(ISender sender, int id)
    {
        await sender.Send(new IncrementPostViewCountCommand(id));
        return TypedResults.NoContent();
    }

    public async Task<Ok<PaginatedList<BlogPostSimplifiedDto>>> GetBlogPostsSimplified(ISender sender, [AsParameters] GetBlogPostsSimplifiedQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }
}
