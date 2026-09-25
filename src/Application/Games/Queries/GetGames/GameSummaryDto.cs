using AutoMapper;
using DemianzxBackend.Domain.Entities;

namespace DemianzxBackend.Application.Games.Queries.GetGames;

public class GameSummaryDto
{
    public int Id { get; init; }
    public int BlogPostId { get; init; }
    public string EmbedUrl { get; init; } = string.Empty;
    public string? AspectRatio { get; init; }
    public bool AllowFullScreen { get; init; }
    public string? Instructions { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Game, GameSummaryDto>();
        }
    }
}