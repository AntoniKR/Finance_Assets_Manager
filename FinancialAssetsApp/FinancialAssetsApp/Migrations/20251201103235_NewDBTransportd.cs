using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancialAssetsApp.Migrations
{
    /// <inheritdoc />
    public partial class NewDBTransportd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TypeEstate",
                table: "Transports",
                newName: "TypeTransport");

            migrationBuilder.AlterColumn<decimal>(
                name: "YearOfTransport",
                table: "Transports",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TypeTransport",
                table: "Transports",
                newName: "TypeEstate");

            migrationBuilder.AlterColumn<decimal>(
                name: "YearOfTransport",
                table: "Transports",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);
        }
    }
}
