using DemianzxBackend.Application.Categories.Queries;
using DemianzxBackend.Application.Common.Interfaces;
using DemianzxBackend.Application.Common.Mappings;
using DemianzxBackend.Application.Common.Models;
using DemianzxBackend.Application.Tags.Queries;

namespace DemianzxBackend.Application.BlogPosts.Queries.GetBlogPosts;

public record GetBlogPostsQuery : IRequest<PaginatedList<BlogPostDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public bool IncludeDrafts { get; init; } = false;
}

public class GetBlogPostsQueryHandler : IRequestHandler<GetBlogPostsQuery, PaginatedList<BlogPostDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IIdentityService _identityService;

    public GetBlogPostsQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        IIdentityService identityService)
    {
        _context = context;
        _mapper = mapper;
        _identityService = identityService;
    }

    public async Task<PaginatedList<BlogPostDto>> Handle(GetBlogPostsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.BlogPosts
            .AsNoTracking();

        if (!request.IncludeDrafts)
        {
            query = query.Where(p => p.IsPublished);
        }

        var posts = await query
            .OrderByDescending(p => p.PublishedDate)
            .ThenByDescending(p => p.Created)
            .ProjectTo<BlogPostDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);

        foreach (var post in posts.Items)
        {
            post.AuthorName = await _identityService.GetUserNameAsync(post.AuthorId) ?? string.Empty;

            var categories = await _context.PostCategories
                .Where(pc => pc.PostId == post.Id)
                .Select(pc => pc.Category)
                .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            foreach (var category in categories)
            {
                post.Categories.Add(category);
            }

            var tags = await _context.PostTags
                .Where(pt => pt.PostId == post.Id)
                .Select(pt => pt.Tag)
                .ProjectTo<TagDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            foreach (var tag in tags)
            {
                post.Tags.Add(tag);
            }
        }

        return posts;
    }
}
