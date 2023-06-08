using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebCrawler.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tag",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tag", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WebsiteRecord",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Periodicity = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    Label = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebsiteRecord", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CrawlInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JobId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    EntryUrl = table.Column<string>(type: "TEXT", nullable: false),
                    RegexPattern = table.Column<string>(type: "TEXT", nullable: false, defaultValue: ".*"),
                    Periodicity = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    WebsiteRecordId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrawlInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrawlInfos_WebsiteRecord_WebsiteRecordId",
                        column: x => x.WebsiteRecordId,
                        principalTable: "WebsiteRecord",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TagWebsiteRecord",
                columns: table => new
                {
                    TagsId = table.Column<int>(type: "INTEGER", nullable: false),
                    WebsiteRecordsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagWebsiteRecord", x => new { x.TagsId, x.WebsiteRecordsId });
                    table.ForeignKey(
                        name: "FK_TagWebsiteRecord_Tag_TagsId",
                        column: x => x.TagsId,
                        principalTable: "Tag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TagWebsiteRecord_WebsiteRecord_WebsiteRecordsId",
                        column: x => x.WebsiteRecordsId,
                        principalTable: "WebsiteRecord",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Executions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Started = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Finished = table.Column<DateTime>(type: "TEXT", nullable: true),
                    WebsiteGraph = table.Column<string>(type: "TEXT", nullable: true),
                    CrawlInfoId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Executions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Executions_CrawlInfos_CrawlInfoId",
                        column: x => x.CrawlInfoId,
                        principalTable: "CrawlInfos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CrawlInfos_WebsiteRecordId",
                table: "CrawlInfos",
                column: "WebsiteRecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Executions_CrawlInfoId",
                table: "Executions",
                column: "CrawlInfoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TagWebsiteRecord_WebsiteRecordsId",
                table: "TagWebsiteRecord",
                column: "WebsiteRecordsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Executions");

            migrationBuilder.DropTable(
                name: "TagWebsiteRecord");

            migrationBuilder.DropTable(
                name: "CrawlInfos");

            migrationBuilder.DropTable(
                name: "Tag");

            migrationBuilder.DropTable(
                name: "WebsiteRecord");
        }
    }
}
