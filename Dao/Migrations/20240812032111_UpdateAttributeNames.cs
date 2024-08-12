using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAttributeNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ItemCode",
                table: "MasterFashionItems",
                newName: "MasterItemCode");

            migrationBuilder.RenameColumn(
                name: "IsUniversal",
                table: "MasterFashionItems",
                newName: "IsConsignment");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "MasterFashionItems",
                newName: "MasterItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MasterItemCode",
                table: "MasterFashionItems",
                newName: "ItemCode");

            migrationBuilder.RenameColumn(
                name: "IsConsignment",
                table: "MasterFashionItems",
                newName: "IsUniversal");

            migrationBuilder.RenameColumn(
                name: "MasterItemId",
                table: "MasterFashionItems",
                newName: "ItemId");
        }
    }
}
