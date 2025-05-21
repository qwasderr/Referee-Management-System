using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportSystem2.Migrations
{
    /// <inheritdoc />
    public partial class news : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NewsPosts_AspNetUsers_AuthorId",
                table: "NewsPosts");

            migrationBuilder.DropTable(
                name: "Penalties");

            migrationBuilder.DropIndex(
                name: "IX_NewsPosts_AuthorId",
                table: "NewsPosts");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "NewsPosts");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "NewsPosts",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoPath",
                table: "NewsPosts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NewsPosts_ApplicationUserId",
                table: "NewsPosts",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_NewsPosts_AspNetUsers_ApplicationUserId",
                table: "NewsPosts",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NewsPosts_AspNetUsers_ApplicationUserId",
                table: "NewsPosts");

            migrationBuilder.DropIndex(
                name: "IX_NewsPosts_ApplicationUserId",
                table: "NewsPosts");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "NewsPosts");

            migrationBuilder.DropColumn(
                name: "PhotoPath",
                table: "NewsPosts");

            migrationBuilder.AddColumn<string>(
                name: "AuthorId",
                table: "NewsPosts",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Penalties",
                columns: table => new
                {
                    PenaltyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MatchId = table.Column<int>(type: "int", nullable: false),
                    PenalizedPlayerId = table.Column<int>(type: "int", nullable: true),
                    PenalizedTeamId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Minute = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Penalties", x => x.PenaltyId);
                    table.ForeignKey(
                        name: "FK_Penalties_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "MatchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Penalties_Players_PenalizedPlayerId",
                        column: x => x.PenalizedPlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId");
                    table.ForeignKey(
                        name: "FK_Penalties_Teams_PenalizedTeamId",
                        column: x => x.PenalizedTeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NewsPosts_AuthorId",
                table: "NewsPosts",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Penalties_MatchId",
                table: "Penalties",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Penalties_PenalizedPlayerId",
                table: "Penalties",
                column: "PenalizedPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Penalties_PenalizedTeamId",
                table: "Penalties",
                column: "PenalizedTeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_NewsPosts_AspNetUsers_AuthorId",
                table: "NewsPosts",
                column: "AuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
