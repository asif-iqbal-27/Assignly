using Assignly.Application.Features.Assignments.Commands.CreateAssignment;
using Assignly.Application.Features.Assignments.Commands.DeleteAssignment;
using Assignly.Application.Features.Assignments.Commands.PublishAssignment;
using Assignly.Application.Features.Assignments.Commands.UpdateAssignment;
using Assignly.Domain.Entities;
using Assignly.Domain.Enums;
using Assignly.Infrastructure.Data;
using Assignly.Infrastructure.Data.Repositories;
using Assignly.Tests.TestHelpers;
using ErrorOr;
using FluentAssertions;

namespace Assignly.Tests.Assignments;

// Plan §7 rule 5: a teacher can only create/update/delete/publish for subjects they hold
// via ClassSubjectTeacher, even if they know the assignment's row id.
public class TeacherOwnershipTests
{
    private sealed record SeedResult(
        Guid ClassId,
        Guid SubjectId,
        Guid OwnerTeacherId,
        Guid OtherTeacherId,
        Guid AssignmentId);

    private static SeedResult Seed(ApplicationDbContext db)
    {
        var classId = Guid.NewGuid();
        var subjectId = Guid.NewGuid();
        var ownerTeacherId = Guid.NewGuid();
        var otherTeacherId = Guid.NewGuid();
        var assignmentId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        db.Classes.Add(new SchoolClass { Id = classId, Name = "Class 10" });
        db.Subjects.Add(new Subject { Id = subjectId, Name = "Mathematics", ClassId = classId });
        db.Users.Add(new ApplicationUser { Id = ownerTeacherId, UserName = "owner", FullName = "Owner Teacher", Role = RoleType.Teacher });
        db.Users.Add(new ApplicationUser { Id = otherTeacherId, UserName = "other", FullName = "Other Teacher", Role = RoleType.Teacher });

        db.ClassSubjectTeachers.Add(new ClassSubjectTeacher
        {
            Id = Guid.NewGuid(),
            TeacherId = ownerTeacherId,
            SubjectId = subjectId
        });

        db.Assignments.Add(new Assignment
        {
            Id = assignmentId,
            Title = "Algebra Basics",
            Description = "Practice set covering linear equations.",
            SubjectId = subjectId,
            ClassId = classId,
            CreatedByTeacherId = ownerTeacherId,
            Deadline = now.AddDays(5),
            MaxMarks = 100,
            Status = AssignmentStatus.Draft,
            CreatedAt = now,
            UpdatedAt = now
        });

        db.SaveChanges();

        return new SeedResult(classId, subjectId, ownerTeacherId, otherTeacherId, assignmentId);
    }

    [Fact]
    public async Task CreateAssignment_ByTeacherWithoutSubjectAssignment_ReturnsForbidden()
    {
        using var db = TestDbContextFactory.Create();
        var seed = Seed(db);

        var handler = new CreateAssignmentCommandHandler(
            new Repository<Assignment>(db),
            new Repository<ClassSubjectTeacher>(db));

        var command = new CreateAssignmentCommand(
            "New Assignment", "Description", seed.SubjectId, seed.ClassId,
            DateTime.UtcNow.AddDays(5), 100, false, true, seed.OtherTeacherId);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public async Task UpdateAssignment_ByNonOwningTeacher_ReturnsForbidden_EvenKnowingTheAssignmentId()
    {
        using var db = TestDbContextFactory.Create();
        var seed = Seed(db);

        var handler = new UpdateAssignmentCommandHandler(
            new Repository<Assignment>(db),
            new Repository<ClassSubjectTeacher>(db));

        var command = new UpdateAssignmentCommand(
            seed.AssignmentId, "Updated Title", "Updated description",
            DateTime.UtcNow.AddDays(6), 90, true, true, seed.OtherTeacherId);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public async Task DeleteAssignment_ByNonOwningTeacher_ReturnsForbidden_EvenKnowingTheAssignmentId()
    {
        using var db = TestDbContextFactory.Create();
        var seed = Seed(db);

        var handler = new DeleteAssignmentCommandHandler(
            new Repository<Assignment>(db),
            new Repository<ClassSubjectTeacher>(db),
            new Repository<Submission>(db));

        var command = new DeleteAssignmentCommand(seed.AssignmentId, seed.OtherTeacherId);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public async Task PublishAssignment_ByNonOwningTeacher_ReturnsForbidden_EvenKnowingTheAssignmentId()
    {
        using var db = TestDbContextFactory.Create();
        var seed = Seed(db);

        var handler = new PublishAssignmentCommandHandler(
            new Repository<Assignment>(db),
            new Repository<ClassSubjectTeacher>(db));

        var command = new PublishAssignmentCommand(seed.AssignmentId, seed.OtherTeacherId);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public async Task PublishAssignment_ByOwningTeacher_Succeeds()
    {
        using var db = TestDbContextFactory.Create();
        var seed = Seed(db);

        var handler = new PublishAssignmentCommandHandler(
            new Repository<Assignment>(db),
            new Repository<ClassSubjectTeacher>(db));

        var command = new PublishAssignmentCommand(seed.AssignmentId, seed.OwnerTeacherId);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Status.Should().Be(nameof(AssignmentStatus.Published));
    }
}
