using DemianzxBackend.Domain.Entities;

namespace DemianzxBackend.Application.Tags.Queries;

public class TagDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Tag, TagDto>();
        }
    }
}
