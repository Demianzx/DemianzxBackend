using DemianzxBackend.Application.Common.Interfaces;
using System.Text.RegularExpressions;

namespace DemianzxBackend.Infrastructure.Common;

public class SlugService : ISlugService
{
    public string GenerateSlug(string text)
    {
       
        var slug = text.ToLowerInvariant();

        slug = Regex.Replace(slug, @"[áàäâã]", "a");
        slug = Regex.Replace(slug, @"[éèëê]", "e");
        slug = Regex.Replace(slug, @"[íìïî]", "i");
        slug = Regex.Replace(slug, @"[óòöôõ]", "o");
        slug = Regex.Replace(slug, @"[úùüû]", "u");
        slug = Regex.Replace(slug, @"[ñ]", "n");

        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");

        slug = Regex.Replace(slug, @"\s+", "-");

        slug = Regex.Replace(slug, @"-+", "-");

        slug = slug.Trim('-');

        return slug;
    }
}
