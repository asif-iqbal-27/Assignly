using System.Linq.Expressions;
using Assignly.Application.Dtos;
using Assignly.Domain.Entities;

namespace Assignly.Application.Features.Submissions;

public static class SubmissionMappings
{
    public static readonly Expression<Func<Submission, SubmissionDto>> ToDto = s => new SubmissionDto
    {
        Id = s.Id,
        AssignmentId = s.AssignmentId,
        AssignmentTitle = s.Assignment.Title,
        StudentId = s.StudentId,
        StudentName = s.Student.FullName,
        Content = s.Content,
        FileUrl = s.FileUrl,
        SubmittedAt = s.SubmittedAt,
        IsLate = s.IsLate,
        AttemptNumber = s.AttemptNumber,
        Status = s.Status.ToString(),
        Marks = s.Marks,
        Feedback = s.Feedback,
        GradedByTeacherId = s.GradedByTeacherId,
        GradedByTeacherName = s.GradedByTeacher != null ? s.GradedByTeacher.FullName : null,
        GradedAt = s.GradedAt
    };
}
