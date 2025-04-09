using DemianzxBackend.Application.BlogPosts.Queries.GetBlogPosts;
using DemianzxBackend.Application.Common.Interfaces;
using DemianzxBackend.Application.Common.Mappings;
using DemianzxBackend.Application.Common.Models;

namespace DemianzxBackend.Application.BlogPosts.Queries.GetPublicBlogPosts;

public record GetPublicBlogPostsQuery : IRequest<PaginatedList<BlogPostDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? CategorySlug { get; init; } = null;
    public string? TagSlug { get; init; } = null;
}

public class GetPublicBlogPostsQueryHandler : IRequestHandler<GetPublicBlogPostsQuery, PaginatedList<BlogPostDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IIdentityService _identityService;

    public GetPublicBlogPostsQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        IIdentityService identityService)
    {
        _context = context;
        _mapper = mapper;
        _identityService = identityService;
    }

    public async Task<PaginatedList<BlogPostDto>> Handle(GetPublicBlogPostsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.BlogPosts
            .AsNoTracking()
            .Where(p => p.IsPublished); // Only published posts

        // Filter by category if provided
        if (!string.IsNullOrEmpty(request.CategorySlug))
        {
            query = query.Where(p => _context.PostCategories
                .Any(pc => pc.PostId == p.Id &&
                      _context.Categories.Any(c => c.Id == pc.CategoryId && c.Slug == request.CategorySlug)));
        }

        // Filter by tag if provided
        if (!string.IsNullOrEmpty(request.TagSlug))
        {
            query = query.Where(p => _context.PostTags
                .Any(pt => pt.PostId == p.Id &&
                      _context.Tags.Any(t => t.Id == pt.TagId && t.Slug == request.TagSlug)));
        }

        var posts = await query
            .OrderByDescending(p => p.PublishedDate)
            .ProjectTo<BlogPostDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);

        // Get Author name
        foreach (var post in posts.Items)
        {
            post.AuthorName = await _identityService.GetUserNameAsync(post.AuthorId) ?? string.Empty;
        }

        return posts;
    }
}
