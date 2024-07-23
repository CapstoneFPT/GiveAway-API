using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class DeleteValueAndChangeConditionInFashionItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Value",
                table: "FashionItem");

            migrationBuilder.Sql(
                @"ALTER TABLE ""FashionItem"" ALTER COLUMN ""Condition"" TYPE INTEGER USING ""Condition""::INTEGER; ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Condition",
                table: "FashionItem",
                type: "text",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<decimal>(
                name: "Value",
                table: "FashionItem",
                type: "numeric",
                nullable: true);
        }
    }
}
