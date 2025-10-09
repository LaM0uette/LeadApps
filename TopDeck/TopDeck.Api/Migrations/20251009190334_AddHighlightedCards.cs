using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TopDeck.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddHighlightedCards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CollectionCode",
                schema: "data",
                table: "DeckCards",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<bool>(
                name: "IsHighlighted",
                schema: "data",
                table: "DeckCards",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHighlighted",
                schema: "data",
                table: "DeckCards");

            migrationBuilder.AlterColumn<string>(
                name: "CollectionCode",
                schema: "data",
                table: "DeckCards",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);
        }
    }
}
