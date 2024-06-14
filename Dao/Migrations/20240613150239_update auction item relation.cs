using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class updateauctionitemrelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Auction_FashionItem_AuctionFashionItemItemId",
                table: "Auction");

            migrationBuilder.DropIndex(
                name: "IX_Auction_AuctionFashionItemItemId",
                table: "Auction");

            migrationBuilder.DropColumn(
                name: "AuctionFashionItemItemId",
                table: "Auction");

            migrationBuilder.RenameColumn(
                name: "AuctionItemId",
                table: "Auction",
                newName: "AuctionFashionItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Auction_AuctionFashionItemId",
                table: "Auction",
                column: "AuctionFashionItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Auction_FashionItem_AuctionFashionItemId",
                table: "Auction",
                column: "AuctionFashionItemId",
                principalTable: "FashionItem",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Auction_FashionItem_AuctionFashionItemId",
                table: "Auction");

            migrationBuilder.DropIndex(
                name: "IX_Auction_AuctionFashionItemId",
                table: "Auction");

            migrationBuilder.RenameColumn(
                name: "AuctionFashionItemId",
                table: "Auction",
                newName: "AuctionItemId");

            migrationBuilder.AddColumn<Guid>(
                name: "AuctionFashionItemItemId",
                table: "Auction",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Auction_AuctionFashionItemItemId",
                table: "Auction",
                column: "AuctionFashionItemItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Auction_FashionItem_AuctionFashionItemItemId",
                table: "Auction",
                column: "AuctionFashionItemItemId",
                principalTable: "FashionItem",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
