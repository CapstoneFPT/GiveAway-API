using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class DetailRenamedToLineItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedback_OrderDetail_OrderDetailId",
                table: "Feedback");

            migrationBuilder.DropForeignKey(
                name: "FK_Image_ConsignSaleDetail_ConsignSaleDetailId",
                table: "Image");

            migrationBuilder.DropForeignKey(
                name: "FK_IndividualFashionItems_ConsignSaleDetail_ConsignSaleDetailId",
                table: "IndividualFashionItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Refund_OrderDetail_OrderDetailId",
                table: "Refund");

            migrationBuilder.DropTable(
                name: "ConsignSaleDetail");

            migrationBuilder.DropTable(
                name: "OrderDetail");

            migrationBuilder.RenameColumn(
                name: "OrderDetailId",
                table: "Refund",
                newName: "OrderLineItemId");

            migrationBuilder.RenameIndex(
                name: "IX_Refund_OrderDetailId",
                table: "Refund",
                newName: "IX_Refund_OrderLineItemId");

            migrationBuilder.RenameColumn(
                name: "ConsignSaleDetailId",
                table: "IndividualFashionItems",
                newName: "ConsignSaleLineItemId");

            migrationBuilder.RenameIndex(
                name: "IX_IndividualFashionItems_ConsignSaleDetailId",
                table: "IndividualFashionItems",
                newName: "IX_IndividualFashionItems_ConsignSaleLineItemId");

            migrationBuilder.RenameColumn(
                name: "ConsignSaleDetailId",
                table: "Image",
                newName: "ConsignLineItemId");

            migrationBuilder.RenameIndex(
                name: "IX_Image_ConsignSaleDetailId",
                table: "Image",
                newName: "IX_Image_ConsignLineItemId");

            migrationBuilder.RenameColumn(
                name: "OrderDetailId",
                table: "Feedback",
                newName: "OrderLineItemId");

            migrationBuilder.RenameIndex(
                name: "IX_Feedback_OrderDetailId",
                table: "Feedback",
                newName: "IX_Feedback_OrderLineItemId");

            migrationBuilder.CreateTable(
                name: "ConsignSaleLineItem",
                columns: table => new
                {
                    ConsignSaleLineItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsignSaleId = table.Column<Guid>(type: "uuid", nullable: false),
                    DealPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: false),
                    ConfirmedPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    ProductName = table.Column<string>(type: "varchar", maxLength: 60, nullable: false),
                    Brand = table.Column<string>(type: "varchar", maxLength: 30, nullable: false),
                    Color = table.Column<string>(type: "varchar", maxLength: 30, nullable: false),
                    Size = table.Column<string>(type: "varchar", maxLength: 15, nullable: false),
                    Condition = table.Column<string>(type: "varchar", maxLength: 20, nullable: false),
                    Gender = table.Column<string>(type: "varchar", maxLength: 15, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsignSaleLineItem", x => x.ConsignSaleLineItemId);
                    table.ForeignKey(
                        name: "FK_ConsignSaleLineItem_ConsignSale_ConsignSaleId",
                        column: x => x.ConsignSaleId,
                        principalTable: "ConsignSale",
                        principalColumn: "ConsignSaleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderLineItem",
                columns: table => new
                {
                    OrderLineItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    RefundExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IndividualFashionItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    PointPackageId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderLineItem", x => x.OrderLineItemId);
                    table.ForeignKey(
                        name: "FK_OrderLineItem_IndividualFashionItems_IndividualFashionItemId",
                        column: x => x.IndividualFashionItemId,
                        principalTable: "IndividualFashionItems",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_OrderLineItem_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderLineItem_PointPackage_PointPackageId",
                        column: x => x.PointPackageId,
                        principalTable: "PointPackage",
                        principalColumn: "PointPackageId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsignSaleLineItem_ConsignSaleId",
                table: "ConsignSaleLineItem",
                column: "ConsignSaleId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLineItem_IndividualFashionItemId",
                table: "OrderLineItem",
                column: "IndividualFashionItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLineItem_OrderId",
                table: "OrderLineItem",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLineItem_PointPackageId",
                table: "OrderLineItem",
                column: "PointPackageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedback_OrderLineItem_OrderLineItemId",
                table: "Feedback",
                column: "OrderLineItemId",
                principalTable: "OrderLineItem",
                principalColumn: "OrderLineItemId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Image_ConsignSaleLineItem_ConsignLineItemId",
                table: "Image",
                column: "ConsignLineItemId",
                principalTable: "ConsignSaleLineItem",
                principalColumn: "ConsignSaleLineItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_IndividualFashionItems_ConsignSaleLineItem_ConsignSaleLineI~",
                table: "IndividualFashionItems",
                column: "ConsignSaleLineItemId",
                principalTable: "ConsignSaleLineItem",
                principalColumn: "ConsignSaleLineItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Refund_OrderLineItem_OrderLineItemId",
                table: "Refund",
                column: "OrderLineItemId",
                principalTable: "OrderLineItem",
                principalColumn: "OrderLineItemId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedback_OrderLineItem_OrderLineItemId",
                table: "Feedback");

            migrationBuilder.DropForeignKey(
                name: "FK_Image_ConsignSaleLineItem_ConsignLineItemId",
                table: "Image");

            migrationBuilder.DropForeignKey(
                name: "FK_IndividualFashionItems_ConsignSaleLineItem_ConsignSaleLineI~",
                table: "IndividualFashionItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Refund_OrderLineItem_OrderLineItemId",
                table: "Refund");

            migrationBuilder.DropTable(
                name: "ConsignSaleLineItem");

            migrationBuilder.DropTable(
                name: "OrderLineItem");

            migrationBuilder.RenameColumn(
                name: "OrderLineItemId",
                table: "Refund",
                newName: "OrderDetailId");

            migrationBuilder.RenameIndex(
                name: "IX_Refund_OrderLineItemId",
                table: "Refund",
                newName: "IX_Refund_OrderDetailId");

            migrationBuilder.RenameColumn(
                name: "ConsignSaleLineItemId",
                table: "IndividualFashionItems",
                newName: "ConsignSaleDetailId");

            migrationBuilder.RenameIndex(
                name: "IX_IndividualFashionItems_ConsignSaleLineItemId",
                table: "IndividualFashionItems",
                newName: "IX_IndividualFashionItems_ConsignSaleDetailId");

            migrationBuilder.RenameColumn(
                name: "ConsignLineItemId",
                table: "Image",
                newName: "ConsignSaleDetailId");

            migrationBuilder.RenameIndex(
                name: "IX_Image_ConsignLineItemId",
                table: "Image",
                newName: "IX_Image_ConsignSaleDetailId");

            migrationBuilder.RenameColumn(
                name: "OrderLineItemId",
                table: "Feedback",
                newName: "OrderDetailId");

            migrationBuilder.RenameIndex(
                name: "IX_Feedback_OrderLineItemId",
                table: "Feedback",
                newName: "IX_Feedback_OrderDetailId");

            migrationBuilder.CreateTable(
                name: "ConsignSaleDetail",
                columns: table => new
                {
                    ConsignSaleDetailId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsignSaleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Brand = table.Column<string>(type: "varchar", maxLength: 30, nullable: false),
                    Color = table.Column<string>(type: "varchar", maxLength: 30, nullable: false),
                    Condition = table.Column<string>(type: "varchar", maxLength: 20, nullable: false),
                    ConfirmedPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DealPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    Gender = table.Column<string>(type: "varchar", maxLength: 15, nullable: false),
                    Note = table.Column<string>(type: "text", nullable: false),
                    ProductName = table.Column<string>(type: "varchar", maxLength: 60, nullable: false),
                    Size = table.Column<string>(type: "varchar", maxLength: 15, nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "OrderDetail",
                columns: table => new
                {
                    OrderDetailId = table.Column<Guid>(type: "uuid", nullable: false),
                    IndividualFashionItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    PointPackageId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    RefundExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetail", x => x.OrderDetailId);
                    table.ForeignKey(
                        name: "FK_OrderDetail_IndividualFashionItems_IndividualFashionItemId",
                        column: x => x.IndividualFashionItemId,
                        principalTable: "IndividualFashionItems",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_OrderDetail_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderDetail_PointPackage_PointPackageId",
                        column: x => x.PointPackageId,
                        principalTable: "PointPackage",
                        principalColumn: "PointPackageId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsignSaleDetail_ConsignSaleId",
                table: "ConsignSaleDetail",
                column: "ConsignSaleId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetail_IndividualFashionItemId",
                table: "OrderDetail",
                column: "IndividualFashionItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetail_OrderId",
                table: "OrderDetail",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetail_PointPackageId",
                table: "OrderDetail",
                column: "PointPackageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedback_OrderDetail_OrderDetailId",
                table: "Feedback",
                column: "OrderDetailId",
                principalTable: "OrderDetail",
                principalColumn: "OrderDetailId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Image_ConsignSaleDetail_ConsignSaleDetailId",
                table: "Image",
                column: "ConsignSaleDetailId",
                principalTable: "ConsignSaleDetail",
                principalColumn: "ConsignSaleDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_IndividualFashionItems_ConsignSaleDetail_ConsignSaleDetailId",
                table: "IndividualFashionItems",
                column: "ConsignSaleDetailId",
                principalTable: "ConsignSaleDetail",
                principalColumn: "ConsignSaleDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_Refund_OrderDetail_OrderDetailId",
                table: "Refund",
                column: "OrderDetailId",
                principalTable: "OrderDetail",
                principalColumn: "OrderDetailId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
