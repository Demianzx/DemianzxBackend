using System.Net.Mail;
using System.Text;
using DemianzxBackend.Application.Common.Exceptions;
using DemianzxBackend.Application.Common.Interfaces;
using DemianzxBackend.Infrastructure.Identity;
using DemianzxBackend.Infrastructure.Email;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace DemianzxBackend.Application.UnitTests.Users;

public class PasswordRecoveryServiceTests
{
    private ApplicationUser _user = null!;
    private Mock<IEmailSender> _emailSender = null!;
    private UserManager<ApplicationUser> _userManager = null!;
    private PasswordRecoveryService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _user = new ApplicationUser
        {
            Id = "test-user",
            UserName = "player",
            Email = "player+blog@example.com",
            SecurityStamp = Guid.NewGuid().ToString()
        };

        var store = new Mock<IUserPasswordStore<ApplicationUser>>();
        var emailStore = store.As<IUserEmailStore<ApplicationUser>>();
        var stampStore = store.As<IUserSecurityStampStore<ApplicationUser>>();
        store.Setup(x => x.GetUserIdAsync(_user, It.IsAny<CancellationToken>())).ReturnsAsync(_user.Id);
        store.Setup(x => x.GetUserNameAsync(_user, It.IsAny<CancellationToken>())).ReturnsAsync(_user.UserName);
        store.Setup(x => x.HasPasswordAsync(_user, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        store.Setup(x => x.GetPasswordHashAsync(_user, It.IsAny<CancellationToken>())).ReturnsAsync(() => _user.PasswordHash);
        store.Setup(x => x.SetPasswordHashAsync(_user, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Callback<ApplicationUser, string?, CancellationToken>((user, hash, _) => user.PasswordHash = hash)
            .Returns(Task.CompletedTask);
        store.Setup(x => x.UpdateAsync(_user, It.IsAny<CancellationToken>())).ReturnsAsync(IdentityResult.Success);
        emailStore.Setup(x => x.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string email, CancellationToken _) =>
                string.Equals(email, _user.Email, StringComparison.OrdinalIgnoreCase) ? _user : null);
        emailStore.Setup(x => x.GetEmailAsync(_user, It.IsAny<CancellationToken>())).ReturnsAsync(() => _user.Email);
        stampStore.Setup(x => x.GetSecurityStampAsync(_user, It.IsAny<CancellationToken>())).ReturnsAsync(() => _user.SecurityStamp);
        stampStore.Setup(x => x.SetSecurityStampAsync(_user, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<ApplicationUser, string, CancellationToken>((user, stamp, _) => user.SecurityStamp = stamp)
            .Returns(Task.CompletedTask);

        var identityOptions = Options.Create(new IdentityOptions());
        identityOptions.Value.Password.RequiredLength = 8;
        _userManager = new UserManager<ApplicationUser>(store.Object, identityOptions,
            new PasswordHasher<ApplicationUser>(), Array.Empty<IUserValidator<ApplicationUser>>(),
            new[] { new PasswordValidator<ApplicationUser>() }, new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(), Mock.Of<IServiceProvider>(), NullLogger<UserManager<ApplicationUser>>.Instance);
        RegisterTokenProvider(TimeSpan.FromHours(1));
        _emailSender = new Mock<IEmailSender>();
        _emailSender.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var environment = new Mock<IHostEnvironment>();
        environment.SetupGet(x => x.EnvironmentName).Returns(Environments.Production);
        _service = new PasswordRecoveryService(_userManager, _emailSender.Object,
            Options.Create(new PasswordRecoveryOptions { ResetUrl = "https://blog.example.com/reset-password" }),
            environment.Object, NullLogger<PasswordRecoveryService>.Instance, new PasswordRecoveryQueue());
    }

    [TearDown]
    public void TearDown() => _userManager.Dispose();

    private void RegisterTokenProvider(TimeSpan lifetime)
    {
        var provider = new DataProtectorTokenProvider<ApplicationUser>(new EphemeralDataProtectionProvider(),
            Options.Create(new DataProtectionTokenProviderOptions { TokenLifespan = lifetime }),
            NullLogger<DataProtectorTokenProvider<ApplicationUser>>.Instance);
        _userManager.RegisterTokenProvider(TokenOptions.DefaultProvider, provider);
    }

    private async Task<string> GenerateToken()
        => WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(await _userManager.GeneratePasswordResetTokenAsync(_user)));

    [Test]
    public async Task UnknownEmailDoesNotSendMail()
    {
        await _service.SendResetEmailAsync("unknown@example.com", CancellationToken.None);
        _emailSender.Verify(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task EmailContainsAnEncodedUsableTokenAndUsesStoredRecipient()
    {
        string body = string.Empty;
        _emailSender.Setup(x => x.SendAsync(_user.Email!, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, string, CancellationToken>((_, _, html, _) => body = html)
            .Returns(Task.CompletedTask);
        await _service.SendResetEmailAsync(_user.Email!, CancellationToken.None);
        var link = System.Net.WebUtility.HtmlDecode(body.Split("href=\"")[1].Split('"')[0]);
        var query = QueryHelpers.ParseQuery(new Uri(link).Query);
        Assert.That(query["email"].ToString(), Is.EqualTo(_user.Email));
        var result = await _service.ResetPasswordAsync(query["email"].ToString(), query["token"].ToString(), "NewPassword123!");
        Assert.That(result.Succeeded, Is.True);
    }

    [Test]
    public async Task SuccessfulResetUpdatesPasswordAndInvalidatesTokenAndSecurityStamp()
    {
        var token = await GenerateToken();
        var otherToken = await GenerateToken();
        var oldStamp = _user.SecurityStamp;
        var result = await _service.ResetPasswordAsync(_user.Email!, token, "NewPassword123!");
        Assert.That(result.Succeeded, Is.True);
        Assert.That(await _userManager.CheckPasswordAsync(_user, "NewPassword123!"), Is.True);
        Assert.That(_user.SecurityStamp, Is.Not.EqualTo(oldStamp));
        Assert.That((await _service.ResetPasswordAsync(_user.Email!, token, "OtherPassword123!")).Succeeded, Is.False);
        Assert.That((await _service.ResetPasswordAsync(_user.Email!, otherToken, "OtherPassword123!")).Succeeded, Is.False);
    }

    [Test]
    public async Task ExpiredTokenCannotChangePassword()
    {
        RegisterTokenProvider(TimeSpan.FromSeconds(-1));
        var token = await GenerateToken();
        var result = await _service.ResetPasswordAsync(_user.Email!, token, "NewPassword123!");
        Assert.That(result.Succeeded, Is.False);
        Assert.That(_user.PasswordHash, Is.Null);
    }

    [TestCase("not-a-token!")]
    [TestCase("YWJj")]
    public async Task MalformedOrTamperedTokenCannotChangePassword(string token)
    {
        Assert.That((await _service.ResetPasswordAsync(_user.Email!, token, "NewPassword123!")).Succeeded, Is.False);
        Assert.That(_user.PasswordHash, Is.Null);
    }

    [Test]
    public async Task UnknownAccountAndInvalidTokenHaveIdenticalErrors()
    {
        var token = await GenerateToken();
        var unknown = await _service.ResetPasswordAsync("unknown@example.com", token, "NewPassword123!");
        var invalid = await _service.ResetPasswordAsync(_user.Email!, "YWJj", "NewPassword123!");
        Assert.That(unknown.Errors, Is.EqualTo(invalid.Errors));
    }

    [Test]
    public async Task WeakPasswordIsRejectedAndTokenCanBeRetried()
    {
        var token = await GenerateToken();
        var result = await _service.ResetPasswordAsync(_user.Email!, token, "weak");
        Assert.That(result.Succeeded, Is.False);
        Assert.That(_user.PasswordHash, Is.Null);
        Assert.That((await _service.ResetPasswordAsync(_user.Email!, token, "NewPassword123!")).Succeeded, Is.True);
    }

    [Test]
    public async Task DeliveryFailureDoesNotRevealAccountExistence()
    {
        _emailSender.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new SmtpException("Delivery failed"));
        await _service.SendResetEmailAsync(_user.Email!, CancellationToken.None);
        await _service.SendResetEmailAsync("unknown@example.com", CancellationToken.None);
        Assert.That(_user.PasswordHash, Is.Null);
    }

    [Test]
    public void MissingSmtpConfigurationFailsForAnyEmail()
    {
        _emailSender.Setup(x => x.ValidateConfiguration()).Throws(new PasswordRecoveryUnavailableException());
        Assert.ThrowsAsync<PasswordRecoveryUnavailableException>(() => _service.RequestResetAsync(_user.Email!, CancellationToken.None));
        Assert.ThrowsAsync<PasswordRecoveryUnavailableException>(() => _service.RequestResetAsync("unknown@example.com", CancellationToken.None));
    }

    [Test]
    public async Task PublicRequestQueuesKnownAndUnknownEmailsWithoutSendingSynchronously()
    {
        await _service.RequestResetAsync(_user.Email!, CancellationToken.None);
        await _service.RequestResetAsync("unknown@example.com", CancellationToken.None);
        _emailSender.Verify(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
