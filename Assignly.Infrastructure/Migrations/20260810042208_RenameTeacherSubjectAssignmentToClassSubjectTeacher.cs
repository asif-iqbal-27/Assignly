using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assignly.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameTeacherSubjectAssignmentToClassSubjectTeacher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "TeacherSubjectAssignments",
                newName: "ClassSubjectTeachers");

            migrationBuilder.RenameIndex(
                name: "IX_TeacherSubjectAssignments_SubjectId",
                table: "ClassSubjectTeachers",
                newName: "IX_ClassSubjectTeachers_SubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_TeacherSubjectAssignments_TeacherId_SubjectId",
                table: "ClassSubjectTeachers",
                newName: "IX_ClassSubjectTeachers_TeacherId_SubjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_ClassSubjectTeachers_SubjectId",
                table: "ClassSubjectTeachers",
                newName: "IX_TeacherSubjectAssignments_SubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_ClassSubjectTeachers_TeacherId_SubjectId",
                table: "ClassSubjectTeachers",
                newName: "IX_TeacherSubjectAssignments_TeacherId_SubjectId");

            migrationBuilder.RenameTable(
                name: "ClassSubjectTeachers",
                newName: "TeacherSubjectAssignments");
        }
    }
}
