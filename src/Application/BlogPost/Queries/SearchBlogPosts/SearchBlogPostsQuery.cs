using DemianzxBackend.Application.Common.Interfaces;
using DemianzxBackend.Application.Common.Mappings;
using DemianzxBackend.Application.Common.Models;
using DemianzxBackend.Application.BlogPosts.Queries.GetBlogPosts;
using System;

namespace DemianzxBackend.Application.BlogPosts.Queries.SearchBlogPosts;

public record SearchBlogPostsQuery : IRequest<PaginatedList<BlogPostDto>>
{
    public string? SearchText { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string? CategorySlug { get; init; }
    public string? TagSlug { get; init; }
    public string? AuthorId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SortBy { get; init; } = "PublishedDate"; // Default sort by published date
    public string? SortDirection { get; init; } = "desc"; // Default sort direction descending
}

public class SearchBlogPostsQueryHandler : IRequestHandler<SearchBlogPostsQuery, PaginatedList<BlogPostDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IIdentityService _identityService;

    public SearchBlogPostsQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        IIdentityService identityService)
    {
        _context = context;
        _mapper = mapper;
        _identityService = identityService;
    }

    public async Task<PaginatedList<BlogPostDto>> Handle(SearchBlogPostsQuery request, CancellationToken cancellationToken)
    {
        // Start with base query for published posts only
        var query = _context.BlogPosts
            .AsNoTracking()
            .Where(p => p.IsPublished);

        // Apply search filters
        if (!string.IsNullOrEmpty(request.SearchText))
        {
            string searchTerm = request.SearchText.ToLower();
            query = query.Where(p =>
                p.Title.ToLower().Contains(searchTerm) ||
                p.Content.ToLower().Contains(searchTerm));
        }

        // Date filtering
        if (request.FromDate.HasValue)
        {
            query = query.Where(p => p.PublishedDate >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            // Add one day to include the end date fully
            var toDatePlusOneDay = request.ToDate.Value.AddDays(1);
            query = query.Where(p => p.PublishedDate < toDatePlusOneDay);
        }

        // Filter by category
        if (!string.IsNullOrEmpty(request.CategorySlug))
        {
            query = query.Where(p => _context.PostCategories
                .Any(pc => pc.PostId == p.Id &&
                     _context.Categories.Any(c => c.Id == pc.CategoryId && c.Slug == request.CategorySlug)));
        }

        // Filter by tag
        if (!string.IsNullOrEmpty(request.TagSlug))
        {
            query = query.Where(p => _context.PostTags
                .Any(pt => pt.PostId == p.Id &&
                     _context.Tags.Any(t => t.Id == pt.TagId && t.Slug == request.TagSlug)));
        }

        // Filter by author
        if (!string.IsNullOrEmpty(request.AuthorId))
        {
            query = query.Where(p => p.AuthorId == request.AuthorId);
        }

        // Apply sorting
        query = ApplySorting(query, request.SortBy, request.SortDirection);

        // Get paged results
        var posts = await query
            .ProjectTo<BlogPostDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);

        // Populate related data
        foreach (var post in posts.Items)
        {
            post.AuthorName = await _identityService.GetUserNameAsync(post.AuthorId) ?? string.Empty;

            // Get categories
            var categories = await _context.PostCategories
                .Where(pc => pc.PostId == post.Id)
                .Select(pc => pc.Category)
                .ProjectTo<Application.Categories.Queries.CategoryDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            foreach (var category in categories)
            {
                post.Categories.Add(category);
            }

            // Get tags
            var tags = await _context.PostTags
                .Where(pt => pt.PostId == post.Id)
                .Select(pt => pt.Tag)
                .ProjectTo<Application.Tags.Queries.TagDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            foreach (var tag in tags)
            {
                post.Tags.Add(tag);
            }
        }

        return posts;
    }

    private IQueryable<Domain.Entities.BlogPost> ApplySorting(
        IQueryable<Domain.Entities.BlogPost> query,
        string? sortBy,
        string? sortDirection)
    {
        var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return sortBy?.ToLower() switch
        {
            "title" => isDescending
                ? query.OrderByDescending(p => p.Title)
                : query.OrderBy(p => p.Title),

            "created" => isDescending
                ? query.OrderByDescending(p => p.Created)
                : query.OrderBy(p => p.Created),

            "viewcount" => isDescending
                ? query.OrderByDescending(p => p.ViewCount)
                : query.OrderBy(p => p.ViewCount),

            // Default is published date
            _ => isDescending
                ? query.OrderByDescending(p => p.PublishedDate)
                    .ThenByDescending(p => p.Created)
                : query.OrderBy(p => p.PublishedDate)
                    .ThenBy(p => p.Created)
        };
    }
}
