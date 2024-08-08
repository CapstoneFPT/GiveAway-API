using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class ManyToManyMasterItemShops : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.AddColumn<Point>(
                name: "Location",
                table: "Shops",
                type: "geography(Point)",
                nullable: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsUniversal",
                table: "MasterFashionItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "MasterFashionItemShop",
                columns: table => new
                {
                    ShopId = table.Column<Guid>(type: "uuid", nullable: false),
                    MasterFashionItemsItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterFashionItemShop", x => new { x.MasterFashionItemsItemId, x.ShopId });
                    table.ForeignKey(
                        name: "FK_MasterFashionItemShop_MasterFashionItems_MasterFashionItems~",
                        column: x => x.MasterFashionItemsItemId,
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
                name: "IX_MasterFashionItemShop_ShopId",
                table: "MasterFashionItemShop",
                column: "ShopId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MasterFashionItemShop");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "IsUniversal",
                table: "MasterFashionItems");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:postgis", ",,");

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
    }
}
