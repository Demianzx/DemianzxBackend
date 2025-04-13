using DemianzxBackend.Application.Common.Interfaces;
using DemianzxBackend.Application.Common.Security;
using DemianzxBackend.Domain.Entities;
using DemianzxBackend.Domain.Events;

namespace DemianzxBackend.Application.BlogPosts.Commands.CreateBlogPost;

[Authorize]
public record CreateBlogPostCommand : IRequest<int>
{
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string? FeaturedImageUrl { get; init; }
    public bool IsPublished { get; init; }
    public bool IsFeatured { get; init; } = false;
    public List<int> CategoryIds { get; init; } = new List<int>();
    public List<int> TagIds { get; init; } = new List<int>();
}

public class CreateBlogPostCommandHandler : IRequestHandler<CreateBlogPostCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ISlugService _slugService;

    public CreateBlogPostCommandHandler(
        IApplicationDbContext context,
        IUser user,
        ISlugService slugService)
    {
        _context = context;
        _user = user;
        _slugService = slugService;
    }

    public async Task<int> Handle(CreateBlogPostCommand request, CancellationToken cancellationToken)
    {
        if (_user.Id == null)
            throw new UnauthorizedAccessException();

        var entity = new BlogPost
        {
            Title = request.Title,
            Content = request.Content,
            Slug = _slugService.GenerateSlug(request.Title),
            FeaturedImageUrl = request.FeaturedImageUrl,
            AuthorId = _user.Id,
            IsPublished = request.IsPublished,
            IsFeatured = request.IsFeatured,
            PublishedDate = request.IsPublished ? DateTime.UtcNow : null
        };

        entity.AddDomainEvent(new BlogPostCreatedEvent(entity));

        _context.BlogPosts.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        // Add Categories
        foreach (var categoryId in request.CategoryIds)
        {
            _context.PostCategories.Add(new PostCategory
            {
                PostId = entity.Id,
                CategoryId = categoryId
            });
        }

        // Add tags
        foreach (var tagId in request.TagIds)
        {
            _context.PostTags.Add(new PostTag
            {
                PostId = entity.Id,
                TagId = tagId
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
