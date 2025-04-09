namespace DemianzxBackend.Domain.Events;

public class BlogPostPublishedEvent : BaseEvent
{
    public BlogPostPublishedEvent(BlogPost item)
    {
        Item = item;
    }

    public BlogPost Item { get; }
}
