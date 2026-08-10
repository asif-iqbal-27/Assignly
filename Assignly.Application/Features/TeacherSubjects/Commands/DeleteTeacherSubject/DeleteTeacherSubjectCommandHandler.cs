using Assignly.Application.Core.Abstractions;
using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using ErrorOr;

namespace Assignly.Application.Features.TeacherSubjects.Commands.DeleteTeacherSubject;

public sealed class DeleteTeacherSubjectCommandHandler : ICommandHandler<DeleteTeacherSubjectCommand, Deleted>
{
    private readonly IRepository<TeacherSubjectAssignment> _teacherSubjectAssignmentRepository;

    public DeleteTeacherSubjectCommandHandler(IRepository<TeacherSubjectAssignment> teacherSubjectAssignmentRepository)
    {
        _teacherSubjectAssignmentRepository = teacherSubjectAssignmentRepository;
    }

    public async Task<ErrorOr<Deleted>> Handle(DeleteTeacherSubjectCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _teacherSubjectAssignmentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (assignment is null)
        {
            return TeacherSubjectErrors.NotFound(request.Id);
        }

        _teacherSubjectAssignmentRepository.Remove(assignment);
        await _teacherSubjectAssignmentRepository.SaveChangesAsync(cancellationToken);

        return Result.Deleted;
    }
}
