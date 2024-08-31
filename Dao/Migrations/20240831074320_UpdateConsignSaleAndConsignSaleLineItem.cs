using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class UpdateConsignSaleAndConsignSaleLineItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RetailPrice",
                table: "IndividualFashionItems");

            migrationBuilder.AlterColumn<decimal>(
                name: "DealPrice",
                table: "ConsignSaleLineItem",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<decimal>(
                name: "ExpectedPrice",
                table: "ConsignSaleLineItem",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ResponseFromShop",
                table: "ConsignSaleLineItem",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ConsignSaleLineItem",
                type: "varchar",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResponseFromShop",
                table: "ConsignSale",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpectedPrice",
                table: "ConsignSaleLineItem");

            migrationBuilder.DropColumn(
                name: "ResponseFromShop",
                table: "ConsignSaleLineItem");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ConsignSaleLineItem");

            migrationBuilder.DropColumn(
                name: "ResponseFromShop",
                table: "ConsignSale");

            migrationBuilder.AddColumn<decimal>(
                name: "RetailPrice",
                table: "IndividualFashionItems",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "DealPrice",
                table: "ConsignSaleLineItem",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);
        }
    }
}
