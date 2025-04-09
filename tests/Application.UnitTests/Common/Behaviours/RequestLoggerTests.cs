using DemianzxBackend.Application.Common.Behaviours;
using DemianzxBackend.Application.Common.Interfaces;
using DemianzxBackend.Application.BlogPosts.Commands.CreateBlogPost;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace DemianzxBackend.Application.UnitTests.Common.Behaviours;

public class RequestLoggerTests
{
    private Mock<ILogger<CreateBlogPostCommand>> _logger = null!;
    private Mock<IUser> _user = null!;
    private Mock<IIdentityService> _identityService = null!;

    [SetUp]
    public void Setup()
    {
        _logger = new Mock<ILogger<CreateBlogPostCommand>>();
        _user = new Mock<IUser>();
        _identityService = new Mock<IIdentityService>();
    }

    [Test]
    public async Task ShouldCallGetUserNameAsyncOnceIfAuthenticated()
    {
        _user.Setup(x => x.Id).Returns(Guid.NewGuid().ToString());

        var requestLogger = new LoggingBehaviour<CreateBlogPostCommand>(_logger.Object, _user.Object, _identityService.Object);

        await requestLogger.Process(new CreateBlogPostCommand { Title = "title", Content = "content" }, new CancellationToken());

        _identityService.Verify(i => i.GetUserNameAsync(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task ShouldNotCallGetUserNameAsyncOnceIfUnauthenticated()
    {
        var requestLogger = new LoggingBehaviour<CreateBlogPostCommand>(_logger.Object, _user.Object, _identityService.Object);

        await requestLogger.Process(new CreateBlogPostCommand { Title = "title", Content = "content" }, new CancellationToken());

        _identityService.Verify(i => i.GetUserNameAsync(It.IsAny<string>()), Times.Never);
    }
}
