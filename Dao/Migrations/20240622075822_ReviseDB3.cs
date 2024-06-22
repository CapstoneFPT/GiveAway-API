using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class ReviseDB3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Wallet_WalletId",
                table: "Transaction");

            migrationBuilder.DropTable(
                name: "Wallet");

            migrationBuilder.RenameColumn(
                name: "WalletId",
                table: "Transaction",
                newName: "MemberId");

            migrationBuilder.RenameIndex(
                name: "IX_Transaction_WalletId",
                table: "Transaction",
                newName: "IX_Transaction_MemberId");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "FashionItem",
                type: "varchar",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(21)",
                oldMaxLength: 21);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "Delivery",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "StaffId",
                table: "Auction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "Balance",
                table: "Account",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Auction_StaffId",
                table: "Auction",
                column: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Auction_Account_StaffId",
                table: "Auction",
                column: "StaffId",
                principalTable: "Account",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Account_MemberId",
                table: "Transaction",
                column: "MemberId",
                principalTable: "Account",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Auction_Account_StaffId",
                table: "Auction");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Account_MemberId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Auction_StaffId",
                table: "Auction");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "Delivery");

            migrationBuilder.DropColumn(
                name: "StaffId",
                table: "Auction");

            migrationBuilder.DropColumn(
                name: "Balance",
                table: "Account");

            migrationBuilder.RenameColumn(
                name: "MemberId",
                table: "Transaction",
                newName: "WalletId");

            migrationBuilder.RenameIndex(
                name: "IX_Transaction_MemberId",
                table: "Transaction",
                newName: "IX_Transaction_WalletId");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "FashionItem",
                type: "character varying(21)",
                maxLength: 21,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar",
                oldMaxLength: 100);

            migrationBuilder.CreateTable(
                name: "Wallet",
                columns: table => new
                {
                    WalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    Balance = table.Column<int>(type: "integer", nullable: false),
                    BankAccountNumber = table.Column<string>(type: "varchar", maxLength: 20, nullable: true),
                    BankName = table.Column<string>(type: "varchar", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallet", x => x.WalletId);
                    table.ForeignKey(
                        name: "FK_Wallet_Account_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Wallet_MemberId",
                table: "Wallet",
                column: "MemberId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Wallet_WalletId",
                table: "Transaction",
                column: "WalletId",
                principalTable: "Wallet",
                principalColumn: "WalletId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
