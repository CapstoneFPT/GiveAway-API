using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class Shop1ToNTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ShopId",
                table: "Transaction",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_ShopId",
                table: "Transaction",
                column: "ShopId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Shops_ShopId",
                table: "Transaction",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "ShopId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Shops_ShopId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_ShopId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "ShopId",
                table: "Transaction");
        }
    }
}
