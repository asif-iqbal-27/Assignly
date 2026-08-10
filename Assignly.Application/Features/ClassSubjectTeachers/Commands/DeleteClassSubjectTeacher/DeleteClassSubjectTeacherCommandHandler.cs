using Assignly.Application.Core.Abstractions;
using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using ErrorOr;

namespace Assignly.Application.Features.ClassSubjectTeachers.Commands.DeleteClassSubjectTeacher;

public sealed class DeleteClassSubjectTeacherCommandHandler : ICommandHandler<DeleteClassSubjectTeacherCommand, Deleted>
{
    private readonly IRepository<ClassSubjectTeacher> _classSubjectTeacherRepository;

    public DeleteClassSubjectTeacherCommandHandler(IRepository<ClassSubjectTeacher> classSubjectTeacherRepository)
    {
        _classSubjectTeacherRepository = classSubjectTeacherRepository;
    }

    public async Task<ErrorOr<Deleted>> Handle(DeleteClassSubjectTeacherCommand request, CancellationToken cancellationToken)
    {
        var classSubjectTeacher = await _classSubjectTeacherRepository.GetByIdAsync(request.Id, cancellationToken);
        if (classSubjectTeacher is null)
        {
            return ClassSubjectTeacherErrors.NotFound(request.Id);
        }

        _classSubjectTeacherRepository.Remove(classSubjectTeacher);
        await _classSubjectTeacherRepository.SaveChangesAsync(cancellationToken);

        return Result.Deleted;
    }
}
