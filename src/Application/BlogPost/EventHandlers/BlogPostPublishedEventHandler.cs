using DemianzxBackend.Domain.Events;
using Microsoft.Extensions.Logging;

namespace DemianzxBackend.Application.BlogPosts.EventHandlers;

public class BlogPostPublishedEventHandler : INotificationHandler<BlogPostPublishedEvent>
{
    private readonly ILogger<BlogPostPublishedEventHandler> _logger;

    public BlogPostPublishedEventHandler(ILogger<BlogPostPublishedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(BlogPostPublishedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("DemianzxBackend Domain Event: {DomainEvent}", notification.GetType().Name);

        return Task.CompletedTask;
    }
}
