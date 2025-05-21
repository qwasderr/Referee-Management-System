using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportSystem2.Migrations
{
    /// <inheritdoc />
    public partial class changetable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JudgeTrainingMaterial");

            migrationBuilder.DropTable(
                name: "TrainingMaterials");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrainingMaterials",
                columns: table => new
                {
                    TrainingMaterialId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingMaterials", x => x.TrainingMaterialId);
                });

            migrationBuilder.CreateTable(
                name: "JudgeTrainingMaterial",
                columns: table => new
                {
                    JudgesJudgeId = table.Column<int>(type: "int", nullable: false),
                    TrainingMaterialsTrainingMaterialId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JudgeTrainingMaterial", x => new { x.JudgesJudgeId, x.TrainingMaterialsTrainingMaterialId });
                    table.ForeignKey(
                        name: "FK_JudgeTrainingMaterial_Judges_JudgesJudgeId",
                        column: x => x.JudgesJudgeId,
                        principalTable: "Judges",
                        principalColumn: "JudgeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JudgeTrainingMaterial_TrainingMaterials_TrainingMaterialsTrainingMaterialId",
                        column: x => x.TrainingMaterialsTrainingMaterialId,
                        principalTable: "TrainingMaterials",
                        principalColumn: "TrainingMaterialId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JudgeTrainingMaterial_TrainingMaterialsTrainingMaterialId",
                table: "JudgeTrainingMaterial",
                column: "TrainingMaterialsTrainingMaterialId");
        }
    }
}
