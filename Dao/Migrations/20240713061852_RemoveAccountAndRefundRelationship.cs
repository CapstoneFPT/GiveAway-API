using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAccountAndRefundRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Refund_Account_MemberId",
                table: "Refund");

            migrationBuilder.DropIndex(
                name: "IX_Refund_MemberId",
                table: "Refund");

            migrationBuilder.DropColumn(
                name: "MemberId",
                table: "Refund");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MemberId",
                table: "Refund",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Refund_MemberId",
                table: "Refund",
                column: "MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Refund_Account_MemberId",
                table: "Refund",
                column: "MemberId",
                principalTable: "Account",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
