using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class AddWithdrawAndTransactionRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "WithdrawId",
                table: "Transaction",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_WithdrawId",
                table: "Transaction",
                column: "WithdrawId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Withdraws_WithdrawId",
                table: "Transaction",
                column: "WithdrawId",
                principalTable: "Withdraws",
                principalColumn: "WithdrawId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Withdraws_WithdrawId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_WithdrawId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "WithdrawId",
                table: "Transaction");
        }
    }
}
