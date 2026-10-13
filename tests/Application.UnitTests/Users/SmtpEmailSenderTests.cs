using System.Net;
using System.Net.Sockets;
using DemianzxBackend.Application.Common.Exceptions;
using DemianzxBackend.Infrastructure.Email;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace DemianzxBackend.Application.UnitTests.Users;

public class SmtpEmailSenderTests
{
    private static SmtpEmailSender CreateSender(SmtpEmailOptions options, string environment)
    {
        var host = new Mock<IHostEnvironment>();
        host.SetupGet(x => x.EnvironmentName).Returns(environment);
        return new SmtpEmailSender(Options.Create(options), host.Object, NullLogger<SmtpEmailSender>.Instance);
    }

    [Test]
    public void MissingConfigurationIsRejected()
    {
        var sender = CreateSender(new SmtpEmailOptions(), Environments.Production);
        Assert.Throws<PasswordRecoveryUnavailableException>(() => sender.ValidateConfiguration());
    }

    [Test]
    public void UnencryptedSmtpIsRejectedInProduction()
    {
        var sender = CreateSender(new SmtpEmailOptions
        {
            Host = "smtp.example.com", FromAddress = "sender@example.com", EnableSsl = false
        }, Environments.Production);
        Assert.Throws<PasswordRecoveryUnavailableException>(() => sender.ValidateConfiguration());
    }

    [Test]
    public async Task SendsHtmlEmailThroughLocalSmtpServer()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var server = Task.Run(async () =>
        {
            using var connection = await listener.AcceptTcpClientAsync(timeout.Token);
            using var reader = new StreamReader(connection.GetStream());
            using var writer = new StreamWriter(connection.GetStream()) { AutoFlush = true, NewLine = "\r\n" };
            await writer.WriteLineAsync("220 localhost test SMTP");
            var envelope = new List<string>();
            var body = new List<string>();
            while (await reader.ReadLineAsync(timeout.Token) is { } line)
            {
                envelope.Add(line);
                if (line == "DATA")
                {
                    await writer.WriteLineAsync("354 End with a dot");
                    while (await reader.ReadLineAsync(timeout.Token) is { } data && data != ".")
                    {
                        body.Add(data);
                    }
                    await writer.WriteLineAsync("250 Accepted");
                    return (Envelope: envelope, Body: string.Join("\n", body));
                }
                await writer.WriteLineAsync("250 OK");
            }
            throw new InvalidOperationException("SMTP connection closed before DATA.");
        }, timeout.Token);

        var sender = CreateSender(new SmtpEmailOptions
        {
            Host = "127.0.0.1", Port = port, EnableSsl = false, FromAddress = "sender@example.com"
        }, Environments.Development);
        await sender.SendAsync("recipient@example.com", "Restablecer password", "<h1>Restablecer password</h1>", timeout.Token);
        var received = await server;
        Assert.That(received.Envelope, Has.Some.Contains("sender@example.com"));
        Assert.That(received.Envelope, Has.Some.Contains("recipient@example.com"));
        Assert.That(received.Body, Does.Contain("text/html"));
        Assert.That(received.Body, Does.Contain("Restablecer password"));
    }
}
