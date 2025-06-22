using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NET_API.Migrations
{
    /// <inheritdoc />
    public partial class AddTaiPowerTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "taipower_generation",
                columns: table => new
                {
                    time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    east_generation = table.Column<decimal>(type: "TEXT", nullable: false),
                    central_generation = table.Column<decimal>(type: "TEXT", nullable: false),
                    north_generation = table.Column<decimal>(type: "TEXT", nullable: false),
                    south_generation = table.Column<decimal>(type: "TEXT", nullable: false),
                    north_consumption = table.Column<decimal>(type: "TEXT", nullable: false),
                    south_consumption = table.Column<decimal>(type: "TEXT", nullable: false),
                    central_consumption = table.Column<decimal>(type: "TEXT", nullable: false),
                    east_consumption = table.Column<decimal>(type: "TEXT", nullable: false),
                    create_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_taipower_generation", x => x.time);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "taipower_generation");
        }
    }
}
