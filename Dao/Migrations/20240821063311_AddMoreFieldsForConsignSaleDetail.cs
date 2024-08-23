using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreFieldsForConsignSaleDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "ConsignSaleDetail",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "ConsignSaleDetail",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Condition",
                table: "ConsignSaleDetail",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "ConsignSaleDetail",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Size",
                table: "ConsignSaleDetail",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Brand",
                table: "ConsignSaleDetail");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "ConsignSaleDetail");

            migrationBuilder.DropColumn(
                name: "Condition",
                table: "ConsignSaleDetail");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "ConsignSaleDetail");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "ConsignSaleDetail");
        }
    }
}
