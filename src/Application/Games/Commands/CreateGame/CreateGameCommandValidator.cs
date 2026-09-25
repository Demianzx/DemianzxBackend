namespace DemianzxBackend.Application.Games.Commands.CreateGame;

public class CreateGameCommandValidator : AbstractValidator<CreateGameCommand>
{
    public CreateGameCommandValidator()
    {
        RuleFor(v => v.BlogPostId)
            .GreaterThan(0).WithMessage("BlogPostId must be greater than 0.");

        RuleFor(v => v.EmbedUrl)
            .NotEmpty().WithMessage("EmbedUrl is required.")
            .MaximumLength(500).WithMessage("EmbedUrl must not exceed 500 characters.")
            .Must(BeAValidHttpUrl!).WithMessage("EmbedUrl must be a valid http/https URL.");

        RuleFor(v => v.AspectRatio)
            .MaximumLength(10).WithMessage("AspectRatio must not exceed 10 characters.")
            .Matches("^[0-9]{1,2}:[0-9]{1,2}$")
            .When(v => !string.IsNullOrEmpty(v.AspectRatio))
            .WithMessage("AspectRatio must have the format 'width:height', e.g. '16:9'.");

        RuleFor(v => v.Instructions)
            .MaximumLength(2000).WithMessage("Instructions must not exceed 2000 characters.");
    }

    private static bool BeAValidHttpUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}