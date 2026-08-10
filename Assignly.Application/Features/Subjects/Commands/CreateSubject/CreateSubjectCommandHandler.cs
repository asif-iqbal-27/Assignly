using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;
using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using ErrorOr;

namespace Assignly.Application.Features.Subjects.Commands.CreateSubject;

public sealed class CreateSubjectCommandHandler : ICommandHandler<CreateSubjectCommand, SubjectDto>
{
    private readonly IRepository<Subject> _subjectRepository;
    private readonly IRepository<SchoolClass> _classRepository;

    public CreateSubjectCommandHandler(IRepository<Subject> subjectRepository, IRepository<SchoolClass> classRepository)
    {
        _subjectRepository = subjectRepository;
        _classRepository = classRepository;
    }

    public async Task<ErrorOr<SubjectDto>> Handle(CreateSubjectCommand request, CancellationToken cancellationToken)
    {
        var schoolClass = await _classRepository.GetByIdAsync(request.ClassId, cancellationToken);
        if (schoolClass is null)
        {
            return SubjectErrors.ClassNotFound(request.ClassId);
        }

        var subject = new Subject
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            ClassId = request.ClassId
        };

        await _subjectRepository.AddAsync(subject, cancellationToken);
        await _subjectRepository.SaveChangesAsync(cancellationToken);

        return new SubjectDto
        {
            Id = subject.Id,
            Name = subject.Name,
            ClassId = subject.ClassId,
            ClassName = schoolClass.Name
        };
    }
}
