using Assignly.Application.Features.Classes.Commands.CreateClass;
using Assignly.Application.Features.Classes.Commands.DeleteClass;
using Assignly.Application.Features.Classes.Commands.UpdateClass;
using Assignly.Application.Features.Classes.Queries.GetClassById;
using Assignly.Application.Features.Classes.Queries.GetClasses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assignly.Host.Controllers;

[Route("api/classes")]
public class ClassesController : BaseApiController
{
    private readonly IMediator _mediator;

    public ClassesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetClasses(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetClassesQuery(), cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetClassById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetClassByIdQuery(id), cancellationToken);
        return HandleResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateClass([FromBody] CreateClassCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateClass(Guid id, [FromBody] UpdateClassCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command with { Id = id }, cancellationToken);
        return HandleResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteClass(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteClassCommand(id), cancellationToken);
        return HandleResult(result);
    }
}
