using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemianzxBackend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBlogPostEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ThumbnailImageUrl",
                table: "BlogPosts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ThumbnailImageUrl",
                table: "BlogPosts");
        }
    }
}
