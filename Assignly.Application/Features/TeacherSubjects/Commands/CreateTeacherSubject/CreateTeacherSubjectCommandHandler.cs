using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;
using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Assignly.Application.Features.TeacherSubjects.Commands.CreateTeacherSubject;

public sealed class CreateTeacherSubjectCommandHandler : ICommandHandler<CreateTeacherSubjectCommand, TeacherSubjectDto>
{
    private readonly IRepository<TeacherSubjectAssignment> _teacherSubjectAssignmentRepository;
    private readonly IRepository<ApplicationUser> _userRepository;
    private readonly IRepository<Subject> _subjectRepository;

    public CreateTeacherSubjectCommandHandler(
        IRepository<TeacherSubjectAssignment> teacherSubjectAssignmentRepository,
        IRepository<ApplicationUser> userRepository,
        IRepository<Subject> subjectRepository)
    {
        _teacherSubjectAssignmentRepository = teacherSubjectAssignmentRepository;
        _userRepository = userRepository;
        _subjectRepository = subjectRepository;
    }

    public async Task<ErrorOr<TeacherSubjectDto>> Handle(CreateTeacherSubjectCommand request, CancellationToken cancellationToken)
    {
        var alreadyAssigned = await _teacherSubjectAssignmentRepository.Query()
            .AnyAsync(t => t.TeacherId == request.TeacherId && t.SubjectId == request.SubjectId, cancellationToken);

        if (alreadyAssigned)
        {
            return TeacherSubjectErrors.AlreadyAssigned;
        }

        var assignment = new TeacherSubjectAssignment
        {
            Id = Guid.NewGuid(),
            TeacherId = request.TeacherId,
            SubjectId = request.SubjectId
        };

        await _teacherSubjectAssignmentRepository.AddAsync(assignment, cancellationToken);
        await _teacherSubjectAssignmentRepository.SaveChangesAsync(cancellationToken);

        var teacher = await _userRepository.GetByIdAsync(request.TeacherId, cancellationToken);
        var subject = await _subjectRepository.GetByIdAsync(request.SubjectId, cancellationToken);

        return new TeacherSubjectDto
        {
            Id = assignment.Id,
            TeacherId = assignment.TeacherId,
            TeacherName = teacher?.FullName ?? string.Empty,
            SubjectId = assignment.SubjectId,
            SubjectName = subject?.Name ?? string.Empty
        };
    }
}
