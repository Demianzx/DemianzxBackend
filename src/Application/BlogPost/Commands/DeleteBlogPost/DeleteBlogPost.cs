using DemianzxBackend.Application.Common.Exceptions;
using DemianzxBackend.Application.Common.Interfaces;
using DemianzxBackend.Application.Common.Security;
using DemianzxBackend.Domain.Entities;

namespace DemianzxBackend.Application.BlogPosts.Commands.DeleteBlogPost;

[Authorize]
public record DeleteBlogPostCommand(int Id) : IRequest;

public class DeleteBlogPostCommandHandler : IRequestHandler<DeleteBlogPostCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public DeleteBlogPostCommandHandler(
        IApplicationDbContext context,
        IUser user,
        IIdentityService identityService)
    {
        _context = context;
        _user = user;
        _identityService = identityService;
    }

    public async Task Handle(DeleteBlogPostCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.BlogPosts
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(BlogPost), request.Id.ToString());
        }

        // Verify if user is an author or an administrator
        if (_user.Id != entity.AuthorId)
        {
            var isAdmin = false;
            if (_user.Id != null)
            {
                // Verify if user is an administrator
                isAdmin = await _identityService.IsInRoleAsync(_user.Id, "Administrator");
            }

            if (!isAdmin)
            {
                throw new ForbiddenAccessException();
            }
        }

        // Detelete Relations
        var categories = _context.PostCategories.Where(pc => pc.PostId == entity.Id);
        var tags = _context.PostTags.Where(pt => pt.PostId == entity.Id);
        var comments = _context.Comments.Where(c => c.PostId == entity.Id);

        _context.PostCategories.RemoveRange(categories);
        _context.PostTags.RemoveRange(tags);
        _context.Comments.RemoveRange(comments);
        _context.BlogPosts.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
