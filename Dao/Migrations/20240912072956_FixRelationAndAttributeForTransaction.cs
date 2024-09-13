using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class FixRelationAndAttributeForTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Account_MemberId",
                table: "Transaction");

            migrationBuilder.RenameColumn(
                name: "MemberId",
                table: "Transaction",
                newName: "SenderId");

            migrationBuilder.RenameIndex(
                name: "IX_Transaction_MemberId",
                table: "Transaction",
                newName: "IX_Transaction_SenderId");

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "Transaction",
                type: "varchar",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ReceiverId",
                table: "Transaction",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_ReceiverId",
                table: "Transaction",
                column: "ReceiverId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Account_ReceiverId",
                table: "Transaction",
                column: "ReceiverId",
                principalTable: "Account",
                principalColumn: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Account_SenderId",
                table: "Transaction",
                column: "SenderId",
                principalTable: "Account",
                principalColumn: "AccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Account_ReceiverId",
                table: "Transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Account_SenderId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_ReceiverId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "ReceiverId",
                table: "Transaction");

            migrationBuilder.RenameColumn(
                name: "SenderId",
                table: "Transaction",
                newName: "MemberId");

            migrationBuilder.RenameIndex(
                name: "IX_Transaction_SenderId",
                table: "Transaction",
                newName: "IX_Transaction_MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Account_MemberId",
                table: "Transaction",
                column: "MemberId",
                principalTable: "Account",
                principalColumn: "AccountId");
        }
    }
}
