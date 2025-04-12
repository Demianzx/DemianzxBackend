using DemianzxBackend.Domain.Entities;

namespace DemianzxBackend.Application.BlogPosts.Queries.GetBlogPostsSimplified;

public class BlogPostSimplifiedDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? FeaturedImageUrl { get; init; }
    public string Excerpt { get; init; } = string.Empty;

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<BlogPost, BlogPostSimplifiedDto>()
                .ForMember(d => d.Excerpt, opt => opt.MapFrom(s =>
                    s.Content.Length <= 200 ?
                    s.Content :
                    s.Content.Substring(0, 200) + "..."));
        }
    }
}
