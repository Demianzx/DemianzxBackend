using DemianzxBackend.Application.BlogPosts.Queries.GetBlogPostsSimplified;
using DemianzxBackend.Application.Common.Interfaces;

namespace DemianzxBackend.Application.BlogPosts.Queries.GetRelatedBlogPosts;

public record GetRelatedBlogPostsQuery : IRequest<List<BlogPostSimplifiedDto>>
{
    public int PostId { get; init; }
    public int Count { get; init; } = 5; // Número de artículos relacionados a devolver
}

public class GetRelatedBlogPostsQueryHandler : IRequestHandler<GetRelatedBlogPostsQuery, List<BlogPostSimplifiedDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetRelatedBlogPostsQueryHandler(
        IApplicationDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<BlogPostSimplifiedDto>> Handle(GetRelatedBlogPostsQuery request, CancellationToken cancellationToken)
    {
        var originalPost = await _context.BlogPosts
            .FirstOrDefaultAsync(p => p.Id == request.PostId, cancellationToken);

        if (originalPost == null)
            return new List<BlogPostSimplifiedDto>();

        var originalPostCategories = await _context.PostCategories
            .Where(pc => pc.PostId == request.PostId)
            .Select(pc => pc.CategoryId)
            .ToListAsync(cancellationToken);

        var originalPostTags = await _context.PostTags
            .Where(pt => pt.PostId == request.PostId)
            .Select(pt => pt.TagId)
            .ToListAsync(cancellationToken);

        var otherPostsIdsWithRelevance = await _context.BlogPosts
            .Where(p => p.Id != request.PostId && p.IsPublished)
            .Select(p => new
            {
                PostId = p.Id,
                Relevance = (_context.PostCategories
                            .Count(pc => pc.PostId == p.Id && originalPostCategories.Contains(pc.CategoryId)) * 2) +
                           (_context.PostTags
                            .Count(pt => pt.PostId == p.Id && originalPostTags.Contains(pt.TagId)))
            })
            .Where(item => item.Relevance > 0) 
            .OrderByDescending(item => item.Relevance)
            .Take(request.Count)
            .ToListAsync(cancellationToken);

        if (!otherPostsIdsWithRelevance.Any())
            return new List<BlogPostSimplifiedDto>();

        var relatedPostIds = otherPostsIdsWithRelevance.Select(p => p.PostId).ToList();

        var relatedPosts = await _context.BlogPosts
            .Where(p => relatedPostIds.Contains(p.Id))
            .ProjectTo<BlogPostSimplifiedDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return relatedPosts
            .OrderBy(p => relatedPostIds.IndexOf(p.Id))
            .ToList();
    }
}
