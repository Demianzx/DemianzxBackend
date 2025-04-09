namespace DemianzxBackend.Domain.Events;

public class CommentAddedEvent : BaseEvent
{
    public CommentAddedEvent(Comment item)
    {
        Item = item;
    }

    public Comment Item { get; }
}
