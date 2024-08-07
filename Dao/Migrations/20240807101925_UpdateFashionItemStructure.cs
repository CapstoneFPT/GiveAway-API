using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFashionItemStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Auction_FashionItem_AuctionFashionItemId",
                table: "Auction");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsignSaleDetail_FashionItem_FashionItemId",
                table: "ConsignSaleDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_Image_FashionItem_FashionItemId",
                table: "Image");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetail_FashionItem_FashionItemId",
                table: "OrderDetail");

            migrationBuilder.DropTable(
                name: "FashionItem");

            migrationBuilder.DropIndex(
                name: "IX_Image_FashionItemId",
                table: "Image");

            migrationBuilder.DropIndex(
                name: "IX_ConsignSaleDetail_FashionItemId",
                table: "ConsignSaleDetail");

            migrationBuilder.DropColumn(
                name: "FashionItemId",
                table: "Image");

            migrationBuilder.RenameColumn(
                name: "FashionItemId",
                table: "OrderDetail",
                newName: "IndividualFashionItemId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderDetail_FashionItemId",
                table: "OrderDetail",
                newName: "IX_OrderDetail_IndividualFashionItemId");

            migrationBuilder.RenameColumn(
                name: "AuctionFashionItemId",
                table: "Auction",
                newName: "IndividualAuctionFashionItemId");

            migrationBuilder.RenameIndex(
                name: "IX_Auction_AuctionFashionItemId",
                table: "Auction",
                newName: "IX_Auction_IndividualAuctionFashionItemId");

            migrationBuilder.AddColumn<Guid>(
                name: "ConsignSaleDetailId",
                table: "Image",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IndividualFashionItemId",
                table: "Image",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MasterFashionItemId",
                table: "Image",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ConfirmedPrice",
                table: "ConsignSaleDetail",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "ConsignSaleDetail",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "MasterFashionItems",
                columns: table => new
                {
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemCode = table.Column<string>(type: "varchar", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "varchar", maxLength: 100, nullable: false),
                    Brand = table.Column<string>(type: "varchar", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Gender = table.Column<string>(type: "varchar", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterFashionItems", x => x.ItemId);
                    table.ForeignKey(
                        name: "FK_MasterFashionItems_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FashionItemVariations",
                columns: table => new
                {
                    VariationId = table.Column<Guid>(type: "uuid", nullable: false),
                    MasterItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Condition = table.Column<string>(type: "varchar", maxLength: 30, nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Color = table.Column<string>(type: "text", nullable: false),
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
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndividualFashionItems",
                columns: table => new
                {
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemCode = table.Column<string>(type: "text", nullable: false),
                    VariationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: false),
                    SellingPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ShopId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "varchar", maxLength: 20, nullable: false),
                    ConsignSaleDetailId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InitialPrice = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndividualFashionItems", x => x.ItemId);
                    table.ForeignKey(
                        name: "FK_IndividualFashionItems_ConsignSaleDetail_ConsignSaleDetailId",
                        column: x => x.ConsignSaleDetailId,
                        principalTable: "ConsignSaleDetail",
                        principalColumn: "ConsignSaleDetailId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IndividualFashionItems_FashionItemVariations_VariationId",
                        column: x => x.VariationId,
                        principalTable: "FashionItemVariations",
                        principalColumn: "VariationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IndividualFashionItems_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "ShopId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Image_ConsignSaleDetailId",
                table: "Image",
                column: "ConsignSaleDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_Image_IndividualFashionItemId",
                table: "Image",
                column: "IndividualFashionItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Image_MasterFashionItemId",
                table: "Image",
                column: "MasterFashionItemId");

            migrationBuilder.CreateIndex(
                name: "IX_FashionItemVariations_MasterItemId",
                table: "FashionItemVariations",
                column: "MasterItemId");

            migrationBuilder.CreateIndex(
                name: "IX_IndividualFashionItems_ConsignSaleDetailId",
                table: "IndividualFashionItems",
                column: "ConsignSaleDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_IndividualFashionItems_ShopId",
                table: "IndividualFashionItems",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_IndividualFashionItems_VariationId",
                table: "IndividualFashionItems",
                column: "VariationId");

            migrationBuilder.CreateIndex(
                name: "IX_MasterFashionItems_CategoryId",
                table: "MasterFashionItems",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Auction_IndividualFashionItems_IndividualAuctionFashionItem~",
                table: "Auction",
                column: "IndividualAuctionFashionItemId",
                principalTable: "IndividualFashionItems",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Image_ConsignSaleDetail_ConsignSaleDetailId",
                table: "Image",
                column: "ConsignSaleDetailId",
                principalTable: "ConsignSaleDetail",
                principalColumn: "ConsignSaleDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_Image_IndividualFashionItems_IndividualFashionItemId",
                table: "Image",
                column: "IndividualFashionItemId",
                principalTable: "IndividualFashionItems",
                principalColumn: "ItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Image_MasterFashionItems_MasterFashionItemId",
                table: "Image",
                column: "MasterFashionItemId",
                principalTable: "MasterFashionItems",
                principalColumn: "ItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetail_IndividualFashionItems_IndividualFashionItemId",
                table: "OrderDetail",
                column: "IndividualFashionItemId",
                principalTable: "IndividualFashionItems",
                principalColumn: "ItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Auction_IndividualFashionItems_IndividualAuctionFashionItem~",
                table: "Auction");

            migrationBuilder.DropForeignKey(
                name: "FK_Image_ConsignSaleDetail_ConsignSaleDetailId",
                table: "Image");

            migrationBuilder.DropForeignKey(
                name: "FK_Image_IndividualFashionItems_IndividualFashionItemId",
                table: "Image");

            migrationBuilder.DropForeignKey(
                name: "FK_Image_MasterFashionItems_MasterFashionItemId",
                table: "Image");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetail_IndividualFashionItems_IndividualFashionItemId",
                table: "OrderDetail");

            migrationBuilder.DropTable(
                name: "IndividualFashionItems");

            migrationBuilder.DropTable(
                name: "FashionItemVariations");

            migrationBuilder.DropTable(
                name: "MasterFashionItems");

            migrationBuilder.DropIndex(
                name: "IX_Image_ConsignSaleDetailId",
                table: "Image");

            migrationBuilder.DropIndex(
                name: "IX_Image_IndividualFashionItemId",
                table: "Image");

            migrationBuilder.DropIndex(
                name: "IX_Image_MasterFashionItemId",
                table: "Image");

            migrationBuilder.DropColumn(
                name: "ConsignSaleDetailId",
                table: "Image");

            migrationBuilder.DropColumn(
                name: "IndividualFashionItemId",
                table: "Image");

            migrationBuilder.DropColumn(
                name: "MasterFashionItemId",
                table: "Image");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "ConsignSaleDetail");

            migrationBuilder.RenameColumn(
                name: "IndividualFashionItemId",
                table: "OrderDetail",
                newName: "FashionItemId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderDetail_IndividualFashionItemId",
                table: "OrderDetail",
                newName: "IX_OrderDetail_FashionItemId");

            migrationBuilder.RenameColumn(
                name: "IndividualAuctionFashionItemId",
                table: "Auction",
                newName: "AuctionFashionItemId");

            migrationBuilder.RenameIndex(
                name: "IX_Auction_IndividualAuctionFashionItemId",
                table: "Auction",
                newName: "IX_Auction_AuctionFashionItemId");

            migrationBuilder.AddColumn<Guid>(
                name: "FashionItemId",
                table: "Image",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<decimal>(
                name: "ConfirmedPrice",
                table: "ConsignSaleDetail",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "FashionItem",
                columns: table => new
                {
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    ShopId = table.Column<Guid>(type: "uuid", nullable: false),
                    Brand = table.Column<string>(type: "text", nullable: true),
                    Color = table.Column<string>(type: "text", nullable: false),
                    Condition = table.Column<decimal>(type: "numeric", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Gender = table.Column<string>(type: "varchar", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "varchar", maxLength: 50, nullable: false),
                    Note = table.Column<string>(type: "varchar", maxLength: 100, nullable: true),
                    SellingPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    Size = table.Column<string>(type: "varchar", maxLength: 5, nullable: false),
                    Status = table.Column<string>(type: "varchar", maxLength: 20, nullable: false),
                    Type = table.Column<string>(type: "varchar", maxLength: 100, nullable: false),
                    Duration = table.Column<int>(type: "integer", nullable: true),
                    InitialPrice = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FashionItem", x => x.ItemId);
                    table.ForeignKey(
                        name: "FK_FashionItem_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "CategoryId");
                    table.ForeignKey(
                        name: "FK_FashionItem_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "ShopId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Image_FashionItemId",
                table: "Image",
                column: "FashionItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsignSaleDetail_FashionItemId",
                table: "ConsignSaleDetail",
                column: "FashionItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FashionItem_CategoryId",
                table: "FashionItem",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FashionItem_ShopId",
                table: "FashionItem",
                column: "ShopId");

            migrationBuilder.AddForeignKey(
                name: "FK_Auction_FashionItem_AuctionFashionItemId",
                table: "Auction",
                column: "AuctionFashionItemId",
                principalTable: "FashionItem",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConsignSaleDetail_FashionItem_FashionItemId",
                table: "ConsignSaleDetail",
                column: "FashionItemId",
                principalTable: "FashionItem",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Image_FashionItem_FashionItemId",
                table: "Image",
                column: "FashionItemId",
                principalTable: "FashionItem",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetail_FashionItem_FashionItemId",
                table: "OrderDetail",
                column: "FashionItemId",
                principalTable: "FashionItem",
                principalColumn: "ItemId");
        }
    }
}
