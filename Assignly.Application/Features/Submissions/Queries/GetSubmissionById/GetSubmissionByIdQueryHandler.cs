using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;
using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using Assignly.Domain.Enums;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Assignly.Application.Features.Submissions.Queries.GetSubmissionById;

public sealed class GetSubmissionByIdQueryHandler : IQueryHandler<GetSubmissionByIdQuery, SubmissionDto>
{
    private readonly IRepository<Submission> _submissionRepository;
    private readonly IRepository<ClassSubjectTeacher> _classSubjectTeacherRepository;

    public GetSubmissionByIdQueryHandler(
        IRepository<Submission> submissionRepository,
        IRepository<ClassSubjectTeacher> classSubjectTeacherRepository)
    {
        _submissionRepository = submissionRepository;
        _classSubjectTeacherRepository = classSubjectTeacherRepository;
    }

    public async Task<ErrorOr<SubmissionDto>> Handle(GetSubmissionByIdQuery request, CancellationToken cancellationToken)
    {
        var summary = await _submissionRepository.Query()
            .Where(s => s.Id == request.Id)
            .Select(s => new { s.StudentId, SubjectId = s.Assignment.SubjectId })
            .FirstOrDefaultAsync(cancellationToken);

        if (summary is null)
        {
            return SubmissionErrors.NotFound(request.Id);
        }

        // Same NotFound-for-everything policy as GetAssignmentById — a student (or a
        // teacher not holding the subject) must not be able to distinguish "doesn't
        // exist" from "exists but isn't yours" by probing ids.
        var visible = request.RequestingUserRole switch
        {
            RoleType.Admin => true,
            RoleType.Student => summary.StudentId == request.RequestingUserId,
            RoleType.Teacher => await _classSubjectTeacherRepository.Query()
                .AnyAsync(t => t.TeacherId == request.RequestingUserId && t.SubjectId == summary.SubjectId, cancellationToken),
            _ => false
        };

        if (!visible)
        {
            return SubmissionErrors.NotFound(request.Id);
        }

        return await _submissionRepository.Query()
            .Where(s => s.Id == request.Id)
            .Select(SubmissionMappings.ToDto)
            .FirstAsync(cancellationToken);
    }
}
