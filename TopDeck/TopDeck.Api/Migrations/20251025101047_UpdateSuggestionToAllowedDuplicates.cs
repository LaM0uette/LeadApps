using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TopDeck.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSuggestionToAllowedDuplicates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DeckSuggestionRemovedCards_DeckSuggestionId_CollectionCode_~",
                schema: "data",
                table: "DeckSuggestionRemovedCards");

            migrationBuilder.DropIndex(
                name: "IX_DeckSuggestionAddedCards_DeckSuggestionId_CollectionCode_Co~",
                schema: "data",
                table: "DeckSuggestionAddedCards");

            migrationBuilder.CreateIndex(
                name: "IX_DeckSuggestionRemovedCards_DeckSuggestionId_CollectionCode_~",
                schema: "data",
                table: "DeckSuggestionRemovedCards",
                columns: new[] { "DeckSuggestionId", "CollectionCode", "CollectionNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_DeckSuggestionAddedCards_DeckSuggestionId_CollectionCode_Co~",
                schema: "data",
                table: "DeckSuggestionAddedCards",
                columns: new[] { "DeckSuggestionId", "CollectionCode", "CollectionNumber" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DeckSuggestionRemovedCards_DeckSuggestionId_CollectionCode_~",
                schema: "data",
                table: "DeckSuggestionRemovedCards");

            migrationBuilder.DropIndex(
                name: "IX_DeckSuggestionAddedCards_DeckSuggestionId_CollectionCode_Co~",
                schema: "data",
                table: "DeckSuggestionAddedCards");

            migrationBuilder.CreateIndex(
                name: "IX_DeckSuggestionRemovedCards_DeckSuggestionId_CollectionCode_~",
                schema: "data",
                table: "DeckSuggestionRemovedCards",
                columns: new[] { "DeckSuggestionId", "CollectionCode", "CollectionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeckSuggestionAddedCards_DeckSuggestionId_CollectionCode_Co~",
                schema: "data",
                table: "DeckSuggestionAddedCards",
                columns: new[] { "DeckSuggestionId", "CollectionCode", "CollectionNumber" },
                unique: true);
        }
    }
}
