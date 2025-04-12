using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DemianzxBackend.Application.Common.Interfaces;
using DemianzxBackend.Application.Common.Models;
using Microsoft.Extensions.Configuration;

namespace DemianzxBackend.Infrastructure.Blob;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public BlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AzureBlobStorage");
        Guard.Against.NullOrEmpty(connectionString, nameof(connectionString), "Connection string 'AzureBlobStorage' not found.");

        _containerName = configuration["BlobStorage:ContainerName"] ?? "media";
        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    private async Task<BlobContainerClient> GetContainerClientAsync()
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
        return containerClient;
    }

    public async Task<string> UploadAsync(Stream content, string fileName, string contentType)
    {
        var containerClient = await GetContainerClientAsync();
        var blobName = $"{Guid.NewGuid()}-{fileName}";
        var blobClient = containerClient.GetBlobClient(blobName);

        var blobHttpHeaders = new BlobHttpHeaders
        {
            ContentType = contentType
        };

        await blobClient.UploadAsync(content, new BlobUploadOptions { HttpHeaders = blobHttpHeaders });
        return blobClient.Uri.ToString();
    }

    public async Task<bool> DeleteAsync(string blobName)
    {
        var containerClient = await GetContainerClientAsync();
        var blobClient = containerClient.GetBlobClient(blobName);
        return await blobClient.DeleteIfExistsAsync();
    }

    public async Task<Stream> DownloadAsync(string blobName)
    {
        var containerClient = await GetContainerClientAsync();
        var blobClient = containerClient.GetBlobClient(blobName);
        var download = await blobClient.DownloadAsync();
        return download.Value.Content;
    }

    public async Task<List<BlobDto>> ListAsync(string? prefix = null)
    {
        var containerClient = await GetContainerClientAsync();
        var blobs = new List<BlobDto>();

        await foreach (var blobItem in containerClient.GetBlobsAsync(prefix: prefix))
        {
            var blobClient = containerClient.GetBlobClient(blobItem.Name);
            var properties = await blobClient.GetPropertiesAsync();

            blobs.Add(new BlobDto
            {
                Name = blobItem.Name,
                Uri = blobClient.Uri.ToString(),
                ContentType = properties.Value.ContentType,
                Size = blobItem.Properties.ContentLength ?? 0,
                Created = blobItem.Properties.CreatedOn?.DateTime ?? DateTime.MinValue
            });
        }

        return blobs;
    }
}
