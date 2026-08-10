using Assignly.Application.Features.Subjects.Commands.CreateSubject;
using Assignly.Application.Features.Subjects.Commands.DeleteSubject;
using Assignly.Application.Features.Subjects.Commands.UpdateSubject;
using Assignly.Application.Features.Subjects.Queries.GetSubjects;
using Assignly.Application.Features.Subjects.Queries.GetSubjectsByClass;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assignly.Host.Controllers;

[Route("api/subjects")]
public class SubjectsController : BaseApiController
{
    private readonly IMediator _mediator;

    public SubjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetSubjects(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSubjectsQuery(), cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("class/{classId:guid}")]
    public async Task<IActionResult> GetSubjectsByClass(Guid classId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSubjectsByClassQuery(classId), cancellationToken);
        return HandleResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateSubject([FromBody] CreateSubjectCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateSubject(Guid id, [FromBody] UpdateSubjectCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command with { Id = id }, cancellationToken);
        return HandleResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteSubject(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteSubjectCommand(id), cancellationToken);
        return HandleResult(result);
    }
}
