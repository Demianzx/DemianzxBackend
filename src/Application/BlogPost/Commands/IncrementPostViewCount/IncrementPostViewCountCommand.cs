using DemianzxBackend.Application.Common.Exceptions;
using DemianzxBackend.Application.Common.Interfaces;
using DemianzxBackend.Domain.Entities;

namespace DemianzxBackend.Application.BlogPosts.Commands.IncrementPostViewCount;

public record IncrementPostViewCountCommand(int Id) : IRequest;

public class IncrementPostViewCountCommandHandler : IRequestHandler<IncrementPostViewCountCommand>
{
    private readonly IApplicationDbContext _context;

    public IncrementPostViewCountCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(IncrementPostViewCountCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.BlogPosts
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(BlogPost), request.Id.ToString());
        }

        // Incrementa el contador de vistas
        entity.ViewCount++;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
