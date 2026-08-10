using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;
using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using ErrorOr;

namespace Assignly.Application.Features.Subjects.Commands.UpdateSubject;

public sealed class UpdateSubjectCommandHandler : ICommandHandler<UpdateSubjectCommand, SubjectDto>
{
    private readonly IRepository<Subject> _subjectRepository;
    private readonly IRepository<SchoolClass> _classRepository;

    public UpdateSubjectCommandHandler(IRepository<Subject> subjectRepository, IRepository<SchoolClass> classRepository)
    {
        _subjectRepository = subjectRepository;
        _classRepository = classRepository;
    }

    public async Task<ErrorOr<SubjectDto>> Handle(UpdateSubjectCommand request, CancellationToken cancellationToken)
    {
        var subject = await _subjectRepository.GetByIdAsync(request.Id, cancellationToken);
        if (subject is null)
        {
            return SubjectErrors.NotFound(request.Id);
        }

        var schoolClass = await _classRepository.GetByIdAsync(request.ClassId, cancellationToken);
        if (schoolClass is null)
        {
            return SubjectErrors.ClassNotFound(request.ClassId);
        }

        subject.Name = request.Name;
        subject.ClassId = request.ClassId;

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
