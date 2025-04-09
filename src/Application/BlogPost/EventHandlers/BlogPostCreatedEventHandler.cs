using DemianzxBackend.Domain.Events;
using Microsoft.Extensions.Logging;

namespace DemianzxBackend.Application.BlogPosts.EventHandlers;

public class BlogPostCreatedEventHandler : INotificationHandler<BlogPostCreatedEvent>
{
    private readonly ILogger<BlogPostCreatedEventHandler> _logger;

    public BlogPostCreatedEventHandler(ILogger<BlogPostCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(BlogPostCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("DemianzxBackend Domain Event: {DomainEvent}", notification.GetType().Name);

        return Task.CompletedTask;
    }
}
