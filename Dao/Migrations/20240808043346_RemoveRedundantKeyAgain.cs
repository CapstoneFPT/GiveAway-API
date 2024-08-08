using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRedundantKeyAgain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MasterFashionItemShop_MasterFashionItems_MasterFashionItems~",
                table: "MasterFashionItemShop");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MasterFashionItemShop",
                table: "MasterFashionItemShop");

            migrationBuilder.DropIndex(
                name: "IX_MasterFashionItemShop_MasterFashionItemsId",
                table: "MasterFashionItemShop");

            migrationBuilder.DropColumn(
                name: "MasterFashionItemItemId",
                table: "MasterFashionItemShop");

            migrationBuilder.RenameColumn(
                name: "MasterFashionItemsId",
                table: "MasterFashionItemShop",
                newName: "MasterFashionItemId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MasterFashionItemShop",
                table: "MasterFashionItemShop",
                columns: new[] { "MasterFashionItemId", "ShopId" });

            migrationBuilder.AddForeignKey(
                name: "FK_MasterFashionItemShop_MasterFashionItems_MasterFashionItemId",
                table: "MasterFashionItemShop",
                column: "MasterFashionItemId",
                principalTable: "MasterFashionItems",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MasterFashionItemShop_MasterFashionItems_MasterFashionItemId",
                table: "MasterFashionItemShop");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MasterFashionItemShop",
                table: "MasterFashionItemShop");

            migrationBuilder.RenameColumn(
                name: "MasterFashionItemId",
                table: "MasterFashionItemShop",
                newName: "MasterFashionItemsId");

            migrationBuilder.AddColumn<Guid>(
                name: "MasterFashionItemItemId",
                table: "MasterFashionItemShop",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_MasterFashionItemShop",
                table: "MasterFashionItemShop",
                columns: new[] { "MasterFashionItemItemId", "ShopId" });

            migrationBuilder.CreateIndex(
                name: "IX_MasterFashionItemShop_MasterFashionItemsId",
                table: "MasterFashionItemShop",
                column: "MasterFashionItemsId");

            migrationBuilder.AddForeignKey(
                name: "FK_MasterFashionItemShop_MasterFashionItems_MasterFashionItems~",
                table: "MasterFashionItemShop",
                column: "MasterFashionItemsId",
                principalTable: "MasterFashionItems",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
