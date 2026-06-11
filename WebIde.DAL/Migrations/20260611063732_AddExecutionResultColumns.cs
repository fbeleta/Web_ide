using System;
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
            // Recreate as unique (one submission → one summary execution result)
            migrationBuilder.DropIndex(
                name: "IX_Submissions_ExecutionResultId",
                table: "Submissions");

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
                name: "IX_Submissions_ExecutionResultId",
                table: "Submissions",
                column: "ExecutionResultId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExecutionResults_TestCaseId",
                table: "ExecutionResults",
                column: "TestCaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExecutionResults_TestCases_TestCaseId",
                table: "ExecutionResults",
                column: "TestCaseId",
                principalTable: "TestCases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExecutionResults_TestCases_TestCaseId",
                table: "ExecutionResults");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_ExecutionResultId",
                table: "Submissions");

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

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ExecutionResultId",
                table: "Submissions",
                column: "ExecutionResultId");
        }
    }
}
