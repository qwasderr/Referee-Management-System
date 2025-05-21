using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportSystem2.Migrations
{
    /// <inheritdoc />
    public partial class deletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Judges_AspNetUsers_ApplicationUserId",
                table: "Judges");

            migrationBuilder.DropForeignKey(
                name: "FK_MatchAnalyses_Judges_CreatedByJudgeId",
                table: "MatchAnalyses");

            migrationBuilder.DropForeignKey(
                name: "FK_MatchResults_Matches_MatchId",
                table: "MatchResults");

            migrationBuilder.DropForeignKey(
                name: "FK_MatchResults_Teams_TeamId",
                table: "MatchResults");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayerEvents_Matches_MatchId",
                table: "PlayerEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayerEvents_Players_PlayerId",
                table: "PlayerEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_Players_Teams_TeamId",
                table: "Players");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamStandings_Teams_TeamId",
                table: "TeamStandings");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamStandings_Tournaments_TournamentId",
                table: "TeamStandings");

            migrationBuilder.DropForeignKey(
                name: "FK_TestResults_Judges_JudgeId",
                table: "TestResults");

            migrationBuilder.DropForeignKey(
                name: "FK_TestResults_Tests_TestId",
                table: "TestResults");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentRounds_Tournaments_TournamentId",
                table: "TournamentRounds");

            migrationBuilder.AddForeignKey(
                name: "FK_Judges_AspNetUsers_ApplicationUserId",
                table: "Judges",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MatchAnalyses_Judges_CreatedByJudgeId",
                table: "MatchAnalyses",
                column: "CreatedByJudgeId",
                principalTable: "Judges",
                principalColumn: "JudgeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MatchResults_Matches_MatchId",
                table: "MatchResults",
                column: "MatchId",
                principalTable: "Matches",
                principalColumn: "MatchId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MatchResults_Teams_TeamId",
                table: "MatchResults",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerEvents_Matches_MatchId",
                table: "PlayerEvents",
                column: "MatchId",
                principalTable: "Matches",
                principalColumn: "MatchId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerEvents_Players_PlayerId",
                table: "PlayerEvents",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "PlayerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Players_Teams_TeamId",
                table: "Players",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TeamStandings_Teams_TeamId",
                table: "TeamStandings",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TeamStandings_Tournaments_TournamentId",
                table: "TeamStandings",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "TournamentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TestResults_Judges_JudgeId",
                table: "TestResults",
                column: "JudgeId",
                principalTable: "Judges",
                principalColumn: "JudgeId",
                onDelete: ReferentialAction.Cascade);

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
                name: "FK_Judges_AspNetUsers_ApplicationUserId",
                table: "Judges");

            migrationBuilder.DropForeignKey(
                name: "FK_MatchAnalyses_Judges_CreatedByJudgeId",
                table: "MatchAnalyses");

            migrationBuilder.DropForeignKey(
                name: "FK_MatchResults_Matches_MatchId",
                table: "MatchResults");

            migrationBuilder.DropForeignKey(
                name: "FK_MatchResults_Teams_TeamId",
                table: "MatchResults");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayerEvents_Matches_MatchId",
                table: "PlayerEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayerEvents_Players_PlayerId",
                table: "PlayerEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_Players_Teams_TeamId",
                table: "Players");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamStandings_Teams_TeamId",
                table: "TeamStandings");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamStandings_Tournaments_TournamentId",
                table: "TeamStandings");

            migrationBuilder.DropForeignKey(
                name: "FK_TestResults_Judges_JudgeId",
                table: "TestResults");

            migrationBuilder.DropForeignKey(
                name: "FK_TestResults_Tests_TestId",
                table: "TestResults");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentRounds_Tournaments_TournamentId",
                table: "TournamentRounds");

            migrationBuilder.AddForeignKey(
                name: "FK_Judges_AspNetUsers_ApplicationUserId",
                table: "Judges",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MatchAnalyses_Judges_CreatedByJudgeId",
                table: "MatchAnalyses",
                column: "CreatedByJudgeId",
                principalTable: "Judges",
                principalColumn: "JudgeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MatchResults_Matches_MatchId",
                table: "MatchResults",
                column: "MatchId",
                principalTable: "Matches",
                principalColumn: "MatchId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MatchResults_Teams_TeamId",
                table: "MatchResults",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerEvents_Matches_MatchId",
                table: "PlayerEvents",
                column: "MatchId",
                principalTable: "Matches",
                principalColumn: "MatchId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerEvents_Players_PlayerId",
                table: "PlayerEvents",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "PlayerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Players_Teams_TeamId",
                table: "Players",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TeamStandings_Teams_TeamId",
                table: "TeamStandings",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TeamStandings_Tournaments_TournamentId",
                table: "TeamStandings",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "TournamentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TestResults_Judges_JudgeId",
                table: "TestResults",
                column: "JudgeId",
                principalTable: "Judges",
                principalColumn: "JudgeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TestResults_Tests_TestId",
                table: "TestResults",
                column: "TestId",
                principalTable: "Tests",
                principalColumn: "TestId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentRounds_Tournaments_TournamentId",
                table: "TournamentRounds",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "TournamentId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
