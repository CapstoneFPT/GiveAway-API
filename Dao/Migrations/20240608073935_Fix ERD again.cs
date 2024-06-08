using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class FixERDagain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Auction_Item_AuctionItemId",
                table: "Auction");

            migrationBuilder.DropForeignKey(
                name: "FK_Image_Item_ItemId",
                table: "Image");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetail_Item_ItemId",
                table: "OrderDetail");

            migrationBuilder.DropTable(
                name: "Item");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetail_ItemId",
                table: "OrderDetail");

            migrationBuilder.DropIndex(
                name: "IX_Image_ItemId",
                table: "Image");

            migrationBuilder.DropIndex(
                name: "IX_Auction_AuctionItemId",
                table: "Auction");

            migrationBuilder.AddColumn<string>(
                name: "BankAccountNumber",
                table: "Wallet",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "Wallet",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "ItemId",
                table: "OrderDetail",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "FashionItemItemId",
                table: "OrderDetail",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PointPackageId",
                table: "OrderDetail",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "OrderDetail",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "RequestId",
                table: "OrderDetail",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FashionItemItemId",
                table: "Image",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TransactionId",
                table: "AuctionDeposit",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "AuctionFashionItemItemId",
                table: "Auction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "FashionItem",
                columns: table => new
                {
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    SellingPrice = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Name = table.Column<string>(type: "varchar", maxLength: 50, nullable: false),
                    Note = table.Column<string>(type: "varchar", maxLength: 100, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<decimal>(type: "numeric", nullable: true),
                    Condition = table.Column<string>(type: "text", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShopId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "varchar", maxLength: 20, nullable: false),
                    Duration = table.Column<int>(type: "integer", nullable: true),
                    InitialPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    AuctionItemStatus = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FashionItem", x => x.ItemId);
                    table.ForeignKey(
                        name: "FK_FashionItem_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FashionItem_Request_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Request",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FashionItem_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "ShopId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetail_FashionItemItemId",
                table: "OrderDetail",
                column: "FashionItemItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetail_PointPackageId",
                table: "OrderDetail",
                column: "PointPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetail_RequestId",
                table: "OrderDetail",
                column: "RequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Image_FashionItemItemId",
                table: "Image",
                column: "FashionItemItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AuctionDeposit_TransactionId",
                table: "AuctionDeposit",
                column: "TransactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Auction_AuctionFashionItemItemId",
                table: "Auction",
                column: "AuctionFashionItemItemId");

            migrationBuilder.CreateIndex(
                name: "IX_FashionItem_CategoryId",
                table: "FashionItem",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FashionItem_RequestId",
                table: "FashionItem",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_FashionItem_ShopId",
                table: "FashionItem",
                column: "ShopId");

            migrationBuilder.AddForeignKey(
                name: "FK_Auction_FashionItem_AuctionFashionItemItemId",
                table: "Auction",
                column: "AuctionFashionItemItemId",
                principalTable: "FashionItem",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AuctionDeposit_Transaction_TransactionId",
                table: "AuctionDeposit",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "TransactionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Image_FashionItem_FashionItemItemId",
                table: "Image",
                column: "FashionItemItemId",
                principalTable: "FashionItem",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetail_FashionItem_FashionItemItemId",
                table: "OrderDetail",
                column: "FashionItemItemId",
                principalTable: "FashionItem",
                principalColumn: "ItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetail_PointPackage_PointPackageId",
                table: "OrderDetail",
                column: "PointPackageId",
                principalTable: "PointPackage",
                principalColumn: "PointPackageId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetail_Request_RequestId",
                table: "OrderDetail",
                column: "RequestId",
                principalTable: "Request",
                principalColumn: "RequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Auction_FashionItem_AuctionFashionItemItemId",
                table: "Auction");

            migrationBuilder.DropForeignKey(
                name: "FK_AuctionDeposit_Transaction_TransactionId",
                table: "AuctionDeposit");

            migrationBuilder.DropForeignKey(
                name: "FK_Image_FashionItem_FashionItemItemId",
                table: "Image");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetail_FashionItem_FashionItemItemId",
                table: "OrderDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetail_PointPackage_PointPackageId",
                table: "OrderDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetail_Request_RequestId",
                table: "OrderDetail");

            migrationBuilder.DropTable(
                name: "FashionItem");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetail_FashionItemItemId",
                table: "OrderDetail");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetail_PointPackageId",
                table: "OrderDetail");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetail_RequestId",
                table: "OrderDetail");

            migrationBuilder.DropIndex(
                name: "IX_Image_FashionItemItemId",
                table: "Image");

            migrationBuilder.DropIndex(
                name: "IX_AuctionDeposit_TransactionId",
                table: "AuctionDeposit");

            migrationBuilder.DropIndex(
                name: "IX_Auction_AuctionFashionItemItemId",
                table: "Auction");

            migrationBuilder.DropColumn(
                name: "BankAccountNumber",
                table: "Wallet");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "Wallet");

            migrationBuilder.DropColumn(
                name: "FashionItemItemId",
                table: "OrderDetail");

            migrationBuilder.DropColumn(
                name: "PointPackageId",
                table: "OrderDetail");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "OrderDetail");

            migrationBuilder.DropColumn(
                name: "RequestId",
                table: "OrderDetail");

            migrationBuilder.DropColumn(
                name: "FashionItemItemId",
                table: "Image");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "AuctionDeposit");

            migrationBuilder.DropColumn(
                name: "AuctionFashionItemItemId",
                table: "Auction");

            migrationBuilder.AlterColumn<Guid>(
                name: "ItemId",
                table: "OrderDetail",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Item",
                columns: table => new
                {
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShopId = table.Column<Guid>(type: "uuid", nullable: false),
                    Condition = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "varchar", maxLength: 50, nullable: false),
                    Note = table.Column<string>(type: "varchar", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "varchar", maxLength: 20, nullable: false),
                    Type = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    Value = table.Column<decimal>(type: "numeric", nullable: false),
                    AuctionItemStatus = table.Column<string>(type: "text", nullable: true),
                    Duration = table.Column<int>(type: "integer", nullable: true),
                    InitialPrice = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item", x => x.ItemId);
                    table.ForeignKey(
                        name: "FK_Item_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Item_Request_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Request",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Item_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "ShopId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetail_ItemId",
                table: "OrderDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Image_ItemId",
                table: "Image",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Auction_AuctionItemId",
                table: "Auction",
                column: "AuctionItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Item_CategoryId",
                table: "Item",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Item_RequestId",
                table: "Item",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Item_ShopId",
                table: "Item",
                column: "ShopId");

            migrationBuilder.AddForeignKey(
                name: "FK_Auction_Item_AuctionItemId",
                table: "Auction",
                column: "AuctionItemId",
                principalTable: "Item",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Image_Item_ItemId",
                table: "Image",
                column: "ItemId",
                principalTable: "Item",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetail_Item_ItemId",
                table: "OrderDetail",
                column: "ItemId",
                principalTable: "Item",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
