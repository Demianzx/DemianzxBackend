using DemianzxBackend.Application.BlogPosts.Queries.GetBlogPosts;
using DemianzxBackend.Application.Common.Interfaces;

namespace DemianzxBackend.Application.BlogPosts.Queries.GetFeaturedBlogPosts;

public record GetFeaturedBlogPostsQuery : IRequest<List<BlogPostDto>>
{
    public int Count { get; init; } = 5;
}

public class GetFeaturedBlogPostsQueryHandler : IRequestHandler<GetFeaturedBlogPostsQuery, List<BlogPostDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IIdentityService _identityService;

    public GetFeaturedBlogPostsQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        IIdentityService identityService)
    {
        _context = context;
        _mapper = mapper;
        _identityService = identityService;
    }

    public async Task<List<BlogPostDto>> Handle(GetFeaturedBlogPostsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.BlogPosts
            .AsNoTracking()
            .Where(p => p.IsPublished && p.IsFeatured)
            .OrderByDescending(p => p.PublishedDate)
            .Take(request.Count);

        var posts = await query
            .ProjectTo<BlogPostDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        // Get Author name
        foreach (var post in posts)
        {
            post.AuthorName = await _identityService.GetUserNameAsync(post.AuthorId) ?? string.Empty;
        }

        return posts;
    }
}
