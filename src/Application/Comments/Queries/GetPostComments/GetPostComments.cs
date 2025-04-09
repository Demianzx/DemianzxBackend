using DemianzxBackend.Application.Common.Interfaces;

namespace DemianzxBackend.Application.Comments.Queries.GetPostComments;

public record GetPostCommentsQuery(int PostId) : IRequest<List<CommentDto>>;

public class GetPostCommentsQueryHandler : IRequestHandler<GetPostCommentsQuery, List<CommentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IIdentityService _identityService;

    public GetPostCommentsQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        IIdentityService identityService)
    {
        _context = context;
        _mapper = mapper;
        _identityService = identityService;
    }

    public async Task<List<CommentDto>> Handle(GetPostCommentsQuery request, CancellationToken cancellationToken)
    {
        // Get root comments
        var rootComments = await _context.Comments
            .Where(c => c.PostId == request.PostId && c.ParentCommentId == null)
            .OrderByDescending(c => c.Created)
            .ProjectTo<CommentDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        // Process every root comment
        foreach (var comment in rootComments)
        {
            // Get Author name
            comment.AuthorName = await _identityService.GetUserNameAsync(comment.AuthorId) ?? string.Empty;

            // Get replies recursive
            await LoadReplies(comment, cancellationToken);
        }

        return rootComments;
    }

    private async Task LoadReplies(CommentDto comment, CancellationToken cancellationToken)
    {
        var replies = await _context.Comments
            .Where(c => c.ParentCommentId == comment.Id)
            .OrderBy(c => c.Created)
            .ProjectTo<CommentDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        foreach (var reply in replies)
        {
            reply.AuthorName = await _identityService.GetUserNameAsync(reply.AuthorId) ?? string.Empty;
            await LoadReplies(reply, cancellationToken);
            comment.Replies.Add(reply);
        }
    }
}
