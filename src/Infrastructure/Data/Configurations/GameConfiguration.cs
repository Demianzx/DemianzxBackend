using DemianzxBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DemianzxBackend.Infrastructure.Data.Configurations;

public class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.Property(g => g.EmbedUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(g => g.Instructions)
            .HasMaxLength(2000);

        builder.Property(g => g.AspectRatio)
            .HasMaxLength(10);

        // BlogPost relation: one-to-one (a post has at most one game)
        builder.HasOne(g => g.Post)
            .WithOne(p => p.Game)
            .HasForeignKey<Game>(g => g.BlogPostId)
            .OnDelete(DeleteBehavior.Cascade);

        // Enforce one-to-one at database level
        builder.HasIndex(g => g.BlogPostId)
            .IsUnique();
    }
}