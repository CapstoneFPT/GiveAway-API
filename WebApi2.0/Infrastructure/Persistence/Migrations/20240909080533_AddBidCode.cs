﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi2._0.Infrastructure.Persistence.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddBidCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BidCode",
                table: "Bid",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BidCode",
                table: "Bid");
        }
    }
}