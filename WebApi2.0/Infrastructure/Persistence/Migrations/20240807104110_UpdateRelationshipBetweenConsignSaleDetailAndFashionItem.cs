﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi2._0.Infrastructure.Persistence.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRelationshipBetweenConsignSaleDetailAndFashionItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IndividualFashionItems_ConsignSaleDetailId",
                table: "IndividualFashionItems");

            migrationBuilder.CreateIndex(
                name: "IX_IndividualFashionItems_ConsignSaleDetailId",
                table: "IndividualFashionItems",
                column: "ConsignSaleDetailId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IndividualFashionItems_ConsignSaleDetailId",
                table: "IndividualFashionItems");

            migrationBuilder.CreateIndex(
                name: "IX_IndividualFashionItems_ConsignSaleDetailId",
                table: "IndividualFashionItems",
                column: "ConsignSaleDetailId");
        }
    }
}