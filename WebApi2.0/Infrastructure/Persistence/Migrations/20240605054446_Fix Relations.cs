﻿#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace WebApi2._0.Infrastructure.Persistence.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class FixRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transaction_OrderId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Shops_StaffId",
                table: "Shops");

            migrationBuilder.DropIndex(
                name: "IX_Schedule_AuctionId",
                table: "Schedule");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_OrderId",
                table: "Transaction",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shops_StaffId",
                table: "Shops",
                column: "StaffId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Schedule_AuctionId",
                table: "Schedule",
                column: "AuctionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transaction_OrderId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Shops_StaffId",
                table: "Shops");

            migrationBuilder.DropIndex(
                name: "IX_Schedule_AuctionId",
                table: "Schedule");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_OrderId",
                table: "Transaction",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Shops_StaffId",
                table: "Shops",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedule_AuctionId",
                table: "Schedule",
                column: "AuctionId");
        }
    }
}
