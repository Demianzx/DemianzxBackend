using DemianzxBackend.Application.Common.Exceptions;
using DemianzxBackend.Application.Common.Interfaces;
using DemianzxBackend.Application.Common.Security;
using DemianzxBackend.Domain.Entities;
using DemianzxBackend.Domain.Events;

namespace DemianzxBackend.Application.BlogPosts.Commands.UpdateBlogPost;

[Authorize]
public record UpdateBlogPostCommand : IRequest
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string? FeaturedImageUrl { get; init; }
    public bool IsPublished { get; init; }
    public List<int> CategoryIds { get; init; } = new List<int>();
    public List<int> TagIds { get; init; } = new List<int>();
}

public class UpdateBlogPostCommandHandler : IRequestHandler<UpdateBlogPostCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ISlugService _slugService;
    private readonly IIdentityService _identityService;

    public UpdateBlogPostCommandHandler(
        IApplicationDbContext context,
        IUser user,
        ISlugService slugService,
        IIdentityService identityService)
    {
        _context = context;
        _user = user;
        _slugService = slugService;
        _identityService = identityService;
    }

    public async Task Handle(UpdateBlogPostCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.BlogPosts
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(BlogPost), request.Id.ToString());
        }

        // Verify if user is an author or administrator
        if (_user.Id != entity.AuthorId)
        {
            var isAdmin = false;
            if (_user.Id != null)
            {
                // Verify if user is an admin (using identity)
                isAdmin = await _identityService.IsInRoleAsync(_user.Id, "Administrator");
            }

            if (!isAdmin)
            {
                throw new ForbiddenAccessException();
            }
        }

        entity.Title = request.Title;
        entity.Content = request.Content;
        entity.FeaturedImageUrl = request.FeaturedImageUrl;

        // If isn't published then no will be published
        if (!entity.IsPublished && request.IsPublished)
        {
            entity.IsPublished = true;
            entity.PublishedDate = DateTime.UtcNow;
            entity.AddDomainEvent(new BlogPostPublishedEvent(entity));
        }
        else
        {
            entity.IsPublished = request.IsPublished;
        }
        //verfy if categories exists
        foreach (var categoryId in request.CategoryIds)
        {
            bool categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == categoryId, cancellationToken);

            if (!categoryExists)
            {
                throw new NotFoundException(nameof(Category), categoryId.ToString());
            }
        }
        // verify if tags exists
        foreach (var tagId in request.TagIds)
        {
            bool tagExists = await _context.Tags
                .AnyAsync(t => t.Id == tagId, cancellationToken);

            if (!tagExists)
            {
                throw new NotFoundException(nameof(Tag), tagId.ToString());
            }
        }
        //update categories

        var existingCategories = await _context.PostCategories
        .Where(pc => pc.PostId == entity.Id)
        .ToListAsync(cancellationToken);

        _context.PostCategories.RemoveRange(existingCategories);

        foreach (var categoryId in request.CategoryIds)
        {
            _context.PostCategories.Add(new PostCategory
            {
                PostId = entity.Id,
                CategoryId = categoryId
            });
        }

        // Update tags
        var existingTags = await _context.PostTags
        .Where(pt => pt.PostId == entity.Id)
        .ToListAsync(cancellationToken);

        _context.PostTags.RemoveRange(existingTags);

        foreach (var tagId in request.TagIds)
        {
            _context.PostTags.Add(new PostTag
            {
                PostId = entity.Id,
                TagId = tagId
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
