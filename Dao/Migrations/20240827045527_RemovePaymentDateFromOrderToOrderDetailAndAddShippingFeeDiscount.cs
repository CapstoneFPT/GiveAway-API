using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class RemovePaymentDateFromOrderToOrderDetailAndAddShippingFeeDiscount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentDate",
                table: "Order");

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDate",
                table: "OrderDetail",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                table: "Order",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingFee",
                table: "Order",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentDate",
                table: "OrderDetail");

            migrationBuilder.DropColumn(
                name: "Discount",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "ShippingFee",
                table: "Order");

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDate",
                table: "Order",
                type: "timestamptz",
                nullable: true);
        }
    }
}
