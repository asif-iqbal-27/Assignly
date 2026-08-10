using Assignly.Application.Core.Abstractions;
using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Assignly.Application.Features.Classes.Commands.DeleteClass;

public sealed class DeleteClassCommandHandler : ICommandHandler<DeleteClassCommand, Deleted>
{
    private readonly IRepository<SchoolClass> _classRepository;
    private readonly IRepository<Subject> _subjectRepository;
    private readonly IRepository<ApplicationUser> _userRepository;
    private readonly IRepository<Assignment> _assignmentRepository;

    public DeleteClassCommandHandler(
        IRepository<SchoolClass> classRepository,
        IRepository<Subject> subjectRepository,
        IRepository<ApplicationUser> userRepository,
        IRepository<Assignment> assignmentRepository)
    {
        _classRepository = classRepository;
        _subjectRepository = subjectRepository;
        _userRepository = userRepository;
        _assignmentRepository = assignmentRepository;
    }

    public async Task<ErrorOr<Deleted>> Handle(DeleteClassCommand request, CancellationToken cancellationToken)
    {
        var schoolClass = await _classRepository.GetByIdAsync(request.Id, cancellationToken);
        if (schoolClass is null)
        {
            return ClassErrors.NotFound(request.Id);
        }

        var hasSubjects = await _subjectRepository.Query().AnyAsync(s => s.ClassId == request.Id, cancellationToken);
        var hasStudents = await _userRepository.Query().AnyAsync(u => u.ClassId == request.Id, cancellationToken);
        var hasAssignments = await _assignmentRepository.Query().AnyAsync(a => a.ClassId == request.Id, cancellationToken);

        if (hasSubjects || hasStudents || hasAssignments)
        {
            return ClassErrors.HasDependents;
        }

        _classRepository.Remove(schoolClass);
        await _classRepository.SaveChangesAsync(cancellationToken);

        return Result.Deleted;
    }
}
