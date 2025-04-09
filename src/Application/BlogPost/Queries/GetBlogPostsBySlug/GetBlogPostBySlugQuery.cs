using DemianzxBackend.Application.BlogPosts.Queries.GetBlogPosts;
using DemianzxBackend.Application.Categories.Queries;
using DemianzxBackend.Application.Common.Interfaces;
using DemianzxBackend.Application.Tags.Queries;

namespace DemianzxBackend.Application.BlogPosts.Queries.GetBlogPostBySlug;

public record GetBlogPostBySlugQuery : IRequest<BlogPostDto?>
{
    public string Slug { get; init; } = string.Empty;
}

public class GetBlogPostBySlugQueryHandler : IRequestHandler<GetBlogPostBySlugQuery, BlogPostDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IIdentityService _identityService;

    public GetBlogPostBySlugQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        IIdentityService identityService)
    {
        _context = context;
        _mapper = mapper;
        _identityService = identityService;
    }

    public async Task<BlogPostDto?> Handle(GetBlogPostBySlugQuery request, CancellationToken cancellationToken)
    {
        var post = await _context.BlogPosts
            .Where(p => p.Slug == request.Slug)
            .ProjectTo<BlogPostDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (post == null)
            return null;

        // Get Author name
        post.AuthorName = await _identityService.GetUserNameAsync(post.AuthorId) ?? string.Empty;

        // Get Category
        var categories = await _context.PostCategories
            .Where(pc => pc.PostId == post.Id)
            .Select(pc => pc.Category)
            .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        foreach (var category in categories)
        {
            post.Categories.Add(category);
        }

        // Get Tag
        var tags = await _context.PostTags
            .Where(pt => pt.PostId == post.Id)
            .Select(pt => pt.Tag)
            .ProjectTo<TagDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        foreach (var tag in tags)
        {
            post.Tags.Add(tag);
        }

        return post;
    }
}
