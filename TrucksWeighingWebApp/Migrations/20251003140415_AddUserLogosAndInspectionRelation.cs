using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TrucksWeighingWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddUserLogosAndInspectionRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserLogoId",
                table: "Inspections",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserLogos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApplicationUserId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Height = table.Column<int>(type: "integer", nullable: false),
                    PaddingBottom = table.Column<int>(type: "integer", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    FilePath = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ContentType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLogos_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_UserLogoId",
                table: "Inspections",
                column: "UserLogoId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogos_ApplicationUserId_Name",
                table: "UserLogos",
                columns: new[] { "ApplicationUserId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Inspections_UserLogos_UserLogoId",
                table: "Inspections",
                column: "UserLogoId",
                principalTable: "UserLogos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inspections_UserLogos_UserLogoId",
                table: "Inspections");

            migrationBuilder.DropTable(
                name: "UserLogos");

            migrationBuilder.DropIndex(
                name: "IX_Inspections_UserLogoId",
                table: "Inspections");

            migrationBuilder.DropColumn(
                name: "UserLogoId",
                table: "Inspections");
        }
    }
}
