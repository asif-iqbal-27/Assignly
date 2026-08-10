using Assignly.Application.Features.Submissions.Queries.GetSubmissionById;
using Assignly.Domain.Entities;
using Assignly.Domain.Enums;
using Assignly.Infrastructure.Data;
using Assignly.Infrastructure.Data.Repositories;
using Assignly.Tests.TestHelpers;
using ErrorOr;
using FluentAssertions;

namespace Assignly.Tests.Submissions;

// Plan §7 rule 8: a student can only ever see their own submissions and marks, never
// another student's — including GetSubmissionById by direct id. Return NotFound, not
// Forbidden, so ids aren't enumerable. The same policy is extended here to Teacher
// visibility, for consistency with how GetAssignmentById already handles this.
public class SubmissionVisibilityTests
{
    private sealed record SeedResult(
        Guid OwningStudentId,
        Guid OtherStudentId,
        Guid OtherTeacherId,
        Guid SubmissionId);

    private static SeedResult Seed(ApplicationDbContext db)
    {
        var classId = Guid.NewGuid();
        var subjectId = Guid.NewGuid();
        var owningTeacherId = Guid.NewGuid();
        var otherTeacherId = Guid.NewGuid();
        var owningStudentId = Guid.NewGuid();
        var otherStudentId = Guid.NewGuid();
        var assignmentId = Guid.NewGuid();
        var submissionId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        db.Classes.Add(new SchoolClass { Id = classId, Name = "Class 10" });
        db.Subjects.Add(new Subject { Id = subjectId, Name = "Mathematics", ClassId = classId });
        db.Users.Add(new ApplicationUser { Id = owningTeacherId, UserName = "owner", FullName = "Owner Teacher", Role = RoleType.Teacher });
        db.Users.Add(new ApplicationUser { Id = otherTeacherId, UserName = "other", FullName = "Other Teacher", Role = RoleType.Teacher });
        db.Users.Add(new ApplicationUser { Id = owningStudentId, UserName = "owningStudent", FullName = "Owning Student", Role = RoleType.Student, ClassId = classId });
        db.Users.Add(new ApplicationUser { Id = otherStudentId, UserName = "otherStudent", FullName = "Other Student", Role = RoleType.Student, ClassId = classId });
        db.ClassSubjectTeachers.Add(new ClassSubjectTeacher { Id = Guid.NewGuid(), TeacherId = owningTeacherId, SubjectId = subjectId });

        db.Assignments.Add(new Assignment
        {
            Id = assignmentId, Title = "Algebra", Description = "d", SubjectId = subjectId, ClassId = classId,
            CreatedByTeacherId = owningTeacherId, Deadline = now.AddDays(5), MaxMarks = 100, Status = AssignmentStatus.Published,
            AllowLateSubmission = false, AllowResubmission = true, CreatedAt = now, UpdatedAt = now
        });

        db.Submissions.Add(new Submission
        {
            Id = submissionId, AssignmentId = assignmentId, StudentId = owningStudentId,
            Content = "My answer", SubmittedAt = now, IsLate = false, AttemptNumber = 1, Status = SubmissionStatus.Submitted
        });

        db.SaveChanges();

        return new SeedResult(owningStudentId, otherStudentId, otherTeacherId, submissionId);
    }

    private static GetSubmissionByIdQueryHandler CreateHandler(ApplicationDbContext db) =>
        new(new Repository<Submission>(db), new Repository<ClassSubjectTeacher>(db));

    [Fact]
    public async Task GetSubmissionById_AsOwningStudent_ReturnsSubmission()
    {
        using var db = TestDbContextFactory.Create();
        var seed = Seed(db);
        var handler = CreateHandler(db);

        var result = await handler.Handle(
            new GetSubmissionByIdQuery(seed.SubmissionId, seed.OwningStudentId, RoleType.Student),
            CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Id.Should().Be(seed.SubmissionId);
    }

    [Fact]
    public async Task GetSubmissionById_AsDifferentStudent_ReturnsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var seed = Seed(db);
        var handler = CreateHandler(db);

        var result = await handler.Handle(
            new GetSubmissionByIdQuery(seed.SubmissionId, seed.OtherStudentId, RoleType.Student),
            CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task GetSubmissionById_AsAdmin_ReturnsSubmission()
    {
        using var db = TestDbContextFactory.Create();
        var seed = Seed(db);
        var handler = CreateHandler(db);

        var result = await handler.Handle(
            new GetSubmissionByIdQuery(seed.SubmissionId, Guid.NewGuid(), RoleType.Admin),
            CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Id.Should().Be(seed.SubmissionId);
    }

    [Fact]
    public async Task GetSubmissionById_AsNonOwningTeacher_ReturnsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var seed = Seed(db);
        var handler = CreateHandler(db);

        var result = await handler.Handle(
            new GetSubmissionByIdQuery(seed.SubmissionId, seed.OtherTeacherId, RoleType.Teacher),
            CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }
}
