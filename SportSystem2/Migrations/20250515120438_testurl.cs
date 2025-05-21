using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportSystem2.Migrations
{
    /// <inheritdoc />
    public partial class testurl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Test",
                newName: "TestUrl");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TestUrl",
                table: "Test",
                newName: "Description");
        }
    }
}
