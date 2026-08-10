using Assignly.Domain.Entities;
using Assignly.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Assignly.Infrastructure.Data.Seed;

public static class DemoDataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        var credentials = new List<(string Role, string UserName, string Password)>();

        await SeedRolesAsync(roleManager);

        await SeedUserAsync(userManager, "admin", "admin@assignly.local", "Admin123!", "System Administrator", RoleType.Admin, null);
        credentials.Add(("Admin", "admin", "Admin123!"));

        var class9 = await SeedClassAsync(dbContext, "Class 9", "A", "Grade 9 - Section A");
        var class10 = await SeedClassAsync(dbContext, "Class 10", "A", "Grade 10 - Section A");

        var mathematics = await SeedSubjectAsync(dbContext, "Mathematics", class10.Id);
        var physics = await SeedSubjectAsync(dbContext, "Physics", class10.Id);
        var english = await SeedSubjectAsync(dbContext, "English", class9.Id);

        var teacher1 = await SeedUserAsync(userManager, "teacher1", "teacher1@assignly.local", "Teacher123!", "Rahim Sheikh", RoleType.Teacher, null);
        credentials.Add(("Teacher", "teacher1", "Teacher123!"));
        var teacher2 = await SeedUserAsync(userManager, "teacher2", "teacher2@assignly.local", "Teacher123!", "Karim Ahmed", RoleType.Teacher, null);
        credentials.Add(("Teacher", "teacher2", "Teacher123!"));

        await SeedClassSubjectTeacherAsync(dbContext, teacher1.Id, mathematics.Id);
        await SeedClassSubjectTeacherAsync(dbContext, teacher1.Id, physics.Id);
        await SeedClassSubjectTeacherAsync(dbContext, teacher2.Id, english.Id);

        var student1 = await SeedUserAsync(userManager, "student1", "student1@assignly.local", "Student123!", "Ayesha Rahman", RoleType.Student, class10.Id);
        credentials.Add(("Student", "student1", "Student123!"));
        await SeedUserAsync(userManager, "student2", "student2@assignly.local", "Student123!", "Fatima Islam", RoleType.Student, class10.Id);
        credentials.Add(("Student", "student2", "Student123!"));
        var student3 = await SeedUserAsync(userManager, "student3", "student3@assignly.local", "Student123!", "Kamal Hossain", RoleType.Student, class9.Id);
        credentials.Add(("Student", "student3", "Student123!"));
        var student4 = await SeedUserAsync(userManager, "student4", "student4@assignly.local", "Student123!", "Nusrat Jahan", RoleType.Student, class9.Id);
        credentials.Add(("Student", "student4", "Student123!"));

        await SeedAssignmentAsync(
            dbContext, "Algebra Basics", "Practice set covering linear equations.",
            mathematics.Id, class10.Id, teacher1.Id,
            DateTime.UtcNow.AddDays(10), 100, AssignmentStatus.Draft,
            allowLateSubmission: false, allowResubmission: true);

        var upcomingAssignment = await SeedAssignmentAsync(
            dbContext, "Newton's Laws", "Short answers on Newton's three laws of motion.",
            physics.Id, class10.Id, teacher1.Id,
            DateTime.UtcNow.AddDays(7), 50, AssignmentStatus.Published,
            allowLateSubmission: false, allowResubmission: true);

        await SeedSubmissionAsync(
            dbContext, upcomingAssignment.Id, student1.Id, "Newton's first law states that an object at rest stays at rest...",
            isLate: false, status: SubmissionStatus.Submitted, marks: null, feedback: null, gradedByTeacherId: null);

        var pastAssignment = await SeedAssignmentAsync(
            dbContext, "Essay Writing", "Write a 500-word essay on your favourite book.",
            english.Id, class9.Id, teacher2.Id,
            DateTime.UtcNow.AddDays(-3), 100, AssignmentStatus.Published,
            allowLateSubmission: true, allowResubmission: false);

        await SeedSubmissionAsync(
            dbContext, pastAssignment.Id, student3.Id, "My favourite book is To Kill a Mockingbird because...",
            isLate: true, status: SubmissionStatus.Late, marks: null, feedback: null, gradedByTeacherId: null);

        await SeedSubmissionAsync(
            dbContext, pastAssignment.Id, student4.Id, "The book that changed my life is 1984 because...",
            isLate: true, status: SubmissionStatus.Graded, marks: 85,
            feedback: "Well argued, minor grammar issues.", gradedByTeacherId: teacher2.Id);

        PrintCredentials(credentials);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        foreach (var roleName in new[] { "Admin", "Teacher", "Student" })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }
        }
    }

    private static async Task<ApplicationUser> SeedUserAsync(
        UserManager<ApplicationUser> userManager,
        string userName,
        string email,
        string password,
        string fullName,
        RoleType role,
        Guid? classId)
    {
        var existing = await userManager.FindByNameAsync(userName);
        if (existing is not null)
        {
            return existing;
        }

        var user = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            EmailConfirmed = true,
            FullName = fullName,
            Role = role,
            ClassId = classId
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to seed user '{userName}': {string.Join(" ", result.Errors.Select(e => e.Description))}");
        }

        await userManager.AddToRoleAsync(user, role.ToString());
        return user;
    }

    private static async Task<SchoolClass> SeedClassAsync(ApplicationDbContext dbContext, string name, string? section, string? description)
    {
        var existing = await dbContext.Classes.FirstOrDefaultAsync(c => c.Name == name);
        if (existing is not null)
        {
            return existing;
        }

        var schoolClass = new SchoolClass
        {
            Id = Guid.NewGuid(),
            Name = name,
            Section = section,
            Description = description
        };

        dbContext.Classes.Add(schoolClass);
        await dbContext.SaveChangesAsync();
        return schoolClass;
    }

    private static async Task<Subject> SeedSubjectAsync(ApplicationDbContext dbContext, string name, Guid classId)
    {
        var existing = await dbContext.Subjects.FirstOrDefaultAsync(s => s.Name == name && s.ClassId == classId);
        if (existing is not null)
        {
            return existing;
        }

        var subject = new Subject
        {
            Id = Guid.NewGuid(),
            Name = name,
            ClassId = classId
        };

        dbContext.Subjects.Add(subject);
        await dbContext.SaveChangesAsync();
        return subject;
    }

    private static async Task SeedClassSubjectTeacherAsync(ApplicationDbContext dbContext, Guid teacherId, Guid subjectId)
    {
        var exists = await dbContext.ClassSubjectTeachers
            .AnyAsync(t => t.TeacherId == teacherId && t.SubjectId == subjectId);

        if (exists)
        {
            return;
        }

        dbContext.ClassSubjectTeachers.Add(new ClassSubjectTeacher
        {
            Id = Guid.NewGuid(),
            TeacherId = teacherId,
            SubjectId = subjectId
        });

        await dbContext.SaveChangesAsync();
    }

    private static async Task<Assignment> SeedAssignmentAsync(
        ApplicationDbContext dbContext,
        string title,
        string description,
        Guid subjectId,
        Guid classId,
        Guid createdByTeacherId,
        DateTime deadline,
        int maxMarks,
        AssignmentStatus status,
        bool allowLateSubmission,
        bool allowResubmission)
    {
        var existing = await dbContext.Assignments.FirstOrDefaultAsync(a => a.Title == title);
        if (existing is not null)
        {
            return existing;
        }

        var now = DateTime.UtcNow;
        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            SubjectId = subjectId,
            ClassId = classId,
            CreatedByTeacherId = createdByTeacherId,
            Deadline = deadline,
            MaxMarks = maxMarks,
            Status = status,
            AllowLateSubmission = allowLateSubmission,
            AllowResubmission = allowResubmission,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.Assignments.Add(assignment);
        await dbContext.SaveChangesAsync();
        return assignment;
    }

    private static async Task SeedSubmissionAsync(
        ApplicationDbContext dbContext,
        Guid assignmentId,
        Guid studentId,
        string content,
        bool isLate,
        SubmissionStatus status,
        int? marks,
        string? feedback,
        Guid? gradedByTeacherId)
    {
        var exists = await dbContext.Submissions
            .AnyAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId);

        if (exists)
        {
            return;
        }

        dbContext.Submissions.Add(new Submission
        {
            Id = Guid.NewGuid(),
            AssignmentId = assignmentId,
            StudentId = studentId,
            Content = content,
            SubmittedAt = DateTime.UtcNow,
            IsLate = isLate,
            AttemptNumber = 1,
            Status = status,
            Marks = marks,
            Feedback = feedback,
            GradedByTeacherId = gradedByTeacherId,
            GradedAt = gradedByTeacherId is null ? null : DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();
    }

    private static void PrintCredentials(List<(string Role, string UserName, string Password)> credentials)
    {
        Console.WriteLine();
        Console.WriteLine("Assignly Demo Credentials");
        foreach (var credential in credentials)
        {
            Console.WriteLine($"  {credential.Role,-7} | username: {credential.UserName,-9} | password: {credential.Password}");
        }
        Console.WriteLine("=====================");
        Console.WriteLine();
    }
}
