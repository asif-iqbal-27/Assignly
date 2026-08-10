using Assignly.Application.Features.Assignments.Queries.GetAssignmentById;
using Assignly.Application.Features.Assignments.Queries.GetAssignments;
using Assignly.Domain.Entities;
using Assignly.Domain.Enums;
using Assignly.Infrastructure.Data;
using Assignly.Infrastructure.Data.Repositories;
using Assignly.Tests.TestHelpers;
using ErrorOr;
using FluentAssertions;

namespace Assignly.Tests.Assignments;

// Plan §7 rule 4: a Draft assignment is invisible to students; only Published
// assignments appear in a student's list — including via GetAssignmentById by direct id.
public class StudentVisibilityTests
{
    private sealed record SeedResult(
        Guid ClassId,
        Guid OtherClassId,
        Guid DraftId,
        Guid PublishedId,
        Guid OtherClassPublishedId);

    private static SeedResult Seed(ApplicationDbContext db)
    {
        var classId = Guid.NewGuid();
        var otherClassId = Guid.NewGuid();
        var subjectId = Guid.NewGuid();
        var teacherId = Guid.NewGuid();

        db.Classes.Add(new SchoolClass { Id = classId, Name = "Class 10" });
        db.Classes.Add(new SchoolClass { Id = otherClassId, Name = "Class 9" });
        db.Subjects.Add(new Subject { Id = subjectId, Name = "Mathematics", ClassId = classId });
        db.Users.Add(new ApplicationUser { Id = teacherId, UserName = "teacher1", FullName = "Teacher One", Role = RoleType.Teacher });

        var draftId = Guid.NewGuid();
        var publishedId = Guid.NewGuid();
        var otherClassPublishedId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        db.Assignments.Add(new Assignment
        {
            Id = draftId,
            Title = "Draft Assignment",
            Description = "A draft assignment that must stay invisible to students.",
            SubjectId = subjectId,
            ClassId = classId,
            CreatedByTeacherId = teacherId,
            Deadline = now.AddDays(5),
            MaxMarks = 100,
            Status = AssignmentStatus.Draft,
            CreatedAt = now,
            UpdatedAt = now
        });

        db.Assignments.Add(new Assignment
        {
            Id = publishedId,
            Title = "Published Assignment",
            Description = "A published assignment in the student's own class.",
            SubjectId = subjectId,
            ClassId = classId,
            CreatedByTeacherId = teacherId,
            Deadline = now.AddDays(5),
            MaxMarks = 100,
            Status = AssignmentStatus.Published,
            CreatedAt = now,
            UpdatedAt = now
        });

        db.Assignments.Add(new Assignment
        {
            Id = otherClassPublishedId,
            Title = "Other Class Published",
            Description = "A published assignment belonging to a different class.",
            SubjectId = subjectId,
            ClassId = otherClassId,
            CreatedByTeacherId = teacherId,
            Deadline = now.AddDays(5),
            MaxMarks = 100,
            Status = AssignmentStatus.Published,
            CreatedAt = now,
            UpdatedAt = now
        });

        db.SaveChanges();

        return new SeedResult(classId, otherClassId, draftId, publishedId, otherClassPublishedId);
    }

    [Fact]
    public async Task GetAssignments_ForStudent_ReturnsOnlyPublishedAssignmentsInOwnClass()
    {
        using var db = TestDbContextFactory.Create();
        var seed = Seed(db);

        var handler = new GetAssignmentsQueryHandler(
            new Repository<Assignment>(db),
            new Repository<ClassSubjectTeacher>(db));

        var result = await handler.Handle(
            new GetAssignmentsQuery(Guid.NewGuid(), RoleType.Student, seed.ClassId),
            CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Select(a => a.Id).Should().BeEquivalentTo([seed.PublishedId]);
    }

    [Fact]
    public async Task GetAssignmentById_ForStudent_OnDraftAssignmentInOwnClass_ReturnsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var seed = Seed(db);

        var handler = new GetAssignmentByIdQueryHandler(
            new Repository<Assignment>(db),
            new Repository<ClassSubjectTeacher>(db));

        var result = await handler.Handle(
            new GetAssignmentByIdQuery(seed.DraftId, Guid.NewGuid(), RoleType.Student, seed.ClassId),
            CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task GetAssignmentById_ForStudent_OnPublishedAssignmentInAnotherClass_ReturnsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var seed = Seed(db);

        var handler = new GetAssignmentByIdQueryHandler(
            new Repository<Assignment>(db),
            new Repository<ClassSubjectTeacher>(db));

        var result = await handler.Handle(
            new GetAssignmentByIdQuery(seed.OtherClassPublishedId, Guid.NewGuid(), RoleType.Student, seed.ClassId),
            CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task GetAssignmentById_ForStudent_OnPublishedAssignmentInOwnClass_ReturnsAssignment()
    {
        using var db = TestDbContextFactory.Create();
        var seed = Seed(db);

        var handler = new GetAssignmentByIdQueryHandler(
            new Repository<Assignment>(db),
            new Repository<ClassSubjectTeacher>(db));

        var result = await handler.Handle(
            new GetAssignmentByIdQuery(seed.PublishedId, Guid.NewGuid(), RoleType.Student, seed.ClassId),
            CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Id.Should().Be(seed.PublishedId);
    }
}
