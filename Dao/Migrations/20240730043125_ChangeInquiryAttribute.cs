using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class ChangeInquiryAttribute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Inquiry");

            migrationBuilder.DropColumn(
                name: "Fullname",
                table: "Inquiry");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Inquiry");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Inquiry",
                type: "varchar",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Inquiry");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Inquiry",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Fullname",
                table: "Inquiry",
                type: "varchar",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Inquiry",
                type: "varchar",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }
    }
}
