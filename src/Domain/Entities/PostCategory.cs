namespace DemianzxBackend.Domain.Entities;

public class PostCategory
{
    public int PostId { get; set; }
    public int CategoryId { get; set; }

    public BlogPost Post { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
