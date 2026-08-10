using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;
using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Assignly.Application.Features.ClassSubjectTeachers.Commands.CreateClassSubjectTeacher;

public sealed class CreateClassSubjectTeacherCommandHandler : ICommandHandler<CreateClassSubjectTeacherCommand, ClassSubjectTeacherDto>
{
    private readonly IRepository<ClassSubjectTeacher> _classSubjectTeacherRepository;
    private readonly IRepository<ApplicationUser> _userRepository;
    private readonly IRepository<Subject> _subjectRepository;

    public CreateClassSubjectTeacherCommandHandler(
        IRepository<ClassSubjectTeacher> classSubjectTeacherRepository,
        IRepository<ApplicationUser> userRepository,
        IRepository<Subject> subjectRepository)
    {
        _classSubjectTeacherRepository = classSubjectTeacherRepository;
        _userRepository = userRepository;
        _subjectRepository = subjectRepository;
    }

    public async Task<ErrorOr<ClassSubjectTeacherDto>> Handle(CreateClassSubjectTeacherCommand request, CancellationToken cancellationToken)
    {
        var alreadyAssigned = await _classSubjectTeacherRepository.Query()
            .AnyAsync(t => t.TeacherId == request.TeacherId && t.SubjectId == request.SubjectId, cancellationToken);

        if (alreadyAssigned)
        {
            return ClassSubjectTeacherErrors.AlreadyAssigned;
        }

        var classSubjectTeacher = new ClassSubjectTeacher
        {
            Id = Guid.NewGuid(),
            TeacherId = request.TeacherId,
            SubjectId = request.SubjectId
        };

        await _classSubjectTeacherRepository.AddAsync(classSubjectTeacher, cancellationToken);
        await _classSubjectTeacherRepository.SaveChangesAsync(cancellationToken);

        var teacher = await _userRepository.GetByIdAsync(request.TeacherId, cancellationToken);
        var subject = await _subjectRepository.GetByIdAsync(request.SubjectId, cancellationToken);

        return new ClassSubjectTeacherDto
        {
            Id = classSubjectTeacher.Id,
            TeacherId = classSubjectTeacher.TeacherId,
            TeacherName = teacher?.FullName ?? string.Empty,
            SubjectId = classSubjectTeacher.SubjectId,
            SubjectName = subject?.Name ?? string.Empty
        };
    }
}
