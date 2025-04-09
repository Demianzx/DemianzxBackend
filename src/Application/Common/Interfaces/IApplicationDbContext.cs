using DemianzxBackend.Domain.Entities;

namespace DemianzxBackend.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<BlogPost> BlogPosts { get; }
    DbSet<Category> Categories { get; }
    DbSet<Tag> Tags { get; }
    DbSet<Comment> Comments { get; }
    DbSet<PostCategory> PostCategories { get; }
    DbSet<PostTag> PostTags { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
