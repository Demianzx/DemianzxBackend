using DemianzxBackend.Domain.Entities;

namespace DemianzxBackend.Application.Comments.Queries;

public class CommentDto
{
    public int Id { get; init; }
    public string Content { get; init; } = string.Empty;
    public DateTimeOffset Created { get; init; }
    public string AuthorId { get; init; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public int PostId { get; init; }
    public int? ParentCommentId { get; init; }
    public List<CommentDto> Replies { get; init; } = new List<CommentDto>();

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Comment, CommentDto>()
                .ForMember(d => d.AuthorName, opt => opt.Ignore()) 
                .ForMember(d => d.Replies, opt => opt.Ignore());   
        }
    }
}
