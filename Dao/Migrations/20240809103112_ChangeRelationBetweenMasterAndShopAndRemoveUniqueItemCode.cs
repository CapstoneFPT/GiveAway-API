using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class ChangeRelationBetweenMasterAndShopAndRemoveUniqueItemCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MasterFashionItemShop");

            migrationBuilder.DropIndex(
                name: "IX_MasterFashionItems_ItemCode",
                table: "MasterFashionItems");

            migrationBuilder.AddColumn<Guid>(
                name: "ShopId",
                table: "MasterFashionItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_MasterFashionItems_ShopId",
                table: "MasterFashionItems",
                column: "ShopId");

            migrationBuilder.AddForeignKey(
                name: "FK_MasterFashionItems_Shops_ShopId",
                table: "MasterFashionItems",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "ShopId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MasterFashionItems_Shops_ShopId",
                table: "MasterFashionItems");

            migrationBuilder.DropIndex(
                name: "IX_MasterFashionItems_ShopId",
                table: "MasterFashionItems");

            migrationBuilder.DropColumn(
                name: "ShopId",
                table: "MasterFashionItems");

            migrationBuilder.CreateTable(
                name: "MasterFashionItemShop",
                columns: table => new
                {
                    MasterFashionItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShopId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterFashionItemShop", x => new { x.MasterFashionItemId, x.ShopId });
                    table.ForeignKey(
                        name: "FK_MasterFashionItemShop_MasterFashionItems_MasterFashionItemId",
                        column: x => x.MasterFashionItemId,
                        principalTable: "MasterFashionItems",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MasterFashionItemShop_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "ShopId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MasterFashionItems_ItemCode",
                table: "MasterFashionItems",
                column: "ItemCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MasterFashionItemShop_ShopId",
                table: "MasterFashionItemShop",
                column: "ShopId");
        }
    }
}
