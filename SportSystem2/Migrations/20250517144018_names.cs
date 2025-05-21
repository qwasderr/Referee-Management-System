using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportSystem2.Migrations
{
    /// <inheritdoc />
    public partial class names : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matches_TournamentRound_TournamentRoundId",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Tournament_TournamentId",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_TestResults_Test_TestId",
                table: "TestResults");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentRound_Tournament_TournamentId",
                table: "TournamentRound");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TournamentRound",
                table: "TournamentRound");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tournament",
                table: "Tournament");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Test",
                table: "Test");

            migrationBuilder.RenameTable(
                name: "TournamentRound",
                newName: "TournamentRounds");

            migrationBuilder.RenameTable(
                name: "Tournament",
                newName: "Tournaments");

            migrationBuilder.RenameTable(
                name: "Test",
                newName: "Tests");

            migrationBuilder.RenameIndex(
                name: "IX_TournamentRound_TournamentId",
                table: "TournamentRounds",
                newName: "IX_TournamentRounds_TournamentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TournamentRounds",
                table: "TournamentRounds",
                column: "RoundId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tournaments",
                table: "Tournaments",
                column: "TournamentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tests",
                table: "Tests",
                column: "TestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_TournamentRounds_TournamentRoundId",
                table: "Matches",
                column: "TournamentRoundId",
                principalTable: "TournamentRounds",
                principalColumn: "RoundId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Tournaments_TournamentId",
                table: "Matches",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "TournamentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TestResults_Tests_TestId",
                table: "TestResults",
                column: "TestId",
                principalTable: "Tests",
                principalColumn: "TestId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentRounds_Tournaments_TournamentId",
                table: "TournamentRounds",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "TournamentId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matches_TournamentRounds_TournamentRoundId",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Tournaments_TournamentId",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_TestResults_Tests_TestId",
                table: "TestResults");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentRounds_Tournaments_TournamentId",
                table: "TournamentRounds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tournaments",
                table: "Tournaments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TournamentRounds",
                table: "TournamentRounds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tests",
                table: "Tests");

            migrationBuilder.RenameTable(
                name: "Tournaments",
                newName: "Tournament");

            migrationBuilder.RenameTable(
                name: "TournamentRounds",
                newName: "TournamentRound");

            migrationBuilder.RenameTable(
                name: "Tests",
                newName: "Test");

            migrationBuilder.RenameIndex(
                name: "IX_TournamentRounds_TournamentId",
                table: "TournamentRound",
                newName: "IX_TournamentRound_TournamentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tournament",
                table: "Tournament",
                column: "TournamentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TournamentRound",
                table: "TournamentRound",
                column: "RoundId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Test",
                table: "Test",
                column: "TestId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_TestResults_Test_TestId",
                table: "TestResults",
                column: "TestId",
                principalTable: "Test",
                principalColumn: "TestId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentRound_Tournament_TournamentId",
                table: "TournamentRound",
                column: "TournamentId",
                principalTable: "Tournament",
                principalColumn: "TournamentId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
