using Assignly.Application.Features.Submissions.Commands.GradeSubmission;
using Assignly.Domain.Entities;
using Assignly.Domain.Enums;
using Assignly.Infrastructure.Data;
using Assignly.Infrastructure.Data.Repositories;
using Assignly.Tests.TestHelpers;
using ErrorOr;
using FluentAssertions;

namespace Assignly.Tests.Submissions;

// Plan §7 rules 5 and 6: grading requires the teacher to hold the subject via
// ClassSubjectTeacher, even knowing the submission's row id, and Marks must be
// >= 0 and <= Assignment.MaxMarks.
public class GradingTests
{
    private sealed record SeedResult(Guid TeacherId, Guid OtherTeacherId, Guid SubmissionId, int MaxMarks);

    private static SeedResult Seed(ApplicationDbContext db)
    {
        var classId = Guid.NewGuid();
        var subjectId = Guid.NewGuid();
        var teacherId = Guid.NewGuid();
        var otherTeacherId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var assignmentId = Guid.NewGuid();
        var submissionId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        const int maxMarks = 100;

        db.Classes.Add(new SchoolClass { Id = classId, Name = "Class 10" });
        db.Subjects.Add(new Subject { Id = subjectId, Name = "Mathematics", ClassId = classId });
        db.Users.Add(new ApplicationUser { Id = teacherId, UserName = "teacher1", FullName = "Teacher One", Role = RoleType.Teacher });
        db.Users.Add(new ApplicationUser { Id = otherTeacherId, UserName = "teacher2", FullName = "Teacher Two", Role = RoleType.Teacher });
        db.Users.Add(new ApplicationUser { Id = studentId, UserName = "student1", FullName = "Student One", Role = RoleType.Student, ClassId = classId });
        db.ClassSubjectTeachers.Add(new ClassSubjectTeacher { Id = Guid.NewGuid(), TeacherId = teacherId, SubjectId = subjectId });

        db.Assignments.Add(new Assignment
        {
            Id = assignmentId, Title = "Algebra", Description = "d", SubjectId = subjectId, ClassId = classId,
            CreatedByTeacherId = teacherId, Deadline = now.AddDays(5), MaxMarks = maxMarks, Status = AssignmentStatus.Published,
            AllowLateSubmission = false, AllowResubmission = true, CreatedAt = now, UpdatedAt = now
        });

        db.Submissions.Add(new Submission
        {
            Id = submissionId, AssignmentId = assignmentId, StudentId = studentId,
            Content = "My answer", SubmittedAt = now, IsLate = false, AttemptNumber = 1, Status = SubmissionStatus.Submitted
        });

        db.SaveChanges();

        return new SeedResult(teacherId, otherTeacherId, submissionId, maxMarks);
    }

    private static GradeSubmissionCommandHandler CreateHandler(ApplicationDbContext db) =>
        new(new Repository<Submission>(db), new Repository<Assignment>(db), new Repository<ClassSubjectTeacher>(db));

    [Fact]
    public async Task GradeSubmission_MarksWithinRange_Succeeds()
    {
        using var db = TestDbContextFactory.Create();
        var seed = Seed(db);
        var handler = CreateHandler(db);

        var result = await handler.Handle(
            new GradeSubmissionCommand(seed.SubmissionId, 85, "Good work.", seed.TeacherId),
            CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Marks.Should().Be(85);
        result.Value.Status.Should().Be(nameof(SubmissionStatus.Graded));
    }

    [Fact]
    public async Task GradeSubmission_MarksAboveMaxMarks_ReturnsValidationError()
    {
        using var db = TestDbContextFactory.Create();
        var seed = Seed(db);
        var handler = CreateHandler(db);

        var result = await handler.Handle(
            new GradeSubmissionCommand(seed.SubmissionId, seed.MaxMarks + 1, "Too high.", seed.TeacherId),
            CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task GradeSubmission_NegativeMarks_ReturnsValidationError()
    {
        using var db = TestDbContextFactory.Create();
        var seed = Seed(db);
        var handler = CreateHandler(db);

        var result = await handler.Handle(
            new GradeSubmissionCommand(seed.SubmissionId, -5, "Negative.", seed.TeacherId),
            CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task GradeSubmission_ByNonOwningTeacher_ReturnsForbidden_EvenKnowingTheSubmissionId()
    {
        using var db = TestDbContextFactory.Create();
        var seed = Seed(db);
        var handler = CreateHandler(db);

        var result = await handler.Handle(
            new GradeSubmissionCommand(seed.SubmissionId, 85, "Good work.", seed.OtherTeacherId),
            CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
    }
}
