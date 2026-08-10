using Assignly.Application.Core.Abstractions;
using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Assignly.Application.Features.Subjects.Commands.DeleteSubject;

public sealed class DeleteSubjectCommandHandler : ICommandHandler<DeleteSubjectCommand, Deleted>
{
    private readonly IRepository<Subject> _subjectRepository;
    private readonly IRepository<ClassSubjectTeacher> _classSubjectTeacherRepository;
    private readonly IRepository<Assignment> _assignmentRepository;

    public DeleteSubjectCommandHandler(
        IRepository<Subject> subjectRepository,
        IRepository<ClassSubjectTeacher> classSubjectTeacherRepository,
        IRepository<Assignment> assignmentRepository)
    {
        _subjectRepository = subjectRepository;
        _classSubjectTeacherRepository = classSubjectTeacherRepository;
        _assignmentRepository = assignmentRepository;
    }

    public async Task<ErrorOr<Deleted>> Handle(DeleteSubjectCommand request, CancellationToken cancellationToken)
    {
        var subject = await _subjectRepository.GetByIdAsync(request.Id, cancellationToken);
        if (subject is null)
        {
            return SubjectErrors.NotFound(request.Id);
        }

        var hasClassSubjectTeachers = await _classSubjectTeacherRepository.Query()
            .AnyAsync(t => t.SubjectId == request.Id, cancellationToken);
        var hasAssignments = await _assignmentRepository.Query()
            .AnyAsync(a => a.SubjectId == request.Id, cancellationToken);

        if (hasClassSubjectTeachers || hasAssignments)
        {
            return SubjectErrors.HasDependents;
        }

        _subjectRepository.Remove(subject);
        await _subjectRepository.SaveChangesAsync(cancellationToken);

        return Result.Deleted;
    }
}
