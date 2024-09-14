using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class ChangeRelationFeedbackToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedback_OrderLineItem_OrderLineItemId",
                table: "Feedback");

            migrationBuilder.RenameColumn(
                name: "OrderLineItemId",
                table: "Feedback",
                newName: "OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_Feedback_OrderLineItemId",
                table: "Feedback",
                newName: "IX_Feedback_OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedback_Order_OrderId",
                table: "Feedback",
                column: "OrderId",
                principalTable: "Order",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedback_Order_OrderId",
                table: "Feedback");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "Feedback",
                newName: "OrderLineItemId");

            migrationBuilder.RenameIndex(
                name: "IX_Feedback_OrderId",
                table: "Feedback",
                newName: "IX_Feedback_OrderLineItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedback_OrderLineItem_OrderLineItemId",
                table: "Feedback",
                column: "OrderLineItemId",
                principalTable: "OrderLineItem",
                principalColumn: "OrderLineItemId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
