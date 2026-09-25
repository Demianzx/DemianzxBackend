namespace DemianzxBackend.Domain.Entities;

public class Game : BaseAuditableEntity
{
    public int BlogPostId { get; set; }
    public string EmbedUrl { get; set; } = string.Empty;
    public string? AspectRatio { get; set; }
    public bool AllowFullScreen { get; set; } = true;
    public string? Instructions { get; set; }

    // Relations
    public BlogPost Post { get; set; } = null!;
}