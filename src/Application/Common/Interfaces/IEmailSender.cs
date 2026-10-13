namespace DemianzxBackend.Application.Common.Interfaces;

public interface IEmailSender
{
    void ValidateConfiguration();
    Task SendAsync(string recipient, string subject, string htmlBody, CancellationToken cancellationToken);
}
