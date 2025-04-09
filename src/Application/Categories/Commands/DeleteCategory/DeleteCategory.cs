using DemianzxBackend.Application.Common.Exceptions;
using DemianzxBackend.Application.Common.Interfaces;
using DemianzxBackend.Application.Common.Security;
using DemianzxBackend.Domain.Entities;

namespace DemianzxBackend.Application.Categories.Commands.DeleteCategory;

[Authorize(Roles = "Administrator")]
public record DeleteCategoryCommand(int Id) : IRequest;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Categories
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Category), request.Id.ToString());
        }

        // Verify if there are related posts
        var hasAssociatedPosts = await _context.PostCategories
            .AnyAsync(pc => pc.CategoryId == request.Id, cancellationToken);

        if (hasAssociatedPosts)
        {
            throw new InvalidOperationException("Cannot delete category with associated posts");
        }

        _context.Categories.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
