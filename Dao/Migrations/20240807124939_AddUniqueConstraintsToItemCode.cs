using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintsToItemCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_MasterFashionItems_ItemCode",
                table: "MasterFashionItems",
                column: "ItemCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IndividualFashionItems_ItemCode",
                table: "IndividualFashionItems",
                column: "ItemCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MasterFashionItems_ItemCode",
                table: "MasterFashionItems");

            migrationBuilder.DropIndex(
                name: "IX_IndividualFashionItems_ItemCode",
                table: "IndividualFashionItems");
        }
    }
}
