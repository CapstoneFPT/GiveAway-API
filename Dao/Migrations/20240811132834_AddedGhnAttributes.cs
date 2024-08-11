using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class AddedGhnAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GhnDistrictId",
                table: "Shops",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GhnShopId",
                table: "Shops",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GhnWardCode",
                table: "Shops",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GhnDistrictId",
                table: "Order",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GhnWardCode",
                table: "Order",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GhnDistrictId",
                table: "Address",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GhnWardCode",
                table: "Address",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GhnDistrictId",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "GhnShopId",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "GhnWardCode",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "GhnDistrictId",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "GhnWardCode",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "GhnDistrictId",
                table: "Address");

            migrationBuilder.DropColumn(
                name: "GhnWardCode",
                table: "Address");
        }
    }
}
