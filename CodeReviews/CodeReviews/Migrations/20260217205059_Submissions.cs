using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeReviews.Migrations
{
    /// <inheritdoc />
    public partial class Submissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Submission_SubmissionId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Submission_AppUsers_StudentAppUserId",
                table: "Submission");

            migrationBuilder.DropForeignKey(
                name: "FK_Submission_Assignments_AssignmentId",
                table: "Submission");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Submission",
                table: "Submission");

            migrationBuilder.RenameTable(
                name: "Submission",
                newName: "Submissions");

            migrationBuilder.RenameIndex(
                name: "IX_Submission_StudentAppUserId",
                table: "Submissions",
                newName: "IX_Submissions_StudentAppUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Submission_AssignmentId",
                table: "Submissions",
                newName: "IX_Submissions_AssignmentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Submissions",
                table: "Submissions",
                column: "SubmissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Submissions_SubmissionId",
                table: "Reviews",
                column: "SubmissionId",
                principalTable: "Submissions",
                principalColumn: "SubmissionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_AppUsers_StudentAppUserId",
                table: "Submissions",
                column: "StudentAppUserId",
                principalTable: "AppUsers",
                principalColumn: "AppUserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Assignments_AssignmentId",
                table: "Submissions",
                column: "AssignmentId",
                principalTable: "Assignments",
                principalColumn: "AssignmentId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Submissions_SubmissionId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_AppUsers_StudentAppUserId",
                table: "Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Assignments_AssignmentId",
                table: "Submissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Submissions",
                table: "Submissions");

            migrationBuilder.RenameTable(
                name: "Submissions",
                newName: "Submission");

            migrationBuilder.RenameIndex(
                name: "IX_Submissions_StudentAppUserId",
                table: "Submission",
                newName: "IX_Submission_StudentAppUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Submissions_AssignmentId",
                table: "Submission",
                newName: "IX_Submission_AssignmentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Submission",
                table: "Submission",
                column: "SubmissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Submission_SubmissionId",
                table: "Reviews",
                column: "SubmissionId",
                principalTable: "Submission",
                principalColumn: "SubmissionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Submission_AppUsers_StudentAppUserId",
                table: "Submission",
                column: "StudentAppUserId",
                principalTable: "AppUsers",
                principalColumn: "AppUserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Submission_Assignments_AssignmentId",
                table: "Submission",
                column: "AssignmentId",
                principalTable: "Assignments",
                principalColumn: "AssignmentId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
