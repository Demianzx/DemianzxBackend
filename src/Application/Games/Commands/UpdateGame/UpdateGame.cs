using DemianzxBackend.Application.Common.Exceptions;
using DemianzxBackend.Application.Common.Interfaces;
using DemianzxBackend.Application.Common.Security;
using DemianzxBackend.Domain.Entities;

namespace DemianzxBackend.Application.Games.Commands.UpdateGame;

[Authorize]
public record UpdateGameCommand : IRequest
{
    public int Id { get; init; }
    public string EmbedUrl { get; init; } = string.Empty;
    public string? AspectRatio { get; init; }
    public bool AllowFullScreen { get; init; } = true;
    public string? Instructions { get; init; }
}

public class UpdateGameCommandHandler : IRequestHandler<UpdateGameCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public UpdateGameCommandHandler(
        IApplicationDbContext context,
        IUser user,
        IIdentityService identityService)
    {
        _context = context;
        _user = user;
        _identityService = identityService;
    }

    public async Task Handle(UpdateGameCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Games
            .Include(g => g.Post)
            .FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Game), request.Id.ToString());
        }

        // Verify if user is the post author or an administrator
        if (entity.Post == null || _user.Id != entity.Post.AuthorId)
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

        entity.EmbedUrl = request.EmbedUrl;
        entity.AspectRatio = request.AspectRatio;
        entity.AllowFullScreen = request.AllowFullScreen;
        entity.Instructions = request.Instructions;

        await _context.SaveChangesAsync(cancellationToken);
    }
}