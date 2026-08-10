using Assignly.Application.Features.ClassSubjectTeachers.Commands.CreateClassSubjectTeacher;
using Assignly.Application.Features.ClassSubjectTeachers.Commands.DeleteClassSubjectTeacher;
using Assignly.Application.Features.ClassSubjectTeachers.Queries.GetClassSubjectTeachersByTeacher;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assignly.Host.Controllers;

[Route("api/class-subject-teachers")]
[Authorize(Roles = "Admin")]
public class ClassSubjectTeachersController : BaseApiController
{
    private readonly IMediator _mediator;

    public ClassSubjectTeachersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateClassSubjectTeacher([FromBody] CreateClassSubjectTeacherCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteClassSubjectTeacher(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteClassSubjectTeacherCommand(id), cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("teacher/{teacherId:guid}")]
    public async Task<IActionResult> GetClassSubjectTeachersByTeacher(Guid teacherId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetClassSubjectTeachersByTeacherQuery(teacherId), cancellationToken);
        return HandleResult(result);
    }
}
