﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi2._0.Infrastructure.Persistence.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class ChangRefundAmountToRefundPercentage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RefundAmount",
                table: "Refund",
                newName: "RefundPercentage");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RefundPercentage",
                table: "Refund",
                newName: "RefundAmount");
        }
    }
}
