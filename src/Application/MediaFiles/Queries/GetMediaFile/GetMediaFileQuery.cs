using DemianzxBackend.Application.Common.Interfaces;

namespace DemianzxBackend.Application.MediaFiles.Queries.GetMediaFile;

public record GetMediaFileQuery(string BlobName) : IRequest<Stream>;

public class GetMediaFileQueryHandler : IRequestHandler<GetMediaFileQuery, Stream>
{
    private readonly IBlobStorageService _blobStorageService;

    public GetMediaFileQueryHandler(IBlobStorageService blobStorageService)
    {
        _blobStorageService = blobStorageService;
    }

    public async Task<Stream> Handle(GetMediaFileQuery request, CancellationToken cancellationToken)
    {
        return await _blobStorageService.DownloadAsync(request.BlobName);
    }
}
