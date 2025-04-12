using DemianzxBackend.Application.Common.Interfaces;
using DemianzxBackend.Application.Common.Security;

namespace DemianzxBackend.Application.MediaFiles.Commands.DeleteMediaFile;

[Authorize]
public record DeleteMediaFileCommand(string BlobName) : IRequest<bool>;

public class DeleteMediaFileCommandHandler : IRequestHandler<DeleteMediaFileCommand, bool>
{
    private readonly IBlobStorageService _blobStorageService;

    public DeleteMediaFileCommandHandler(IBlobStorageService blobStorageService)
    {
        _blobStorageService = blobStorageService;
    }

    public async Task<bool> Handle(DeleteMediaFileCommand request, CancellationToken cancellationToken)
    {
        return await _blobStorageService.DeleteAsync(request.BlobName);
    }
}
