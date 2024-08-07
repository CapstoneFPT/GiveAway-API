using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAttributesSize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IndividualFashionItems_ConsignSaleDetail_ConsignSaleDetailId",
                table: "IndividualFashionItems");

            migrationBuilder.DropColumn(
                name: "FashionItemId",
                table: "ConsignSaleDetail");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "MasterFashionItems",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "ConsignSaleDetailId",
                table: "IndividualFashionItems",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "FashionItemVariations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_IndividualFashionItems_ConsignSaleDetail_ConsignSaleDetailId",
                table: "IndividualFashionItems",
                column: "ConsignSaleDetailId",
                principalTable: "ConsignSaleDetail",
                principalColumn: "ConsignSaleDetailId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IndividualFashionItems_ConsignSaleDetail_ConsignSaleDetailId",
                table: "IndividualFashionItems");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "MasterFashionItems");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "FashionItemVariations");

            migrationBuilder.AlterColumn<Guid>(
                name: "ConsignSaleDetailId",
                table: "IndividualFashionItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FashionItemId",
                table: "ConsignSaleDetail",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddForeignKey(
                name: "FK_IndividualFashionItems_ConsignSaleDetail_ConsignSaleDetailId",
                table: "IndividualFashionItems",
                column: "ConsignSaleDetailId",
                principalTable: "ConsignSaleDetail",
                principalColumn: "ConsignSaleDetailId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
