using DemianzxBackend.Application.Common.Exceptions;
using DemianzxBackend.Application.Common.Interfaces;
using DemianzxBackend.Application.Common.Security;
using DemianzxBackend.Domain.Entities;

namespace DemianzxBackend.Application.Comments.Commands.DeleteComment;

[Authorize]
public record DeleteCommentCommand(int Id) : IRequest;

public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public DeleteCommentCommandHandler(
        IApplicationDbContext context,
        IUser user,
        IIdentityService identityService)
    {
        _context = context;
        _user = user;
        _identityService = identityService;
    }

    public async Task Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Comments
            .Include(c => c.Replies)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Comment), request.Id.ToString());
        }

        // Verificar que el usuario es el autor o es un administrador
        if (_user.Id != entity.AuthorId)
        {
            var isAdmin = false;
            if (_user.Id != null)
            {
                isAdmin = await _identityService.IsInRoleAsync(_user.Id, "Administrator");
            }

            if (!isAdmin)
            {
                throw new ForbiddenAccessException();
            }
        }

        // Delete replies recursively
        await DeleteRepliesRecursively(entity.Id, cancellationToken);

        // Delete comment
        _context.Comments.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task DeleteRepliesRecursively(int commentId, CancellationToken cancellationToken)
    {
        var replies = await _context.Comments
            .Where(c => c.ParentCommentId == commentId)
            .ToListAsync(cancellationToken);

        foreach (var reply in replies)
        {
            await DeleteRepliesRecursively(reply.Id, cancellationToken);
            _context.Comments.Remove(reply);
        }
    }
}
