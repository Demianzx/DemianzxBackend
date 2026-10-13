using DemianzxBackend.Infrastructure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DemianzxBackend.Infrastructure.Email;

public class PasswordRecoveryWorker(
    PasswordRecoveryQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<PasswordRecoveryWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var email in queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var recovery = scope.ServiceProvider.GetRequiredService<PasswordRecoveryService>();
                await recovery.SendResetEmailAsync(email, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception)
            {
                // Never include account details, credentials or reset links in diagnostics.
                logger.LogError("Password recovery processing failed. Check database and email service availability.");
            }
        }
    }
}
