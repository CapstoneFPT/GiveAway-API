using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStaffFromAuction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Auction_Account_StaffId",
                table: "Auction");

            migrationBuilder.DropForeignKey(
                name: "FK_Image_FashionItem_FashionItemItemId",
                table: "Image");

            migrationBuilder.DropIndex(
                name: "IX_Image_FashionItemItemId",
                table: "Image");

            migrationBuilder.DropIndex(
                name: "IX_Auction_StaffId",
                table: "Auction");

            migrationBuilder.DropColumn(
                name: "FashionItemItemId",
                table: "Image");

            migrationBuilder.DropColumn(
                name: "StaffId",
                table: "Auction");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "Image",
                newName: "FashionItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Image_FashionItemId",
                table: "Image",
                column: "FashionItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Image_FashionItem_FashionItemId",
                table: "Image",
                column: "FashionItemId",
                principalTable: "FashionItem",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Image_FashionItem_FashionItemId",
                table: "Image");

            migrationBuilder.DropIndex(
                name: "IX_Image_FashionItemId",
                table: "Image");

            migrationBuilder.RenameColumn(
                name: "FashionItemId",
                table: "Image",
                newName: "ItemId");

            migrationBuilder.AddColumn<Guid>(
                name: "FashionItemItemId",
                table: "Image",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StaffId",
                table: "Auction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Image_FashionItemItemId",
                table: "Image",
                column: "FashionItemItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Auction_StaffId",
                table: "Auction",
                column: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Auction_Account_StaffId",
                table: "Auction",
                column: "StaffId",
                principalTable: "Account",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Image_FashionItem_FashionItemItemId",
                table: "Image",
                column: "FashionItemItemId",
                principalTable: "FashionItem",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
