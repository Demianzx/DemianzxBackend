namespace DemianzxBackend.Domain.Entities;

public class PostTag
{
    public int PostId { get; set; }
    public int TagId { get; set; }

    public BlogPost Post { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
