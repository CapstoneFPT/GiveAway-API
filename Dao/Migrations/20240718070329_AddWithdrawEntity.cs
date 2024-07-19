using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class AddWithdrawEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RefundId",
                table: "Image",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bank",
                table: "Account",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountName",
                table: "Account",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountNumber",
                table: "Account",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Withdraws",
                columns: table => new
                {
                    WithdrawId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Withdraws", x => x.WithdrawId);
                    table.ForeignKey(
                        name: "FK_Withdraws_Account_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Image_RefundId",
                table: "Image",
                column: "RefundId");

            migrationBuilder.CreateIndex(
                name: "IX_Withdraws_MemberId",
                table: "Withdraws",
                column: "MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Image_Refund_RefundId",
                table: "Image",
                column: "RefundId",
                principalTable: "Refund",
                principalColumn: "RefundId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Image_Refund_RefundId",
                table: "Image");

            migrationBuilder.DropTable(
                name: "Withdraws");

            migrationBuilder.DropIndex(
                name: "IX_Image_RefundId",
                table: "Image");

            migrationBuilder.DropColumn(
                name: "RefundId",
                table: "Image");

            migrationBuilder.DropColumn(
                name: "Bank",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "BankAccountName",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "BankAccountNumber",
                table: "Account");
        }
    }
}
