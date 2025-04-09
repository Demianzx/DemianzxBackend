namespace DemianzxBackend.Application.Comments.Commands.CreateComment;

public class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        RuleFor(v => v.Content)
            .NotEmpty().WithMessage("Comment content is required.")
            .MaximumLength(1000).WithMessage("Comment must not exceed 1000 characters.");

        RuleFor(v => v.PostId)
            .GreaterThan(0).WithMessage("A valid post id is required.");
    }
}
