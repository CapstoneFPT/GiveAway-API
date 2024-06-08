#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class Renametable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Package_PackageId",
                table: "Transaction");

            migrationBuilder.DropTable(
                name: "Package");

            migrationBuilder.CreateTable(
                name: "PointPackage",
                columns: table => new
                {
                    PointPackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointPackage", x => x.PointPackageId);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_PointPackage_PackageId",
                table: "Transaction",
                column: "PackageId",
                principalTable: "PointPackage",
                principalColumn: "PointPackageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_PointPackage_PackageId",
                table: "Transaction");

            migrationBuilder.DropTable(
                name: "PointPackage");

            migrationBuilder.CreateTable(
                name: "Package",
                columns: table => new
                {
                    PackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Package", x => x.PackageId);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Package_PackageId",
                table: "Transaction",
                column: "PackageId",
                principalTable: "Package",
                principalColumn: "PackageId");
        }
    }
}
