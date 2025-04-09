namespace DemianzxBackend.Domain.Events;

public class BlogPostCreatedEvent : BaseEvent
{
    public BlogPostCreatedEvent(BlogPost item)
    {
        Item = item;
    }

    public BlogPost Item { get; }
}
