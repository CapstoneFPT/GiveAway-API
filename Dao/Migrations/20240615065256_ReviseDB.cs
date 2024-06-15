using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class ReviseDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FashionItem_Request_RequestId",
                table: "FashionItem");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetail_Request_RequestId",
                table: "OrderDetail");

            migrationBuilder.DropTable(
                name: "Request");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetail_RequestId",
                table: "OrderDetail");

            migrationBuilder.DropIndex(
                name: "IX_FashionItem_RequestId",
                table: "FashionItem");

            migrationBuilder.DropColumn(
                name: "AuctionItemStatus",
                table: "FashionItem");

            migrationBuilder.DropColumn(
                name: "RequestId",
                table: "FashionItem");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Shops",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Order",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "StepIncrement",
                table: "Auction",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ConsignSale",
                columns: table => new
                {
                    ConsignSaleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "varchar", maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    ConsignDuration = table.Column<int>(type: "integer", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    ShopId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "varchar", maxLength: 20, nullable: false),
                    TotalPrice = table.Column<int>(type: "integer", nullable: false),
                    SoldPrice = table.Column<int>(type: "integer", nullable: false),
                    MemberReceivedAmount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsignSale", x => x.ConsignSaleId);
                    table.ForeignKey(
                        name: "FK_ConsignSale_Account_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsignSale_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "ShopId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConsignSaleDetail",
                columns: table => new
                {
                    ConsignSaleDetailId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsignSaleId = table.Column<Guid>(type: "uuid", nullable: false),
                    FashionItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    DealPrice = table.Column<int>(type: "integer", nullable: false),
                    ConfirmedPrice = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsignSaleDetail", x => x.ConsignSaleDetailId);
                    table.ForeignKey(
                        name: "FK_ConsignSaleDetail_ConsignSale_ConsignSaleId",
                        column: x => x.ConsignSaleId,
                        principalTable: "ConsignSale",
                        principalColumn: "ConsignSaleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsignSaleDetail_FashionItem_FashionItemId",
                        column: x => x.FashionItemId,
                        principalTable: "FashionItem",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetail_RequestId",
                table: "OrderDetail",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsignSale_MemberId",
                table: "ConsignSale",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsignSale_ShopId",
                table: "ConsignSale",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsignSaleDetail_ConsignSaleId",
                table: "ConsignSaleDetail",
                column: "ConsignSaleId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsignSaleDetail_FashionItemId",
                table: "ConsignSaleDetail",
                column: "FashionItemId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetail_ConsignSale_RequestId",
                table: "OrderDetail",
                column: "RequestId",
                principalTable: "ConsignSale",
                principalColumn: "ConsignSaleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetail_ConsignSale_RequestId",
                table: "OrderDetail");

            migrationBuilder.DropTable(
                name: "ConsignSaleDetail");

            migrationBuilder.DropTable(
                name: "ConsignSale");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetail_RequestId",
                table: "OrderDetail");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "StepIncrement",
                table: "Auction");

            migrationBuilder.AddColumn<string>(
                name: "AuctionItemStatus",
                table: "FashionItem",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RequestId",
                table: "FashionItem",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Request",
                columns: table => new
                {
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShopId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsignDuration = table.Column<int>(type: "integer", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    Status = table.Column<string>(type: "varchar", maxLength: 20, nullable: false),
                    Type = table.Column<string>(type: "varchar", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Request", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_Request_Account_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Request_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "ShopId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetail_RequestId",
                table: "OrderDetail",
                column: "RequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FashionItem_RequestId",
                table: "FashionItem",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Request_MemberId",
                table: "Request",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Request_ShopId",
                table: "Request",
                column: "ShopId");

            migrationBuilder.AddForeignKey(
                name: "FK_FashionItem_Request_RequestId",
                table: "FashionItem",
                column: "RequestId",
                principalTable: "Request",
                principalColumn: "RequestId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetail_Request_RequestId",
                table: "OrderDetail",
                column: "RequestId",
                principalTable: "Request",
                principalColumn: "RequestId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
