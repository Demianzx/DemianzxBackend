using System.Xml.Linq;

namespace DemianzxBackend.Domain.Entities;

public class BlogPost : BaseAuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? FeaturedImageUrl { get; set; }
    public DateTime? PublishedDate { get; set; }
    public bool IsPublished { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }
    public int ViewCount { get; set; }


    // Relations through ORM
    public IList<Comment> Comments { get; private set; } = new List<Comment>();

    // Domain Events
    public void MarkAsPublished()
    {
        if (!IsPublished)
        {
            IsPublished = true;
            PublishedDate = DateTime.UtcNow;
            AddDomainEvent(new BlogPostPublishedEvent(this));
        }
    }
    public void IncrementViewCount()
    {
        ViewCount++;
    }
}
