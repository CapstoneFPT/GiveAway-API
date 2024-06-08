using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class FixERD2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_PointPackage_PackageId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_PackageId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "PackageId",
                table: "Transaction");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PackageId",
                table: "Transaction",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_PackageId",
                table: "Transaction",
                column: "PackageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_PointPackage_PackageId",
                table: "Transaction",
                column: "PackageId",
                principalTable: "PointPackage",
                principalColumn: "PointPackageId");
        }
    }
}
