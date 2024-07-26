using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class FashionItemSellingPriceIsNullableAndChangeColumnNameForConsign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RecipientName",
                table: "ConsignSale",
                newName: "ConsignorName");

            migrationBuilder.RenameColumn(
                name: "MemberReceivedAmount",
                table: "ConsignSale",
                newName: "ConsignorReceivedAmount");

            migrationBuilder.AlterColumn<int>(
                name: "SellingPrice",
                table: "FashionItem",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ConsignorReceivedAmount",
                table: "ConsignSale",
                newName: "MemberReceivedAmount");

            migrationBuilder.RenameColumn(
                name: "ConsignorName",
                table: "ConsignSale",
                newName: "RecipientName");

            migrationBuilder.AlterColumn<int>(
                name: "SellingPrice",
                table: "FashionItem",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
