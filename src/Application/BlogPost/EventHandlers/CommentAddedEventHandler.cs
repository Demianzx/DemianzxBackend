using DemianzxBackend.Domain.Events;
using Microsoft.Extensions.Logging;

namespace DemianzxBackend.Application.Comments.EventHandlers;

public class CommentAddedEventHandler : INotificationHandler<CommentAddedEvent>
{
    private readonly ILogger<CommentAddedEventHandler> _logger;

    public CommentAddedEventHandler(ILogger<CommentAddedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(CommentAddedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("DemianzxBackend Domain Event: {DomainEvent}", notification.GetType().Name);

        return Task.CompletedTask;
    }
}
