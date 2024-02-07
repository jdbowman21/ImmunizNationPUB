using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ImmunizNation.Client.Migrations
{
    public partial class AddResourceDownloads : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DownloadCount",
                table: "Resources");

            migrationBuilder.CreateTable(
                name: "ResourceDownloads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceDownloads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourceDownloads_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResourceDownloads_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResourceDownloads_ResourceId",
                table: "ResourceDownloads",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceDownloads_UserId",
                table: "ResourceDownloads",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResourceDownloads");

            migrationBuilder.AddColumn<int>(
                name: "DownloadCount",
                table: "Resources",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
