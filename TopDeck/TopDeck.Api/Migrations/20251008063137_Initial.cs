using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TopDeck.Api.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "data");

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "data",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OAuthProvider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OAuthId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Decks",
                schema: "data",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatorId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CardIds = table.Column<int[]>(type: "integer[]", nullable: false),
                    EnergyIds = table.Column<int[]>(type: "integer[]", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Decks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Decks_Users_CreatorId",
                        column: x => x.CreatorId,
                        principalSchema: "data",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeckDislikes",
                schema: "data",
                columns: table => new
                {
                    DeckId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeckDislikes", x => new { x.DeckId, x.UserId });
                    table.ForeignKey(
                        name: "FK_DeckDislikes_Decks_DeckId",
                        column: x => x.DeckId,
                        principalSchema: "data",
                        principalTable: "Decks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeckDislikes_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "data",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeckLikes",
                schema: "data",
                columns: table => new
                {
                    DeckId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeckLikes", x => new { x.DeckId, x.UserId });
                    table.ForeignKey(
                        name: "FK_DeckLikes_Decks_DeckId",
                        column: x => x.DeckId,
                        principalSchema: "data",
                        principalTable: "Decks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeckLikes_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "data",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeckSuggestions",
                schema: "data",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SuggestorId = table.Column<int>(type: "integer", nullable: false),
                    DeckId = table.Column<int>(type: "integer", nullable: false),
                    AddedCardIds = table.Column<int[]>(type: "integer[]", nullable: false),
                    RemovedCardIds = table.Column<int[]>(type: "integer[]", nullable: false),
                    AddedEnergyIds = table.Column<int[]>(type: "integer[]", nullable: false),
                    RemovedEnergyIds = table.Column<int[]>(type: "integer[]", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeckSuggestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeckSuggestions_Decks_DeckId",
                        column: x => x.DeckId,
                        principalSchema: "data",
                        principalTable: "Decks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeckSuggestions_Users_SuggestorId",
                        column: x => x.SuggestorId,
                        principalSchema: "data",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeckSuggestionDislikes",
                schema: "data",
                columns: table => new
                {
                    DeckSuggestionId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeckSuggestionDislikes", x => new { x.DeckSuggestionId, x.UserId });
                    table.ForeignKey(
                        name: "FK_DeckSuggestionDislikes_DeckSuggestions_DeckSuggestionId",
                        column: x => x.DeckSuggestionId,
                        principalSchema: "data",
                        principalTable: "DeckSuggestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeckSuggestionDislikes_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "data",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeckSuggestionLikes",
                schema: "data",
                columns: table => new
                {
                    DeckSuggestionId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeckSuggestionLikes", x => new { x.DeckSuggestionId, x.UserId });
                    table.ForeignKey(
                        name: "FK_DeckSuggestionLikes_DeckSuggestions_DeckSuggestionId",
                        column: x => x.DeckSuggestionId,
                        principalSchema: "data",
                        principalTable: "DeckSuggestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeckSuggestionLikes_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "data",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeckDislikes_UserId",
                schema: "data",
                table: "DeckDislikes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DeckLikes_UserId",
                schema: "data",
                table: "DeckLikes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Decks_Code",
                schema: "data",
                table: "Decks",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Decks_CreatorId",
                schema: "data",
                table: "Decks",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_DeckSuggestionDislikes_UserId",
                schema: "data",
                table: "DeckSuggestionDislikes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DeckSuggestionLikes_UserId",
                schema: "data",
                table: "DeckSuggestionLikes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DeckSuggestions_DeckId",
                schema: "data",
                table: "DeckSuggestions",
                column: "DeckId");

            migrationBuilder.CreateIndex(
                name: "IX_DeckSuggestions_SuggestorId",
                schema: "data",
                table: "DeckSuggestions",
                column: "SuggestorId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_OAuthProvider_OAuthId",
                schema: "data",
                table: "Users",
                columns: new[] { "OAuthProvider", "OAuthId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeckDislikes",
                schema: "data");

            migrationBuilder.DropTable(
                name: "DeckLikes",
                schema: "data");

            migrationBuilder.DropTable(
                name: "DeckSuggestionDislikes",
                schema: "data");

            migrationBuilder.DropTable(
                name: "DeckSuggestionLikes",
                schema: "data");

            migrationBuilder.DropTable(
                name: "DeckSuggestions",
                schema: "data");

            migrationBuilder.DropTable(
                name: "Decks",
                schema: "data");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "data");
        }
    }
}
