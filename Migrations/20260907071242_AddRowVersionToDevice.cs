using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetTelemetryPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddRowVersionToDevice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Devices",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Devices");
        }
    }
}
