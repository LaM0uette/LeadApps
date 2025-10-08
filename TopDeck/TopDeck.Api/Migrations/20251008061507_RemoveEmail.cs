using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TopDeck.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                schema: "data",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "data",
                table: "Users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }
    }
}
