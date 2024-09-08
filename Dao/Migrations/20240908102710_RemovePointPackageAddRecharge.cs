using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class RemovePointPackageAddRecharge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderLineItem_PointPackage_PointPackageId",
                table: "OrderLineItem");

            migrationBuilder.DropTable(
                name: "PointPackage");

            migrationBuilder.DropIndex(
                name: "IX_OrderLineItem_PointPackageId",
                table: "OrderLineItem");

            migrationBuilder.DropColumn(
                name: "PointPackageId",
                table: "OrderLineItem");

            migrationBuilder.AddColumn<Guid>(
                name: "RechargeId",
                table: "Transaction",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Recharge",
                columns: table => new
                {
                    RechargeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "varchar", maxLength: 20, nullable: false),
                    PaymentMethod = table.Column<int>(type: "integer", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recharge", x => x.RechargeId);
                    table.ForeignKey(
                        name: "FK_Recharge_Account_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_RechargeId",
                table: "Transaction",
                column: "RechargeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recharge_MemberId",
                table: "Recharge",
                column: "MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Recharge_RechargeId",
                table: "Transaction",
                column: "RechargeId",
                principalTable: "Recharge",
                principalColumn: "RechargeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Recharge_RechargeId",
                table: "Transaction");

            migrationBuilder.DropTable(
                name: "Recharge");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_RechargeId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "RechargeId",
                table: "Transaction");

            migrationBuilder.AddColumn<Guid>(
                name: "PointPackageId",
                table: "OrderLineItem",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PointPackage",
                columns: table => new
                {
                    PointPackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "varchar", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointPackage", x => x.PointPackageId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderLineItem_PointPackageId",
                table: "OrderLineItem",
                column: "PointPackageId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLineItem_PointPackage_PointPackageId",
                table: "OrderLineItem",
                column: "PointPackageId",
                principalTable: "PointPackage",
                principalColumn: "PointPackageId");
        }
    }
}
