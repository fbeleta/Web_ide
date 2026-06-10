using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebIde.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddExecutionResultColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PeakMemoryKb",
                table: "ExecutionResults",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SubmissionId",
                table: "ExecutionResults",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TestCaseId",
                table: "ExecutionResults",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Verdict",
                table: "ExecutionResults",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WallTimeMs",
                table: "ExecutionResults",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ExecutionResults_TestCaseId",
                table: "ExecutionResults",
                column: "TestCaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExecutionResults_TestCaseId",
                table: "ExecutionResults");

            migrationBuilder.DropColumn(
                name: "PeakMemoryKb",
                table: "ExecutionResults");

            migrationBuilder.DropColumn(
                name: "SubmissionId",
                table: "ExecutionResults");

            migrationBuilder.DropColumn(
                name: "TestCaseId",
                table: "ExecutionResults");

            migrationBuilder.DropColumn(
                name: "Verdict",
                table: "ExecutionResults");

            migrationBuilder.DropColumn(
                name: "WallTimeMs",
                table: "ExecutionResults");
        }
    }
}
