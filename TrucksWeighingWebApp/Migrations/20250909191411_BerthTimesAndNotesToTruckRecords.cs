using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrucksWeighingWebApp.Migrations
{
    /// <inheritdoc />
    public partial class BerthTimesAndNotesToTruckRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BerthNote",
                table: "TruckRecords",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FinalBerthAtUtc",
                table: "TruckRecords",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InitialBerthAtUtc",
                table: "TruckRecords",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BerthNote",
                table: "TruckRecords");

            migrationBuilder.DropColumn(
                name: "FinalBerthAtUtc",
                table: "TruckRecords");

            migrationBuilder.DropColumn(
                name: "InitialBerthAtUtc",
                table: "TruckRecords");
        }
    }
}
