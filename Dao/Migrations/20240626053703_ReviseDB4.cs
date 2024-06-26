using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class ReviseDB4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "OrderDetail");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "FashionItem");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Timeslot",
                type: "varchar",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "BidId",
                table: "Order",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrderCode",
                table: "Order",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "FashionItem",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "FashionItem",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "FashionItem",
                type: "varchar",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "FashionItem",
                type: "varchar",
                maxLength: 5,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConsignSaleCode",
                table: "ConsignSale",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Order_BidId",
                table: "Order",
                column: "BidId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Order_OrderCode",
                table: "Order",
                column: "OrderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConsignSale_ConsignSaleCode",
                table: "ConsignSale",
                column: "ConsignSaleCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Bid_BidId",
                table: "Order",
                column: "BidId",
                principalTable: "Bid",
                principalColumn: "BidId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Bid_BidId",
                table: "Order");

            migrationBuilder.DropIndex(
                name: "IX_Order_BidId",
                table: "Order");

            migrationBuilder.DropIndex(
                name: "IX_Order_OrderCode",
                table: "Order");

            migrationBuilder.DropIndex(
                name: "IX_ConsignSale_ConsignSaleCode",
                table: "ConsignSale");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Timeslot");

            migrationBuilder.DropColumn(
                name: "BidId",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "OrderCode",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "Brand",
                table: "FashionItem");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "FashionItem");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "FashionItem");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "FashionItem");

            migrationBuilder.DropColumn(
                name: "ConsignSaleCode",
                table: "ConsignSale");

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "OrderDetail",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "FashionItem",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
