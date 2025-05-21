using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportSystem2.Migrations
{
    /// <inheritdoc />
    public partial class judge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsJudge",
                table: "AspNetUsers",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsJudge",
                table: "AspNetUsers");
        }
    }
}
