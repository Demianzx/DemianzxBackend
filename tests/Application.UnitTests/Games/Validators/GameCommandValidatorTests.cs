using DemianzxBackend.Application.Games.Commands.CreateGame;
using DemianzxBackend.Application.Games.Commands.UpdateGame;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace DemianzxBackend.Application.UnitTests.Games.Validators;

[TestFixture]
public class CreateGameCommandValidatorTests
{
    private CreateGameCommandValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new CreateGameCommandValidator();
    }

    private static CreateGameCommand ValidCommand() => new()
    {
        BlogPostId = 1,
        EmbedUrl = "https://user.github.io/my-game/",
        AspectRatio = "16:9",
        AllowFullScreen = true
    };

    [Test]
    public void ShouldPassForValidCommand()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void ShouldFailForEmptyEmbedUrl()
    {
        var command = ValidCommand() with { EmbedUrl = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.EmbedUrl);
    }

    [TestCase("not-a-url")]
    [TestCase("ftp://example.com/game")]
    [TestCase("javascript:alert(1)")]
    [TestCase("data:text/html,<script>")]
    [TestCase("/relative/path")]
    public void ShouldFailForInvalidOrUnsafeUrl(string url)
    {
        var command = ValidCommand() with { EmbedUrl = url };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.EmbedUrl);
    }

    [Test]
    public void ShouldFailForInvalidBlogPostId()
    {
        var command = ValidCommand() with { BlogPostId = 0 };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.BlogPostId);
    }

    [TestCase("16:9")]
    [TestCase("4:3")]
    [TestCase("1:1")]
    public void ShouldPassForValidAspectRatios(string ratio)
    {
        var command = ValidCommand() with { AspectRatio = ratio };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [TestCase("16x9")]
    [TestCase("wide")]
    [TestCase("1:2:3")]
    public void ShouldFailForInvalidAspectRatios(string ratio)
    {
        var command = ValidCommand() with { AspectRatio = ratio };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.AspectRatio);
    }

    [Test]
    public void ShouldFailForTooLongInstructions()
    {
        var command = ValidCommand() with { Instructions = new string('a', 2001) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Instructions);
    }
}

[TestFixture]
public class UpdateGameCommandValidatorTests
{
    private UpdateGameCommandValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new UpdateGameCommandValidator();
    }

    private static UpdateGameCommand ValidCommand() => new()
    {
        Id = 1,
        EmbedUrl = "https://user.github.io/my-game/",
        AspectRatio = "16:9",
        AllowFullScreen = true
    };

    [Test]
    public void ShouldPassForValidCommand()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void ShouldFailForInvalidId()
    {
        var command = ValidCommand() with { Id = 0 };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Id);
    }

    [TestCase("javascript:alert(1)")]
    [TestCase("not-a-url")]
    public void ShouldFailForInvalidUrl(string url)
    {
        var command = ValidCommand() with { EmbedUrl = url };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.EmbedUrl);
    }
}