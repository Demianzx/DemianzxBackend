using DemianzxBackend.Application.Common.Interfaces;
using DemianzxBackend.Application.Common.Models;

namespace DemianzxBackend.Application.MediaFiles.Queries.GetMediaFiles;

public record GetMediaFilesQuery(string? Prefix = null) : IRequest<List<BlobDto>>;

public class GetMediaFilesQueryHandler : IRequestHandler<GetMediaFilesQuery, List<BlobDto>>
{
    private readonly IBlobStorageService _blobStorageService;

    public GetMediaFilesQueryHandler(IBlobStorageService blobStorageService)
    {
        _blobStorageService = blobStorageService;
    }

    public async Task<List<BlobDto>> Handle(GetMediaFilesQuery request, CancellationToken cancellationToken)
    {
        return await _blobStorageService.ListAsync(request.Prefix);
    }
}
