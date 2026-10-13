using System.Net;
using System.Net.Mail;
using System.Text;
using DemianzxBackend.Application.Common.Exceptions;
using DemianzxBackend.Application.Common.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DemianzxBackend.Infrastructure.Email;

public class SmtpEmailSender(
    IOptions<SmtpEmailOptions> options,
    IHostEnvironment environment,
    ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public void ValidateConfiguration()
    {
        var smtp = options.Value;
        if (string.IsNullOrWhiteSpace(smtp.Host) || smtp.Port is < 1 or > 65535
            || !MailAddress.TryCreate(smtp.FromAddress, out _)
            || (!smtp.EnableSsl && !environment.IsDevelopment())
            || (string.IsNullOrWhiteSpace(smtp.UserName) != string.IsNullOrWhiteSpace(smtp.Password)))
        {
            logger.LogError("Password recovery requires a valid SMTP host, port, sender and credentials. TLS is required outside Development.");
            throw new PasswordRecoveryUnavailableException();
        }
    }

    public async Task SendAsync(string recipient, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        ValidateConfiguration();
        var smtp = options.Value;
        using var message = new MailMessage
        {
            From = new MailAddress(smtp.FromAddress, smtp.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true,
            BodyEncoding = Encoding.UTF8,
            SubjectEncoding = Encoding.UTF8
        };
        message.To.Add(new MailAddress(recipient));

        using var client = new SmtpClient(smtp.Host, smtp.Port)
        {
            EnableSsl = smtp.EnableSsl,
            UseDefaultCredentials = false,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };
        if (!string.IsNullOrWhiteSpace(smtp.UserName))
        {
            client.Credentials = new NetworkCredential(smtp.UserName, smtp.Password);
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(30));
        await client.SendMailAsync(message, timeout.Token);
    }
}
