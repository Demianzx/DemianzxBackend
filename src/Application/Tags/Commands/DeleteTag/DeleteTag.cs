using DemianzxBackend.Application.Common.Exceptions;
using DemianzxBackend.Application.Common.Interfaces;
using DemianzxBackend.Application.Common.Security;
using DemianzxBackend.Domain.Entities;

namespace DemianzxBackend.Application.Tags.Commands.DeleteTag;

[Authorize(Roles = "Administrator")]
public record DeleteTagCommand(int Id) : IRequest;

public class DeleteTagCommandHandler : IRequestHandler<DeleteTagCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteTagCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteTagCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Tags
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Tag), request.Id.ToString());
        }

        // Verify related posts
        var hasAssociatedPosts = await _context.PostTags
            .AnyAsync(pt => pt.TagId == request.Id, cancellationToken);

        if (hasAssociatedPosts)
        {
            throw new InvalidOperationException("Cannot delete tag with associated posts");
        }

        _context.Tags.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
