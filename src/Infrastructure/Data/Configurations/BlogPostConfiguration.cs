using DemianzxBackend.Domain.Entities;
using DemianzxBackend.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DemianzxBackend.Infrastructure.Data.Configurations;

public class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
{
    public void Configure(EntityTypeBuilder<BlogPost> builder)
    {
        builder.Property(b => b.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(b => b.Slug)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(b => b.Slug)
            .IsUnique();

        builder.Property(b => b.Content)
            .IsRequired();

        builder.Property(b => b.AuthorId)
            .IsRequired();

        // Relation with ApplicationUser
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(b => b.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
