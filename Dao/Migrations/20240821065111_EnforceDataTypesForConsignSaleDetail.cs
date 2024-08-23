using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class EnforceDataTypesForConsignSaleDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Size",
                table: "ConsignSaleDetail",
                type: "varchar",
                maxLength: 15,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "ProductName",
                table: "ConsignSaleDetail",
                type: "varchar",
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Condition",
                table: "ConsignSaleDetail",
                type: "varchar",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Color",
                table: "ConsignSaleDetail",
                type: "varchar",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Brand",
                table: "ConsignSaleDetail",
                type: "varchar",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Size",
                table: "ConsignSaleDetail",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar",
                oldMaxLength: 15);

            migrationBuilder.AlterColumn<string>(
                name: "ProductName",
                table: "ConsignSaleDetail",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar",
                oldMaxLength: 60);

            migrationBuilder.AlterColumn<string>(
                name: "Condition",
                table: "ConsignSaleDetail",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Color",
                table: "ConsignSaleDetail",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Brand",
                table: "ConsignSaleDetail",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar",
                oldMaxLength: 30);
        }
    }
}
