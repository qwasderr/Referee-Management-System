using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportSystem2.Migrations
{
    /// <inheritdoc />
    public partial class matchanal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrl",
                table: "MatchAnalyses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastEditedAt",
                table: "MatchAnalyses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinuteFrom",
                table: "MatchAnalyses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinuteTo",
                table: "MatchAnalyses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "MatchAnalyses",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentUrl",
                table: "MatchAnalyses");

            migrationBuilder.DropColumn(
                name: "LastEditedAt",
                table: "MatchAnalyses");

            migrationBuilder.DropColumn(
                name: "MinuteFrom",
                table: "MatchAnalyses");

            migrationBuilder.DropColumn(
                name: "MinuteTo",
                table: "MatchAnalyses");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "MatchAnalyses");
        }
    }
}
