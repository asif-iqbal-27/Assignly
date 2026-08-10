using Assignly.Application.Features.TeacherSubjects.Commands.CreateTeacherSubject;
using Assignly.Application.Features.TeacherSubjects.Commands.DeleteTeacherSubject;
using Assignly.Application.Features.TeacherSubjects.Queries.GetTeacherSubjectsByTeacher;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assignly.Host.Controllers;

[Route("api/teacher-subjects")]
[Authorize(Roles = "Admin")]
public class TeacherSubjectsController : BaseApiController
{
    private readonly IMediator _mediator;

    public TeacherSubjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTeacherSubject([FromBody] CreateTeacherSubjectCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTeacherSubject(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteTeacherSubjectCommand(id), cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("teacher/{teacherId:guid}")]
    public async Task<IActionResult> GetTeacherSubjectsByTeacher(Guid teacherId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTeacherSubjectsByTeacherQuery(teacherId), cancellationToken);
        return HandleResult(result);
    }
}
