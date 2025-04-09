using DemianzxBackend.Application.Common.Interfaces;
using System.Text.RegularExpressions;

namespace DemianzxBackend.Infrastructure.Common;

public class SlugService : ISlugService
{
    public string GenerateSlug(string text)
    {
       
        var slug = text.ToLowerInvariant();

        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");

        slug = Regex.Replace(slug, @"\s+", "-");

        slug = Regex.Replace(slug, @"-+", "-");

        slug = slug.Trim('-');

        return slug;
    }
}
