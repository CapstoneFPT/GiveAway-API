using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class RemoveVariantAndUpdateFashionItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IndividualFashionItems_FashionItemVariations_VariationId",
                table: "IndividualFashionItems");

            migrationBuilder.DropTable(
                name: "FashionItemVariations");

            migrationBuilder.RenameColumn(
                name: "VariationId",
                table: "IndividualFashionItems",
                newName: "MasterItemId");

            migrationBuilder.RenameIndex(
                name: "IX_IndividualFashionItems_VariationId",
                table: "IndividualFashionItems",
                newName: "IX_IndividualFashionItems_MasterItemId");

            migrationBuilder.AddColumn<int>(
                name: "StockCount",
                table: "MasterFashionItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "IndividualFashionItems",
                type: "varchar",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Condition",
                table: "IndividualFashionItems",
                type: "varchar",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "RetailPrice",
                table: "IndividualFashionItems",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "IndividualFashionItems",
                type: "varchar",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_IndividualFashionItems_MasterFashionItems_MasterItemId",
                table: "IndividualFashionItems",
                column: "MasterItemId",
                principalTable: "MasterFashionItems",
                principalColumn: "MasterItemId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IndividualFashionItems_MasterFashionItems_MasterItemId",
                table: "IndividualFashionItems");

            migrationBuilder.DropColumn(
                name: "StockCount",
                table: "MasterFashionItems");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "IndividualFashionItems");

            migrationBuilder.DropColumn(
                name: "Condition",
                table: "IndividualFashionItems");

            migrationBuilder.DropColumn(
                name: "RetailPrice",
                table: "IndividualFashionItems");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "IndividualFashionItems");

            migrationBuilder.RenameColumn(
                name: "MasterItemId",
                table: "IndividualFashionItems",
                newName: "VariationId");

            migrationBuilder.RenameIndex(
                name: "IX_IndividualFashionItems_MasterItemId",
                table: "IndividualFashionItems",
                newName: "IX_IndividualFashionItems_VariationId");

            migrationBuilder.CreateTable(
                name: "FashionItemVariations",
                columns: table => new
                {
                    VariationId = table.Column<Guid>(type: "uuid", nullable: false),
                    MasterItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Color = table.Column<string>(type: "text", nullable: false),
                    Condition = table.Column<string>(type: "varchar", maxLength: 30, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Size = table.Column<string>(type: "varchar", maxLength: 20, nullable: false),
                    StockCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FashionItemVariations", x => x.VariationId);
                    table.ForeignKey(
                        name: "FK_FashionItemVariations_MasterFashionItems_MasterItemId",
                        column: x => x.MasterItemId,
                        principalTable: "MasterFashionItems",
                        principalColumn: "MasterItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FashionItemVariations_MasterItemId",
                table: "FashionItemVariations",
                column: "MasterItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_IndividualFashionItems_FashionItemVariations_VariationId",
                table: "IndividualFashionItems",
                column: "VariationId",
                principalTable: "FashionItemVariations",
                principalColumn: "VariationId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
