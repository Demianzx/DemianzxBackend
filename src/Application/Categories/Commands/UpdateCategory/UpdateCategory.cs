using DemianzxBackend.Application.Common.Exceptions;
using DemianzxBackend.Application.Common.Interfaces;
using DemianzxBackend.Application.Common.Security;
using DemianzxBackend.Domain.Entities;

namespace DemianzxBackend.Application.Categories.Commands.UpdateCategory;

[Authorize(Roles = "Administrator")]
public record UpdateCategoryCommand : IRequest
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISlugService _slugService;

    public UpdateCategoryCommandHandler(
        IApplicationDbContext context,
        ISlugService slugService)
    {
        _context = context;
        _slugService = slugService;
    }

    public async Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Categories
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Category), request.Id.ToString());
        }

        entity.Name = request.Name;
        entity.Slug = _slugService.GenerateSlug(request.Name);
        entity.Description = request.Description;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
