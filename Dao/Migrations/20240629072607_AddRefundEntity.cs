using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class AddRefundEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RefundId",
                table: "Transaction",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Refund",
                columns: table => new
                {
                    RefundId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OrderDetailId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    RefundStatus = table.Column<string>(type: "varchar", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Refund", x => x.RefundId);
                    table.ForeignKey(
                        name: "FK_Refund_Account_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Refund_OrderDetail_OrderDetailId",
                        column: x => x.OrderDetailId,
                        principalTable: "OrderDetail",
                        principalColumn: "OrderDetailId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_RefundId",
                table: "Transaction",
                column: "RefundId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Refund_MemberId",
                table: "Refund",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Refund_OrderDetailId",
                table: "Refund",
                column: "OrderDetailId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Refund_RefundId",
                table: "Transaction",
                column: "RefundId",
                principalTable: "Refund",
                principalColumn: "RefundId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Refund_RefundId",
                table: "Transaction");

            migrationBuilder.DropTable(
                name: "Refund");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_RefundId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "RefundId",
                table: "Transaction");
        }
    }
}
