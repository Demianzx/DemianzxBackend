using DemianzxBackend.Application.Common.Models;

namespace DemianzxBackend.Application.Common.Interfaces;

public interface IBlobStorageService
{
    Task<string> UploadAsync(Stream content, string fileName, string contentType);
    Task<bool> DeleteAsync(string blobName);
    Task<Stream> DownloadAsync(string blobName);
    Task<List<BlobDto>> ListAsync(string? prefix = null);
}
