using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebCrawler.Data.Migrations
{
    /// <inheritdoc />
    public partial class addJobId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "JobId",
                table: "WebsiteRecord",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JobId",
                table: "WebsiteRecord");
        }
    }
}
