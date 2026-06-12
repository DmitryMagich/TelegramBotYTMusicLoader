using System;
using Microsoft.EntityFrameworkCore.Migrations;
using TelegramBotYtMusic.Database;

#nullable disable

namespace TelegramBotYtMusic.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrackCaches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SearchQuery = table.Column<string>(type: "text", nullable: false),
                    TelegramFileId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackCaches", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrackCaches_SearchQuery",
                table: "TrackCaches",
                column: "SearchQuery");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrackCaches");
        }
    }
}
