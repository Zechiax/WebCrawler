using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebCrawler.Data.Migrations
{
    /// <inheritdoc />
    public partial class datadivision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CrawlInfos_WebsiteRecord_WebsiteRecordId",
                table: "CrawlInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_Executions_CrawlInfos_CrawlInfoId",
                table: "Executions");

            migrationBuilder.DropForeignKey(
                name: "FK_TagWebsiteRecord_WebsiteRecord_WebsiteRecordsId",
                table: "TagWebsiteRecord");

            migrationBuilder.DropColumn(
                name: "WebsiteGraph",
                table: "Executions");

            migrationBuilder.RenameColumn(
                name: "WebsiteRecordsId",
                table: "TagWebsiteRecord",
                newName: "WebsiteRecordsDataId");

            migrationBuilder.RenameIndex(
                name: "IX_TagWebsiteRecord_WebsiteRecordsId",
                table: "TagWebsiteRecord",
                newName: "IX_TagWebsiteRecord_WebsiteRecordsDataId");

            migrationBuilder.RenameColumn(
                name: "CrawlInfoId",
                table: "Executions",
                newName: "CrawlInfoDataId");

            migrationBuilder.RenameIndex(
                name: "IX_Executions_CrawlInfoId",
                table: "Executions",
                newName: "IX_Executions_CrawlInfoDataId");

            migrationBuilder.RenameColumn(
                name: "WebsiteRecordId",
                table: "CrawlInfos",
                newName: "WebsiteRecordDataId");

            migrationBuilder.RenameIndex(
                name: "IX_CrawlInfos_WebsiteRecordId",
                table: "CrawlInfos",
                newName: "IX_CrawlInfos_WebsiteRecordDataId");

            migrationBuilder.AddColumn<string>(
                name: "WebsiteGraphSnapshotJson",
                table: "Executions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_CrawlInfos_WebsiteRecord_WebsiteRecordDataId",
                table: "CrawlInfos",
                column: "WebsiteRecordDataId",
                principalTable: "WebsiteRecord",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Executions_CrawlInfos_CrawlInfoDataId",
                table: "Executions",
                column: "CrawlInfoDataId",
                principalTable: "CrawlInfos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TagWebsiteRecord_WebsiteRecord_WebsiteRecordsDataId",
                table: "TagWebsiteRecord",
                column: "WebsiteRecordsDataId",
                principalTable: "WebsiteRecord",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CrawlInfos_WebsiteRecord_WebsiteRecordDataId",
                table: "CrawlInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_Executions_CrawlInfos_CrawlInfoDataId",
                table: "Executions");

            migrationBuilder.DropForeignKey(
                name: "FK_TagWebsiteRecord_WebsiteRecord_WebsiteRecordsDataId",
                table: "TagWebsiteRecord");

            migrationBuilder.DropColumn(
                name: "WebsiteGraphSnapshotJson",
                table: "Executions");

            migrationBuilder.RenameColumn(
                name: "WebsiteRecordsDataId",
                table: "TagWebsiteRecord",
                newName: "WebsiteRecordsId");

            migrationBuilder.RenameIndex(
                name: "IX_TagWebsiteRecord_WebsiteRecordsDataId",
                table: "TagWebsiteRecord",
                newName: "IX_TagWebsiteRecord_WebsiteRecordsId");

            migrationBuilder.RenameColumn(
                name: "CrawlInfoDataId",
                table: "Executions",
                newName: "CrawlInfoId");

            migrationBuilder.RenameIndex(
                name: "IX_Executions_CrawlInfoDataId",
                table: "Executions",
                newName: "IX_Executions_CrawlInfoId");

            migrationBuilder.RenameColumn(
                name: "WebsiteRecordDataId",
                table: "CrawlInfos",
                newName: "WebsiteRecordId");

            migrationBuilder.RenameIndex(
                name: "IX_CrawlInfos_WebsiteRecordDataId",
                table: "CrawlInfos",
                newName: "IX_CrawlInfos_WebsiteRecordId");

            migrationBuilder.AddColumn<string>(
                name: "WebsiteGraph",
                table: "Executions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CrawlInfos_WebsiteRecord_WebsiteRecordId",
                table: "CrawlInfos",
                column: "WebsiteRecordId",
                principalTable: "WebsiteRecord",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Executions_CrawlInfos_CrawlInfoId",
                table: "Executions",
                column: "CrawlInfoId",
                principalTable: "CrawlInfos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TagWebsiteRecord_WebsiteRecord_WebsiteRecordsId",
                table: "TagWebsiteRecord",
                column: "WebsiteRecordsId",
                principalTable: "WebsiteRecord",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
