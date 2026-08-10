using Assignly.Application.Features.Submissions.Commands.SubmitAssignment;
using Assignly.Domain.Entities;
using Assignly.Domain.Enums;
using Assignly.Infrastructure.Data;
using Assignly.Infrastructure.Data.Repositories;
using Assignly.Tests.TestHelpers;
using ErrorOr;
using FluentAssertions;

namespace Assignly.Tests.Submissions;

// Plan §7 rules 1 and 2: a student cannot submit outside their own class, and a
// submission after the deadline is rejected unless AllowLateSubmission == true (in
// which case it's accepted and flagged IsLate = true). Also verifies students can
// never submit to a Draft assignment.
public class SubmitAssignmentTests
{
    private sealed record SeedResult(
        Guid StudentInClassAId,
        Guid StudentInClassBId,
        Guid OnTimeAssignmentId,
        Guid PastDeadlineNoLateAssignmentId,
        Guid PastDeadlineAllowLateAssignmentId,
        Guid DraftAssignmentId);

    private static SeedResult Seed(ApplicationDbContext db)
    {
        var classA = Guid.NewGuid();
        var classB = Guid.NewGuid();
        var subjectId = Guid.NewGuid();
        var teacherId = Guid.NewGuid();
        var studentInClassAId = Guid.NewGuid();
        var studentInClassBId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        db.Classes.Add(new SchoolClass { Id = classA, Name = "Class A" });
        db.Classes.Add(new SchoolClass { Id = classB, Name = "Class B" });
        db.Subjects.Add(new Subject { Id = subjectId, Name = "Mathematics", ClassId = classA });
        db.Users.Add(new ApplicationUser { Id = teacherId, UserName = "teacher1", FullName = "Teacher One", Role = RoleType.Teacher });
        db.Users.Add(new ApplicationUser { Id = studentInClassAId, UserName = "studentA", FullName = "Student A", Role = RoleType.Student, ClassId = classA });
        db.Users.Add(new ApplicationUser { Id = studentInClassBId, UserName = "studentB", FullName = "Student B", Role = RoleType.Student, ClassId = classB });

        var onTimeAssignmentId = Guid.NewGuid();
        var pastNoLateId = Guid.NewGuid();
        var pastAllowLateId = Guid.NewGuid();
        var draftId = Guid.NewGuid();

        db.Assignments.Add(new Assignment
        {
            Id = onTimeAssignmentId, Title = "On Time", Description = "d", SubjectId = subjectId, ClassId = classA,
            CreatedByTeacherId = teacherId, Deadline = now.AddDays(5), MaxMarks = 100, Status = AssignmentStatus.Published,
            AllowLateSubmission = false, AllowResubmission = true, CreatedAt = now, UpdatedAt = now
        });

        db.Assignments.Add(new Assignment
        {
            Id = pastNoLateId, Title = "Past No Late", Description = "d", SubjectId = subjectId, ClassId = classA,
            CreatedByTeacherId = teacherId, Deadline = now.AddDays(-2), MaxMarks = 100, Status = AssignmentStatus.Published,
            AllowLateSubmission = false, AllowResubmission = true, CreatedAt = now, UpdatedAt = now
        });

        db.Assignments.Add(new Assignment
        {
            Id = pastAllowLateId, Title = "Past Allow Late", Description = "d", SubjectId = subjectId, ClassId = classA,
            CreatedByTeacherId = teacherId, Deadline = now.AddDays(-2), MaxMarks = 100, Status = AssignmentStatus.Published,
            AllowLateSubmission = true, AllowResubmission = true, CreatedAt = now, UpdatedAt = now
        });

        db.Assignments.Add(new Assignment
        {
            Id = draftId, Title = "Draft", Description = "d", SubjectId = subjectId, ClassId = classA,
            CreatedByTeacherId = teacherId, Deadline = now.AddDays(5), MaxMarks = 100, Status = AssignmentStatus.Draft,
            AllowLateSubmission = false, AllowResubmission = true, CreatedAt = now, UpdatedAt = now
        });

        db.SaveChanges();

        return new SeedResult(studentInClassAId, studentInClassBId, onTimeAssignmentId, pastNoLateId, pastAllowLateId, draftId);
    }

    private static SubmitAssignmentCommandHandler CreateHandler(ApplicationDbContext db) =>
        new(new Repository<Assignment>(db), new Repository<ApplicationUser>(db), new Repository<Submission>(db));

    [Fact]
    public async Task SubmitAssignment_InOwnClassOnTime_Succeeds()
    {
        using var db = TestDbContextFactory.Create();
        var seed = Seed(db);
        var handler = CreateHandler(db);

        var result = await handler.Handle(
            new SubmitAssignmentCommand(seed.OnTimeAssignmentId, "My answer", seed.StudentInClassAId),
            CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.IsLate.Should().BeFalse();
        result.Value.AttemptNumber.Should().Be(1);
    }

    [Fact]
    public async Task SubmitAssignment_InDifferentClass_ReturnsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var seed = Seed(db);
        var handler = CreateHandler(db);

        var result = await handler.Handle(
            new SubmitAssignmentCommand(seed.OnTimeAssignmentId, "My answer", seed.StudentInClassBId),
            CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task SubmitAssignment_ToDraftAssignment_ReturnsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var seed = Seed(db);
        var handler = CreateHandler(db);

        var result = await handler.Handle(
            new SubmitAssignmentCommand(seed.DraftAssignmentId, "My answer", seed.StudentInClassAId),
            CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task SubmitAssignment_AfterDeadlineWithoutAllowLateSubmission_ReturnsConflict()
    {
        using var db = TestDbContextFactory.Create();
        var seed = Seed(db);
        var handler = CreateHandler(db);

        var result = await handler.Handle(
            new SubmitAssignmentCommand(seed.PastDeadlineNoLateAssignmentId, "My answer", seed.StudentInClassAId),
            CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task SubmitAssignment_AfterDeadlineWithAllowLateSubmission_SucceedsAndMarksLate()
    {
        using var db = TestDbContextFactory.Create();
        var seed = Seed(db);
        var handler = CreateHandler(db);

        var result = await handler.Handle(
            new SubmitAssignmentCommand(seed.PastDeadlineAllowLateAssignmentId, "My answer", seed.StudentInClassAId),
            CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.IsLate.Should().BeTrue();
        result.Value.Status.Should().Be(nameof(SubmissionStatus.Late));
    }
}
