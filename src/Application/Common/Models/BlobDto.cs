namespace DemianzxBackend.Application.Common.Models;

public class BlobDto
{
    public string Name { get; set; } = string.Empty;
    public string Uri { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime Created { get; set; }
}
