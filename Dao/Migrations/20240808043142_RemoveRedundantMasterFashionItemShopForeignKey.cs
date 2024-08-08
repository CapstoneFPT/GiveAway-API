using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRedundantMasterFashionItemShopForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MasterFashionItemShop_MasterFashionItems_MasterFashionItems~",
                table: "MasterFashionItemShop");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "MasterFashionItemShop",
                newName: "MasterFashionItemsId");

            migrationBuilder.RenameColumn(
                name: "MasterFashionItemsItemId",
                table: "MasterFashionItemShop",
                newName: "MasterFashionItemItemId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MasterFashionItemShop_MasterFashionItems_MasterFashionItems~",
                table: "MasterFashionItemShop");

            migrationBuilder.DropIndex(
                name: "IX_MasterFashionItemShop_MasterFashionItemsId",
                table: "MasterFashionItemShop");

            migrationBuilder.RenameColumn(
                name: "MasterFashionItemsId",
                table: "MasterFashionItemShop",
                newName: "ItemId");

            migrationBuilder.RenameColumn(
                name: "MasterFashionItemItemId",
                table: "MasterFashionItemShop",
                newName: "MasterFashionItemsItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_MasterFashionItemShop_MasterFashionItems_MasterFashionItems~",
                table: "MasterFashionItemShop",
                column: "MasterFashionItemsItemId",
                principalTable: "MasterFashionItems",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
