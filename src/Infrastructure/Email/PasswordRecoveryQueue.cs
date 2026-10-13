using System.Threading.Channels;
using DemianzxBackend.Application.Common.Exceptions;

namespace DemianzxBackend.Infrastructure.Email;

public class PasswordRecoveryQueue
{
    private readonly Channel<string> _requests = Channel.CreateBounded<string>(new BoundedChannelOptions(100)
    {
        SingleReader = true,
        FullMode = BoundedChannelFullMode.Wait
    });

    public void Enqueue(string email)
    {
        if (!_requests.Writer.TryWrite(email))
        {
            throw new PasswordRecoveryUnavailableException();
        }
    }

    public IAsyncEnumerable<string> ReadAllAsync(CancellationToken cancellationToken)
        => _requests.Reader.ReadAllAsync(cancellationToken);
}
