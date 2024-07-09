using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRelationshipForOfflinePurchaseAndConsign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Account_MemberId",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetail_ConsignSale_ConsignSaleId",
                table: "OrderDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Account_MemberId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetail_ConsignSaleId",
                table: "OrderDetail");

            migrationBuilder.DropColumn(
                name: "TransactionNumber",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "ConsignSaleId",
                table: "OrderDetail");

            migrationBuilder.AlterColumn<Guid>(
                name: "MemberId",
                table: "Transaction",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "ConsignSaleId",
                table: "Transaction",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VnPayTransactionNumber",
                table: "Transaction",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefundExpirationDate",
                table: "OrderDetail",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "MemberId",
                table: "Order",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "PurchaseType",
                table: "Order",
                type: "varchar",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_ConsignSaleId",
                table: "Transaction",
                column: "ConsignSaleId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Account_MemberId",
                table: "Order",
                column: "MemberId",
                principalTable: "Account",
                principalColumn: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Account_MemberId",
                table: "Transaction",
                column: "MemberId",
                principalTable: "Account",
                principalColumn: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_ConsignSale_ConsignSaleId",
                table: "Transaction",
                column: "ConsignSaleId",
                principalTable: "ConsignSale",
                principalColumn: "ConsignSaleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Account_MemberId",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Account_MemberId",
                table: "Transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_ConsignSale_ConsignSaleId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_ConsignSaleId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "ConsignSaleId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "VnPayTransactionNumber",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "RefundExpirationDate",
                table: "OrderDetail");

            migrationBuilder.DropColumn(
                name: "PurchaseType",
                table: "Order");

            migrationBuilder.AlterColumn<Guid>(
                name: "MemberId",
                table: "Transaction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransactionNumber",
                table: "Transaction",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ConsignSaleId",
                table: "OrderDetail",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "MemberId",
                table: "Order",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetail_ConsignSaleId",
                table: "OrderDetail",
                column: "ConsignSaleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Account_MemberId",
                table: "Order",
                column: "MemberId",
                principalTable: "Account",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetail_ConsignSale_ConsignSaleId",
                table: "OrderDetail",
                column: "ConsignSaleId",
                principalTable: "ConsignSale",
                principalColumn: "ConsignSaleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Account_MemberId",
                table: "Transaction",
                column: "MemberId",
                principalTable: "Account",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
