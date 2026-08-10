using Assignly.Application.Features.Submissions.Commands.UpdateSubmission;
using Assignly.Application.Features.Submissions.Queries.GetMySubmissions;
using Assignly.Domain.Entities;
using Assignly.Domain.Enums;
using Assignly.Infrastructure.Data;
using Assignly.Infrastructure.Data.Repositories;
using Assignly.Tests.TestHelpers;
using ErrorOr;
using FluentAssertions;

namespace Assignly.Tests.Submissions;

// Plan §7 rule 3: resubmission requires AllowResubmission == true AND the deadline not
// yet passed. AllowLateSubmission does NOT extend the resubmission window — the two
// flags are independent. Resubmission inserts a new row with an incremented
// AttemptNumber; queries return only the latest attempt.
public class ResubmissionTests
{
    private sealed record SeedResult(
        Guid StudentId,
        Guid ResubmittableAssignmentId,
        Guid ResubmittableSubmissionId,
        Guid NoResubmitSubmissionId,
        Guid PastDeadlineAllowLateSubmissionId);

    private static SeedResult Seed(ApplicationDbContext db)
    {
        var classId = Guid.NewGuid();
        var subjectId = Guid.NewGuid();
        var teacherId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        db.Classes.Add(new SchoolClass { Id = classId, Name = "Class 10" });
        db.Subjects.Add(new Subject { Id = subjectId, Name = "Mathematics", ClassId = classId });
        db.Users.Add(new ApplicationUser { Id = teacherId, UserName = "teacher1", FullName = "Teacher One", Role = RoleType.Teacher });
        db.Users.Add(new ApplicationUser { Id = studentId, UserName = "student1", FullName = "Student One", Role = RoleType.Student, ClassId = classId });

        var resubmittableAssignmentId = Guid.NewGuid();
        var noResubmitAssignmentId = Guid.NewGuid();
        var pastDeadlineAllowLateAssignmentId = Guid.NewGuid();

        db.Assignments.Add(new Assignment
        {
            Id = resubmittableAssignmentId, Title = "Resubmittable", Description = "d", SubjectId = subjectId, ClassId = classId,
            CreatedByTeacherId = teacherId, Deadline = now.AddDays(5), MaxMarks = 100, Status = AssignmentStatus.Published,
            AllowLateSubmission = false, AllowResubmission = true, CreatedAt = now, UpdatedAt = now
        });

        db.Assignments.Add(new Assignment
        {
            Id = noResubmitAssignmentId, Title = "No Resubmit", Description = "d", SubjectId = subjectId, ClassId = classId,
            CreatedByTeacherId = teacherId, Deadline = now.AddDays(5), MaxMarks = 100, Status = AssignmentStatus.Published,
            AllowLateSubmission = false, AllowResubmission = false, CreatedAt = now, UpdatedAt = now
        });

        db.Assignments.Add(new Assignment
        {
            Id = pastDeadlineAllowLateAssignmentId, Title = "Past Deadline Allow Late", Description = "d", SubjectId = subjectId, ClassId = classId,
            CreatedByTeacherId = teacherId, Deadline = now.AddDays(-2), MaxMarks = 100, Status = AssignmentStatus.Published,
            AllowLateSubmission = true, AllowResubmission = true, CreatedAt = now, UpdatedAt = now
        });

        var resubmittableSubmissionId = Guid.NewGuid();
        var noResubmitSubmissionId = Guid.NewGuid();
        var pastDeadlineAllowLateSubmissionId = Guid.NewGuid();

        db.Submissions.Add(new Submission
        {
            Id = resubmittableSubmissionId, AssignmentId = resubmittableAssignmentId, StudentId = studentId,
            Content = "First attempt", SubmittedAt = now, IsLate = false, AttemptNumber = 1, Status = SubmissionStatus.Submitted
        });

        db.Submissions.Add(new Submission
        {
            Id = noResubmitSubmissionId, AssignmentId = noResubmitAssignmentId, StudentId = studentId,
            Content = "First attempt", SubmittedAt = now, IsLate = false, AttemptNumber = 1, Status = SubmissionStatus.Submitted
        });

        db.Submissions.Add(new Submission
        {
            Id = pastDeadlineAllowLateSubmissionId, AssignmentId = pastDeadlineAllowLateAssignmentId, StudentId = studentId,
            Content = "First attempt (late)", SubmittedAt = now, IsLate = true, AttemptNumber = 1, Status = SubmissionStatus.Late
        });

        db.SaveChanges();

        return new SeedResult(
            studentId,
            resubmittableAssignmentId, resubmittableSubmissionId,
            noResubmitSubmissionId,
            pastDeadlineAllowLateSubmissionId);
    }

    private static UpdateSubmissionCommandHandler CreateUpdateHandler(ApplicationDbContext db) =>
        new(new Repository<Submission>(db), new Repository<Assignment>(db));

    [Fact]
    public async Task UpdateSubmission_WhenAllowedAndBeforeDeadline_CreatesNewAttemptWithIncrementedNumber()
    {
        using var db = TestDbContextFactory.Create();
        var seed = Seed(db);
        var handler = CreateUpdateHandler(db);

        var result = await handler.Handle(
            new UpdateSubmissionCommand(seed.ResubmittableSubmissionId, "Second attempt", seed.StudentId),
            CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.AttemptNumber.Should().Be(2);
        result.Value.Id.Should().NotBe(seed.ResubmittableSubmissionId);

        var totalAttempts = db.Submissions.Count(s => s.AssignmentId == seed.ResubmittableAssignmentId && s.StudentId == seed.StudentId);
        totalAttempts.Should().Be(2);
    }

    [Fact]
    public async Task UpdateSubmission_WhenAllowResubmissionFalse_ReturnsConflict()
    {
        using var db = TestDbContextFactory.Create();
        var seed = Seed(db);
        var handler = CreateUpdateHandler(db);

        var result = await handler.Handle(
            new UpdateSubmissionCommand(seed.NoResubmitSubmissionId, "Second attempt", seed.StudentId),
            CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task UpdateSubmission_AfterDeadline_ReturnsConflict_EvenWhenAllowLateSubmissionTrue()
    {
        using var db = TestDbContextFactory.Create();
        var seed = Seed(db);
        var handler = CreateUpdateHandler(db);

        var result = await handler.Handle(
            new UpdateSubmissionCommand(seed.PastDeadlineAllowLateSubmissionId, "Second attempt", seed.StudentId),
            CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task GetMySubmissions_AfterResubmission_ReturnsOnlyLatestAttempt()
    {
        using var db = TestDbContextFactory.Create();
        var seed = Seed(db);
        var updateHandler = CreateUpdateHandler(db);

        await updateHandler.Handle(
            new UpdateSubmissionCommand(seed.ResubmittableSubmissionId, "Second attempt", seed.StudentId),
            CancellationToken.None);

        var queryHandler = new GetMySubmissionsQueryHandler(new Repository<Submission>(db));
        var result = await queryHandler.Handle(new GetMySubmissionsQuery(seed.StudentId), CancellationToken.None);

        result.IsError.Should().BeFalse();
        var forResubmittable = result.Value.Single(s => s.AssignmentId == seed.ResubmittableAssignmentId);
        forResubmittable.AttemptNumber.Should().Be(2);
        forResubmittable.Content.Should().Be("Second attempt");
    }
}
