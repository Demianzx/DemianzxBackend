using DemianzxBackend.Application.Categories.Queries;
using DemianzxBackend.Application.Common.Interfaces;
using DemianzxBackend.Application.Common.Mappings;
using DemianzxBackend.Application.Common.Models;

namespace DemianzxBackend.Application.Games.Queries.GetGames;

public record GetGamesQuery : IRequest<PaginatedList<GameSimplifiedDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 12;
    public string? CategorySlug { get; init; }
    public bool IncludeDrafts { get; init; } = false;
}

public class GetGamesQueryHandler : IRequestHandler<GetGamesQuery, PaginatedList<GameSimplifiedDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IIdentityService _identityService;

    public GetGamesQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        IIdentityService identityService)
    {
        _context = context;
        _mapper = mapper;
        _identityService = identityService;
    }

    public async Task<PaginatedList<GameSimplifiedDto>> Handle(GetGamesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Games
            .AsNoTracking();

        if (!request.IncludeDrafts)
        {
            // Only games attached to published posts
            query = query.Where(g => g.Post.IsPublished);
        }

        // Filter by the post's category if provided
        if (!string.IsNullOrEmpty(request.CategorySlug))
        {
            query = query.Where(g => _context.PostCategories
                .Any(pc => pc.PostId == g.BlogPostId &&
                      _context.Categories.Any(c => c.Id == pc.CategoryId && c.Slug == request.CategorySlug)));
        }

        var games = await query
            .OrderByDescending(g => g.Post.PublishedDate)
            .ThenByDescending(g => g.Created)
            .ProjectTo<GameSimplifiedDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);

        // Fill author names and categories (same pattern as GetBlogPostsQuery)
        foreach (var game in games.Items)
        {
            game.AuthorName = await _identityService.GetUserNameAsync(game.AuthorId) ?? string.Empty;

            var categories = await _context.PostCategories
                .Where(pc => pc.PostId == game.PostId)
                .Select(pc => pc.Category)
                .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            foreach (var category in categories)
            {
                game.Categories.Add(category);
            }
        }

        return games;
    }
}