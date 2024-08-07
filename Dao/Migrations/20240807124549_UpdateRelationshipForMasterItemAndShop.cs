using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRelationshipForMasterItemAndShop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IndividualFashionItems_Shops_ShopId",
                table: "IndividualFashionItems");

            migrationBuilder.DropIndex(
                name: "IX_IndividualFashionItems_ShopId",
                table: "IndividualFashionItems");

            migrationBuilder.DropColumn(
                name: "ShopId",
                table: "IndividualFashionItems");

            migrationBuilder.AddColumn<string>(
                name: "ShopCode",
                table: "Shops",
                type: "text",
                nullable: false,
                defaultValue: "");

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
                name: "ShopCode",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "ShopId",
                table: "MasterFashionItems");

            migrationBuilder.AddColumn<Guid>(
                name: "ShopId",
                table: "IndividualFashionItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_IndividualFashionItems_ShopId",
                table: "IndividualFashionItems",
                column: "ShopId");

            migrationBuilder.AddForeignKey(
                name: "FK_IndividualFashionItems_Shops_ShopId",
                table: "IndividualFashionItems",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "ShopId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
