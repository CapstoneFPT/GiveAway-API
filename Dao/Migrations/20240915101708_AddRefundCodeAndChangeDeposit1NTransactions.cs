using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class AddRefundCodeAndChangeDeposit1NTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuctionDeposit_Transaction_TransactionId",
                table: "AuctionDeposit");

            migrationBuilder.DropIndex(
                name: "IX_AuctionDeposit_TransactionId",
                table: "AuctionDeposit");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "AuctionDeposit");

            migrationBuilder.AddColumn<Guid>(
                name: "AuctionDepositId",
                table: "Transaction",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefundCode",
                table: "Refund",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_AuctionDepositId",
                table: "Transaction",
                column: "AuctionDepositId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_AuctionDeposit_AuctionDepositId",
                table: "Transaction",
                column: "AuctionDepositId",
                principalTable: "AuctionDeposit",
                principalColumn: "AuctionDepositId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_AuctionDeposit_AuctionDepositId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_AuctionDepositId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "AuctionDepositId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "RefundCode",
                table: "Refund");

            migrationBuilder.AddColumn<Guid>(
                name: "TransactionId",
                table: "AuctionDeposit",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_AuctionDeposit_TransactionId",
                table: "AuctionDeposit",
                column: "TransactionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AuctionDeposit_Transaction_TransactionId",
                table: "AuctionDeposit",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "TransactionId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
