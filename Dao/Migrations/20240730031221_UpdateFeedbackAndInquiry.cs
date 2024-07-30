using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFeedbackAndInquiry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedback_Account_MemberId",
                table: "Feedback");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedback_Shops_ShopId",
                table: "Feedback");

            migrationBuilder.DropForeignKey(
                name: "FK_Inquiry_Shops_ShopId",
                table: "Inquiry");

            migrationBuilder.DropIndex(
                name: "IX_Inquiry_ShopId",
                table: "Inquiry");

            migrationBuilder.DropIndex(
                name: "IX_Feedback_MemberId",
                table: "Feedback");

            migrationBuilder.DropIndex(
                name: "IX_Feedback_ShopId",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "ShopId",
                table: "Inquiry");

            migrationBuilder.DropColumn(
                name: "MemberId",
                table: "Feedback");

            migrationBuilder.RenameColumn(
                name: "ShopId",
                table: "Feedback",
                newName: "OrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_OrderDetailId",
                table: "Feedback",
                column: "OrderDetailId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedback_OrderDetail_OrderDetailId",
                table: "Feedback",
                column: "OrderDetailId",
                principalTable: "OrderDetail",
                principalColumn: "OrderDetailId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedback_OrderDetail_OrderDetailId",
                table: "Feedback");

            migrationBuilder.DropIndex(
                name: "IX_Feedback_OrderDetailId",
                table: "Feedback");

            migrationBuilder.RenameColumn(
                name: "OrderDetailId",
                table: "Feedback",
                newName: "ShopId");

            migrationBuilder.AddColumn<Guid>(
                name: "ShopId",
                table: "Inquiry",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "MemberId",
                table: "Feedback",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Inquiry_ShopId",
                table: "Inquiry",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_MemberId",
                table: "Feedback",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_ShopId",
                table: "Feedback",
                column: "ShopId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedback_Account_MemberId",
                table: "Feedback",
                column: "MemberId",
                principalTable: "Account",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedback_Shops_ShopId",
                table: "Feedback",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "ShopId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Inquiry_Shops_ShopId",
                table: "Inquiry",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "ShopId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
