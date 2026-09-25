using DemianzxBackend.Application.Categories.Queries;
using DemianzxBackend.Domain.Entities;

namespace DemianzxBackend.Application.Games.Queries.GetGames;

public class GameSimplifiedDto
{
    public int Id { get; init; }
    public string EmbedUrl { get; init; } = string.Empty;
    public string? AspectRatio { get; init; }
    public bool AllowFullScreen { get; init; }
    public string? Instructions { get; init; }

    // Flattened post fields (the post is the game's publication)
    public int PostId { get; init; }
    public string PostTitle { get; init; } = string.Empty;
    public string PostSlug { get; init; } = string.Empty;
    public string? PostThumbnailImageUrl { get; init; }
    public DateTime? PostPublishedDate { get; init; }
    public bool PostIsPublished { get; init; }
    public string AuthorId { get; init; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public IList<CategoryDto> Categories { get; init; } = new List<CategoryDto>();

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Game, GameSimplifiedDto>()
                .ForMember(d => d.PostId, opt => opt.MapFrom(s => s.Post.Id))
                .ForMember(d => d.PostTitle, opt => opt.MapFrom(s => s.Post.Title))
                .ForMember(d => d.PostSlug, opt => opt.MapFrom(s => s.Post.Slug))
                .ForMember(d => d.PostThumbnailImageUrl, opt => opt.MapFrom(s => s.Post.ThumbnailImageUrl))
                .ForMember(d => d.PostPublishedDate, opt => opt.MapFrom(s => s.Post.PublishedDate))
                .ForMember(d => d.PostIsPublished, opt => opt.MapFrom(s => s.Post.IsPublished))
                .ForMember(d => d.AuthorId, opt => opt.MapFrom(s => s.Post.AuthorId))
                .ForMember(d => d.AuthorName, opt => opt.Ignore()) // filled by the handler
                .ForMember(d => d.Categories, opt => opt.Ignore()); // filled by the handler
        }
    }
}