using DemianzxBackend.Application.Common.Interfaces;
using DemianzxBackend.Application.Common.Security;
using DemianzxBackend.Domain.Entities;

namespace DemianzxBackend.Application.Tags.Commands.CreateTag;

[Authorize(Roles = "Administrator")]
public record CreateTagCommand : IRequest<int>
{
    public string Name { get; init; } = string.Empty;
}

public class CreateTagCommandHandler : IRequestHandler<CreateTagCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ISlugService _slugService;

    public CreateTagCommandHandler(
        IApplicationDbContext context,
        ISlugService slugService)
    {
        _context = context;
        _slugService = slugService;
    }

    public async Task<int> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        var entity = new Tag
        {
            Name = request.Name,
            Slug = _slugService.GenerateSlug(request.Name)
        };

        _context.Tags.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
