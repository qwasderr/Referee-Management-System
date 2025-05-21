using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportSystem2.Migrations
{
    /// <inheritdoc />
    public partial class matches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location",
                table: "Matches");

            migrationBuilder.AddColumn<int>(
                name: "TournamentId",
                table: "Matches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TournamentRoundId",
                table: "Matches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Matches_TournamentId",
                table: "Matches",
                column: "TournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_TournamentRoundId",
                table: "Matches",
                column: "TournamentRoundId");

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_TournamentRound_TournamentRoundId",
                table: "Matches",
                column: "TournamentRoundId",
                principalTable: "TournamentRound",
                principalColumn: "RoundId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Tournament_TournamentId",
                table: "Matches",
                column: "TournamentId",
                principalTable: "Tournament",
                principalColumn: "TournamentId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matches_TournamentRound_TournamentRoundId",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Tournament_TournamentId",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Matches_TournamentId",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Matches_TournamentRoundId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "TournamentId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "TournamentRoundId",
                table: "Matches");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Matches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
