using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TopDeck.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserUuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                schema: "data",
                table: "Users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<Guid>(
                name: "Uuid",
                schema: "data",
                table: "Users",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Users_Uuid",
                schema: "data",
                table: "Users",
                column: "Uuid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Uuid",
                schema: "data",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Uuid",
                schema: "data",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                schema: "data",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);
        }
    }
}
