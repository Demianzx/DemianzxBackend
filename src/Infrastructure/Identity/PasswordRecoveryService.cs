using System.Net;
using System.Net.Mail;
using System.Text;
using DemianzxBackend.Application.Common.Exceptions;
using DemianzxBackend.Application.Common.Interfaces;
using DemianzxBackend.Application.Common.Models;
using DemianzxBackend.Infrastructure.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DemianzxBackend.Infrastructure.Identity;

public class PasswordRecoveryService(
    UserManager<ApplicationUser> userManager,
    IEmailSender emailSender,
    IOptions<PasswordRecoveryOptions> options,
    IHostEnvironment environment,
    ILogger<PasswordRecoveryService> logger,
    PasswordRecoveryQueue queue) : IPasswordRecoveryService
{
    private const string InvalidLinkMessage = "El enlace de recuperación no es válido o expiró. Solicita uno nuevo.";

    public Task RequestResetAsync(string email, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // Check configuration before looking up an account, so a 503 cannot reveal its existence.
        emailSender.ValidateConfiguration();
        ValidateResetUrl();
        // Queue every valid request before account lookup. SMTP latency cannot expose registered emails.
        queue.Enqueue(email);
        return Task.CompletedTask;
    }

    private Uri ValidateResetUrl()
    {
        if (!Uri.TryCreate(options.Value.ResetUrl, UriKind.Absolute, out var resetUri)
            || !string.IsNullOrEmpty(resetUri.Query) || !string.IsNullOrEmpty(resetUri.Fragment)
            || !string.IsNullOrEmpty(resetUri.UserInfo)
            || (resetUri.Scheme != Uri.UriSchemeHttps
                && !(environment.IsDevelopment() && resetUri.Scheme == Uri.UriSchemeHttp && resetUri.IsLoopback)))
        {
            logger.LogError("PasswordRecovery:ResetUrl must be an absolute HTTPS URL without query parameters or fragment (loopback HTTP is allowed in Development).");
            throw new PasswordRecoveryUnavailableException();
        }
        return resetUri;
    }

    public async Task SendResetEmailAsync(string email, CancellationToken cancellationToken)
    {
        var resetUri = ValidateResetUrl();

        var user = await userManager.FindByEmailAsync(email);
        if (user == null || !await userManager.HasPasswordAsync(user))
        {
            return;
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        // The destination and recipient come from trusted configuration and the stored account.
        var resetLink = QueryHelpers.AddQueryString(resetUri.AbsoluteUri, new Dictionary<string, string?>
        {
            ["email"] = user.Email!,
            ["token"] = encodedToken
        });
        var safeLink = WebUtility.HtmlEncode(resetLink);
        var body = $"""
            <h1>Restablecer tu contraseña</h1>
            <p>Recibimos una solicitud para restablecer la contraseña de tu cuenta en DemianzxGames.</p>
            <p><a href="{safeLink}">Restablecer contraseña</a></p>
            <p>Este enlace vence en una hora y solo puede usarse una vez.</p>
            <p>Si no solicitaste este cambio, puedes ignorar este correo. Tu contraseña seguirá igual.</p>
            """;

        try
        {
            await emailSender.SendAsync(user.Email!, "Restablece tu contraseña de DemianzxGames", body, cancellationToken);
        }
        catch (Exception ex) when (ex is SmtpException || ex is OperationCanceledException && !cancellationToken.IsCancellationRequested)
        {
            // Keep the public response identical for existing and unknown accounts, even on delivery failure.
            // Do not log the exception, recipient, body or token: SMTP diagnostics may include personal data.
            logger.LogError("Password recovery email delivery failed. Check SMTP connectivity and credentials.");
        }
    }

    public async Task<Result> ResetPasswordAsync(string email, string token, string newPassword)
    {
        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        }
        catch (FormatException)
        {
            return Result.Failure(new[] { InvalidLinkMessage });
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user == null || !await userManager.HasPasswordAsync(user))
        {
            return Result.Failure(new[] { InvalidLinkMessage });
        }

        var result = await userManager.ResetPasswordAsync(user, decodedToken, newPassword);
        if (result.Succeeded)
        {
            return Result.Success();
        }

        // Identity changes the security stamp on success, invalidating previously issued reset tokens.
        return result.Errors.Any(error => error.Code == "InvalidToken")
            ? Result.Failure(new[] { InvalidLinkMessage })
            : result.ToApplicationResult();
    }
}
