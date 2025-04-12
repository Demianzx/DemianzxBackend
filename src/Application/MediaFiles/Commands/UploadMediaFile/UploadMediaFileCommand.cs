using DemianzxBackend.Application.Common.Interfaces;
using DemianzxBackend.Application.Common.Security;

namespace DemianzxBackend.Application.MediaFiles.Commands.UploadMediaFile;

[Authorize]
public record UploadMediaFileCommand : IRequest<string>
{
    public Stream Content { get; init; } = null!;
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
}

public class UploadMediaFileCommandHandler : IRequestHandler<UploadMediaFileCommand, string>
{
    private readonly IBlobStorageService _blobStorageService;

    public UploadMediaFileCommandHandler(IBlobStorageService blobStorageService)
    {
        _blobStorageService = blobStorageService;
    }

    public async Task<string> Handle(UploadMediaFileCommand request, CancellationToken cancellationToken)
    {
        return await _blobStorageService.UploadAsync(
            request.Content,
            request.FileName,
            request.ContentType);
    }
}
