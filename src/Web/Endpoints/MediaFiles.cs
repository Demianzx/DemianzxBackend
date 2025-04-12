using DemianzxBackend.Application.Common.Models;
using DemianzxBackend.Application.MediaFiles.Commands.DeleteMediaFile;
using DemianzxBackend.Application.MediaFiles.Commands.UploadMediaFile;
using DemianzxBackend.Application.MediaFiles.Queries.GetMediaFile;
using DemianzxBackend.Application.MediaFiles.Queries.GetMediaFiles;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace DemianzxBackend.Web.Endpoints;

public class MediaFiles : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetMediaFiles)
            .MapGet(GetMediaFile, "{blobName}")
            .MapPost(UploadMediaFile)
            .MapDelete(DeleteMediaFile, "{blobName}");
    }

    public async Task<Ok<List<BlobDto>>> GetMediaFiles(ISender sender, [AsParameters] GetMediaFilesQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    public async Task<Results<FileStreamHttpResult, NotFound>> GetMediaFile(ISender sender, string blobName)
    {
        try
        {
            var stream = await sender.Send(new GetMediaFileQuery(blobName));
            return TypedResults.File(stream, contentType: GetContentType(blobName));
        }
        catch
        {
            return TypedResults.NotFound();
        }
    }

    public async Task<Ok<string>> UploadMediaFile(ISender sender, IFormFile file)
    {
        var command = new UploadMediaFileCommand
        {
            Content = file.OpenReadStream(),
            FileName = file.FileName,
            ContentType = file.ContentType
        };

        var result = await sender.Send(command);
        return TypedResults.Ok(result);
    }

    public async Task<Results<NoContent, NotFound>> DeleteMediaFile(ISender sender, string blobName)
    {
        var result = await sender.Send(new DeleteMediaFileCommand(blobName));

        if (result)
            return TypedResults.NoContent();

        return TypedResults.NotFound();
    }

    private string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            ".svg" => "image/svg+xml",
            _ => "application/octet-stream"
        };
    }
}
