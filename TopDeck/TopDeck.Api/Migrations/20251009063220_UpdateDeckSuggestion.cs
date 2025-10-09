using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TopDeck.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDeckSuggestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddedCardIds",
                schema: "data",
                table: "DeckSuggestions");

            migrationBuilder.DropColumn(
                name: "RemovedCardIds",
                schema: "data",
                table: "DeckSuggestions");

            migrationBuilder.CreateTable(
                name: "DeckSuggestionAddedCards",
                schema: "data",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeckSuggestionId = table.Column<int>(type: "integer", nullable: false),
                    CollectionCode = table.Column<string>(type: "text", nullable: false),
                    CollectionNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeckSuggestionAddedCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeckSuggestionAddedCards_DeckSuggestions_DeckSuggestionId",
                        column: x => x.DeckSuggestionId,
                        principalSchema: "data",
                        principalTable: "DeckSuggestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeckSuggestionRemovedCards",
                schema: "data",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeckSuggestionId = table.Column<int>(type: "integer", nullable: false),
                    CollectionCode = table.Column<string>(type: "text", nullable: false),
                    CollectionNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeckSuggestionRemovedCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeckSuggestionRemovedCards_DeckSuggestions_DeckSuggestionId",
                        column: x => x.DeckSuggestionId,
                        principalSchema: "data",
                        principalTable: "DeckSuggestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeckSuggestionAddedCards_DeckSuggestionId_CollectionCode_Co~",
                schema: "data",
                table: "DeckSuggestionAddedCards",
                columns: new[] { "DeckSuggestionId", "CollectionCode", "CollectionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeckSuggestionRemovedCards_DeckSuggestionId_CollectionCode_~",
                schema: "data",
                table: "DeckSuggestionRemovedCards",
                columns: new[] { "DeckSuggestionId", "CollectionCode", "CollectionNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeckSuggestionAddedCards",
                schema: "data");

            migrationBuilder.DropTable(
                name: "DeckSuggestionRemovedCards",
                schema: "data");

            migrationBuilder.AddColumn<int[]>(
                name: "AddedCardIds",
                schema: "data",
                table: "DeckSuggestions",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);

            migrationBuilder.AddColumn<int[]>(
                name: "RemovedCardIds",
                schema: "data",
                table: "DeckSuggestions",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);
        }
    }
}
