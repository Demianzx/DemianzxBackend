namespace DemianzxBackend.Domain.Entities;

public class Comment : BaseAuditableEntity
{
    public string Content { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public int PostId { get; set; }
    public int? ParentCommentId { get; set; }

    // Relations
    public BlogPost Post { get; set; } = null!;
    public IList<Comment> Replies { get; private set; } = new List<Comment>();
}
