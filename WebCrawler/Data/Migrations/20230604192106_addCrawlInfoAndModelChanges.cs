using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebCrawler.Data.Migrations
{
    /// <inheritdoc />
    public partial class addCrawlInfoAndModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdjacencyListJson",
                table: "Executions");

            migrationBuilder.AddColumn<int>(
                name: "CrawlInfoId",
                table: "Executions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CrawlInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EntryUrl = table.Column<string>(type: "TEXT", nullable: false),
                    RegexPattern = table.Column<string>(type: "TEXT", nullable: false, defaultValue: ".*"),
                    Periodicity = table.Column<TimeSpan>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrawlInfo", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Executions_CrawlInfoId",
                table: "Executions",
                column: "CrawlInfoId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Executions_CrawlInfo_CrawlInfoId",
                table: "Executions",
                column: "CrawlInfoId",
                principalTable: "CrawlInfo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Executions_CrawlInfo_CrawlInfoId",
                table: "Executions");

            migrationBuilder.DropTable(
                name: "CrawlInfo");

            migrationBuilder.DropIndex(
                name: "IX_Executions_CrawlInfoId",
                table: "Executions");

            migrationBuilder.DropColumn(
                name: "CrawlInfoId",
                table: "Executions");

            migrationBuilder.AddColumn<string>(
                name: "AdjacencyListJson",
                table: "Executions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
