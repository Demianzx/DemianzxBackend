using DemianzxBackend.Application.Common.Interfaces;
using DemianzxBackend.Application.Common.Mappings;
using DemianzxBackend.Application.Common.Models;

namespace DemianzxBackend.Application.BlogPosts.Queries.GetBlogPostsSimplified;

public record GetBlogPostsSimplifiedQuery : IRequest<PaginatedList<BlogPostSimplifiedDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? CategorySlug { get; init; } = null;
    public string? TagSlug { get; init; } = null;
}

public class GetBlogPostsSimplifiedQueryHandler : IRequestHandler<GetBlogPostsSimplifiedQuery, PaginatedList<BlogPostSimplifiedDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetBlogPostsSimplifiedQueryHandler(
        IApplicationDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<BlogPostSimplifiedDto>> Handle(GetBlogPostsSimplifiedQuery request, CancellationToken cancellationToken)
    {
        var query = _context.BlogPosts
            .AsNoTracking()
            .Where(p => p.IsPublished); // Solo posts publicados

        // Filtrar por categoría si se proporciona
        if (!string.IsNullOrEmpty(request.CategorySlug))
        {
            query = query.Where(p => _context.PostCategories
                .Any(pc => pc.PostId == p.Id &&
                      _context.Categories.Any(c => c.Id == pc.CategoryId && c.Slug == request.CategorySlug)));
        }

        // Filtrar por etiqueta si se proporciona
        if (!string.IsNullOrEmpty(request.TagSlug))
        {
            query = query.Where(p => _context.PostTags
                .Any(pt => pt.PostId == p.Id &&
                      _context.Tags.Any(t => t.Id == pt.TagId && t.Slug == request.TagSlug)));
        }

        var posts = await query
            .OrderByDescending(p => p.PublishedDate)
            .ProjectTo<BlogPostSimplifiedDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);

        return posts;
    }
}
