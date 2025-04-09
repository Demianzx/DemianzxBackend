using DemianzxBackend.Application.BlogPosts.Queries.GetBlogPostBySlug;
using DemianzxBackend.Application.BlogPosts.Queries.GetBlogPosts;
using DemianzxBackend.Application.BlogPosts.Queries.GetPublicBlogPosts;
using DemianzxBackend.Application.BlogPosts.Queries.GetFeaturedBlogPosts;
using DemianzxBackend.Application.Common.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using DemianzxBackend.Application.BlogPosts.Commands.IncrementPostViewCount;

namespace DemianzxBackend.Web.Endpoints;

public class PublicBlogPosts : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapGet(GetPublicBlogPosts)
            .MapGet(GetPublicBlogPostBySlug, "{slug}")
            .MapGet(GetFeaturedBlogPosts, "featured")
            .MapPost(IncrementPostViewCount, "{id}/view");
    }

    public async Task<Ok<PaginatedList<BlogPostDto>>> GetPublicBlogPosts(ISender sender, [AsParameters] GetPublicBlogPostsQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
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
}
