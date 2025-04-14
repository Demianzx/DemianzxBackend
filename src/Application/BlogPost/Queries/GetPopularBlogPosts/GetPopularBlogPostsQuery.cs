using DemianzxBackend.Application.BlogPosts.Queries.GetBlogPosts;
using DemianzxBackend.Application.Common.Interfaces;

namespace DemianzxBackend.Application.BlogPosts.Queries.GetPopularBlogPosts;

public record GetPopularBlogPostsQuery : IRequest<List<BlogPostDto>>
{
    public int Count { get; init; } = 5;
}

public class GetPopularBlogPostsQueryHandler : IRequestHandler<GetPopularBlogPostsQuery, List<BlogPostDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IIdentityService _identityService;

    public GetPopularBlogPostsQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        IIdentityService identityService)
    {
        _context = context;
        _mapper = mapper;
        _identityService = identityService;
    }

    public async Task<List<BlogPostDto>> Handle(GetPopularBlogPostsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.BlogPosts
            .AsNoTracking()
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.ViewCount)
            .Take(request.Count);

        var posts = await query
            .ProjectTo<BlogPostDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        // Obtener los nombres de los autores
        foreach (var post in posts)
        {
            post.AuthorName = await _identityService.GetUserNameAsync(post.AuthorId) ?? string.Empty;
        }

        return posts;
    }
}
