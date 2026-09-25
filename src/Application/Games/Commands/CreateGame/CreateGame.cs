using DemianzxBackend.Application.Common.Exceptions;
using DemianzxBackend.Application.Common.Interfaces;
using DemianzxBackend.Application.Common.Security;
using DemianzxBackend.Domain.Entities;
using DemianzxBackend.Domain.Events;
using FluentValidation.Results;
using ValidationException = DemianzxBackend.Application.Common.Exceptions.ValidationException;

namespace DemianzxBackend.Application.Games.Commands.CreateGame;

[Authorize]
public record CreateGameCommand : IRequest<int>
{
    public int BlogPostId { get; init; }
    public string EmbedUrl { get; init; } = string.Empty;
    public string? AspectRatio { get; init; }
    public bool AllowFullScreen { get; init; } = true;
    public string? Instructions { get; init; }
}

public class CreateGameCommandHandler : IRequestHandler<CreateGameCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public CreateGameCommandHandler(
        IApplicationDbContext context,
        IUser user,
        IIdentityService identityService)
    {
        _context = context;
        _user = user;
        _identityService = identityService;
    }

    public async Task<int> Handle(CreateGameCommand request, CancellationToken cancellationToken)
    {
        var post = await _context.BlogPosts
            .FindAsync(new object[] { request.BlogPostId }, cancellationToken);

        if (post == null)
        {
            throw new NotFoundException(nameof(BlogPost), request.BlogPostId.ToString());
        }

        // Verify if user is the post author or an administrator
        if (_user.Id != post.AuthorId)
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

        // Verify the post doesn't already have a game (one-to-one)
        bool postHasGame = await _context.Games
            .AnyAsync(g => g.BlogPostId == request.BlogPostId, cancellationToken);

        if (postHasGame)
        {
            throw new ValidationException(new[] { new ValidationFailure("BlogPostId", "This blog post already has an associated game.") });
        }

        var entity = new Game
        {
            BlogPostId = request.BlogPostId,
            EmbedUrl = request.EmbedUrl,
            AspectRatio = request.AspectRatio,
            AllowFullScreen = request.AllowFullScreen,
            Instructions = request.Instructions
        };

        entity.AddDomainEvent(new GameCreatedEvent(entity));

        _context.Games.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}