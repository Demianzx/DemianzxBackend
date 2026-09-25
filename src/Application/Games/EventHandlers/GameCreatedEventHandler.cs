using DemianzxBackend.Domain.Events;
using Microsoft.Extensions.Logging;

namespace DemianzxBackend.Application.Games.EventHandlers;

public class GameCreatedEventHandler : INotificationHandler<GameCreatedEvent>
{
    private readonly ILogger<GameCreatedEventHandler> _logger;

    public GameCreatedEventHandler(ILogger<GameCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(GameCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("DemianzxBackend Domain Event: {DomainEvent}", notification.GetType().Name);

        return Task.CompletedTask;
    }
}