using DemianzxBackend.Application.Common.Interfaces;
using DemianzxBackend.Application.Common.Security;
using DemianzxBackend.Domain.Entities;
using DemianzxBackend.Domain.Events;

namespace DemianzxBackend.Application.Comments.Commands.CreateComment;

[Authorize]
public record CreateCommentCommand : IRequest<int>
{
    public string Content { get; init; } = string.Empty;
    public int PostId { get; init; }
    public int? ParentCommentId { get; init; }
}

public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CreateCommentCommandHandler(
        IApplicationDbContext context,
        IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<int> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        if (_user.Id == null)
            throw new UnauthorizedAccessException();

        // Verify if post exists
        var post = await _context.BlogPosts
            .FindAsync(new object[] { request.PostId }, cancellationToken);

        if (post == null)
            throw new NotFoundException(nameof(BlogPost), request.PostId.ToString());

        // Verify if comment parent exists if especifies
        if (request.ParentCommentId.HasValue)
        {
            var parentComment = await _context.Comments
                .FindAsync(new object[] { request.ParentCommentId.Value }, cancellationToken);

            if (parentComment == null)
                throw new NotFoundException(nameof(Comment), request.ParentCommentId.Value.ToString());

            // Verify that the parent comments from same post
            if (parentComment.PostId != request.PostId)
                throw new InvalidOperationException("Parent comment does not belong to the specified post");
        }

        var entity = new Comment
        {
            Content = request.Content,
            PostId = request.PostId,
            AuthorId = _user.Id,
            ParentCommentId = request.ParentCommentId
        };

        entity.AddDomainEvent(new CommentAddedEvent(entity));

        _context.Comments.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
