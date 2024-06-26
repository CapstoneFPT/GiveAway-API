using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class AdjustRedundantColumnInOrderDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetail_ConsignSale_RequestId",
                table: "OrderDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetail_FashionItem_FashionItemItemId",
                table: "OrderDetail");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetail_FashionItemItemId",
                table: "OrderDetail");

            migrationBuilder.DropColumn(
                name: "FashionItemItemId",
                table: "OrderDetail");

            migrationBuilder.RenameColumn(
                name: "RequestId",
                table: "OrderDetail",
                newName: "FashionItemId");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "OrderDetail",
                newName: "ConsignSaleId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderDetail_RequestId",
                table: "OrderDetail",
                newName: "IX_OrderDetail_FashionItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetail_ConsignSaleId",
                table: "OrderDetail",
                column: "ConsignSaleId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetail_ConsignSale_ConsignSaleId",
                table: "OrderDetail",
                column: "ConsignSaleId",
                principalTable: "ConsignSale",
                principalColumn: "ConsignSaleId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetail_FashionItem_FashionItemId",
                table: "OrderDetail",
                column: "FashionItemId",
                principalTable: "FashionItem",
                principalColumn: "ItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetail_ConsignSale_ConsignSaleId",
                table: "OrderDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetail_FashionItem_FashionItemId",
                table: "OrderDetail");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetail_ConsignSaleId",
                table: "OrderDetail");

            migrationBuilder.RenameColumn(
                name: "FashionItemId",
                table: "OrderDetail",
                newName: "RequestId");

            migrationBuilder.RenameColumn(
                name: "ConsignSaleId",
                table: "OrderDetail",
                newName: "ItemId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderDetail_FashionItemId",
                table: "OrderDetail",
                newName: "IX_OrderDetail_RequestId");

            migrationBuilder.AddColumn<Guid>(
                name: "FashionItemItemId",
                table: "OrderDetail",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetail_FashionItemItemId",
                table: "OrderDetail",
                column: "FashionItemItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetail_ConsignSale_RequestId",
                table: "OrderDetail",
                column: "RequestId",
                principalTable: "ConsignSale",
                principalColumn: "ConsignSaleId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetail_FashionItem_FashionItemItemId",
                table: "OrderDetail",
                column: "FashionItemItemId",
                principalTable: "FashionItem",
                principalColumn: "ItemId");
        }
    }
}
