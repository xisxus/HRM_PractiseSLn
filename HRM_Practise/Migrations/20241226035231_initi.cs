using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRM_Practise.Migrations
{
    /// <inheritdoc />
    public partial class initi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HRM_ATD_MachineData",
                columns: table => new
                {
                    AutoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FingerPrintId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MachineId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Latitude = table.Column<string>(type: "nvarchar(max)", maxLength: 2147483647, nullable: false),
                    Longitude = table.Column<string>(type: "nvarchar(max)", maxLength: 2147483647, nullable: false),
                    HOALR = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HRM_ATD_MachineData", x => x.AutoId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HRM_ATD_MachineData");
        }
    }
}
