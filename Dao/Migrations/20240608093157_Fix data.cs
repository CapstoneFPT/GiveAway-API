using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dao.Migrations
{
    /// <inheritdoc />
    public partial class Fixdata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "BankName",
                table: "Wallet",
                type: "varchar",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "BankAccountNumber",
                table: "Wallet",
                type: "varchar",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
            
            migrationBuilder.Sql(
                @"ALTER TABLE ""Account"" ALTER COLUMN ""PasswordSalt"" TYPE TEXT USING ""PasswordSalt""::TEXT; ");

            migrationBuilder.Sql(
                @"ALTER TABLE ""Account"" ALTER COLUMN ""PasswordHash"" TYPE TEXT USING ""PasswordHash""::TEXT; ");

            migrationBuilder.Sql(
                @"ALTER TABLE ""Account"" ALTER COLUMN ""PasswordSalt"" TYPE BYTEA USING ""PasswordSalt""::bytea; ");

            migrationBuilder.Sql(
                @"ALTER TABLE ""Account"" ALTER COLUMN ""PasswordHash"" TYPE BYTEA USING ""PasswordHash""::bytea; ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "BankName",
                table: "Wallet",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BankAccountNumber",
                table: "Wallet",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordSalt",
                table: "Account",
                type: "text",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "bytea");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Account",
                type: "text",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "bytea");
        }
    }
}
