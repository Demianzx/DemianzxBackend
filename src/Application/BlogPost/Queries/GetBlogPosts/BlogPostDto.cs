using DemianzxBackend.Application.Categories.Queries;
using DemianzxBackend.Application.Tags.Queries;
using DemianzxBackend.Domain.Entities;

namespace DemianzxBackend.Application.BlogPosts.Queries.GetBlogPosts;

public class BlogPostDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string? FeaturedImageUrl { get; init; }
    public DateTime? PublishedDate { get; init; }
    public bool IsPublished { get; init; }
    public bool IsFeatured { get; init; }
    public string AuthorId { get; init; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public IList<CategoryDto> Categories { get; init; } = new List<CategoryDto>();
    public IList<TagDto> Tags { get; init; } = new List<TagDto>();

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<BlogPost, BlogPostDto>()
                .ForMember(d => d.AuthorName, opt => opt.Ignore()); // Esto se llenará desde el handler
        }
    }
}
