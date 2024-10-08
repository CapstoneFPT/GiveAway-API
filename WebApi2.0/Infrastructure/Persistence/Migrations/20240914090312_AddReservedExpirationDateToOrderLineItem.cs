﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi2._0.Infrastructure.Persistence.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddReservedExpirationDateToOrderLineItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReservedExpirationDate",
                table: "OrderLineItem",
                type: "timestamptz",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReservedExpirationDate",
                table: "OrderLineItem");
        }
    }
}