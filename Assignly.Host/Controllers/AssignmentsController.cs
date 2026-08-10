using Assignly.Application.Features.Assignments.Commands.CreateAssignment;
using Assignly.Application.Features.Assignments.Commands.DeleteAssignment;
using Assignly.Application.Features.Assignments.Commands.PublishAssignment;
using Assignly.Application.Features.Assignments.Commands.UpdateAssignment;
using Assignly.Application.Features.Assignments.Queries.GetAssignmentById;
using Assignly.Application.Features.Assignments.Queries.GetAssignments;
using Assignly.Application.Features.Submissions.Commands.SubmitAssignment;
using Assignly.Application.Features.Submissions.Queries.GetSubmissionsForAssignment;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assignly.Host.Controllers;

[Route("api/assignments")]
public class AssignmentsController : BaseApiController
{
    private readonly IMediator _mediator;
    public AssignmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAssignments(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetAssignmentsQuery(CurrentUserId, CurrentUserRole, CurrentUserClassId), cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAssignmentById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetAssignmentByIdQuery(id, CurrentUserId, CurrentUserRole, CurrentUserClassId), cancellationToken);
        return HandleResult(result);
    }

    [Authorize(Roles = "Teacher")]
    [HttpPost]
    public async Task<IActionResult> CreateAssignment([FromBody] CreateAssignmentCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command with { TeacherId = CurrentUserId }, cancellationToken);
        return HandleResult(result);
    }

    [Authorize(Roles = "Teacher")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAssignment(Guid id, [FromBody] UpdateAssignmentCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command with { Id = id, TeacherId = CurrentUserId }, cancellationToken);
        return HandleResult(result);
    }

    [Authorize(Roles = "Teacher")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAssignment(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteAssignmentCommand(id, CurrentUserId), cancellationToken);
        return HandleResult(result);
    }

    [Authorize(Roles = "Teacher")]
    [HttpPost("{id:guid}/publish")]
    public async Task<IActionResult> PublishAssignment(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new PublishAssignmentCommand(id, CurrentUserId), cancellationToken);
        return HandleResult(result);
    }

    [Authorize(Roles = "Student")]
    [HttpPost("{id:guid}/submissions")]
    public async Task<IActionResult> SubmitAssignment(Guid id, [FromBody] SubmitAssignmentCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command with { AssignmentId = id, StudentId = CurrentUserId }, cancellationToken);
        return HandleResult(result);
    }

    [Authorize(Roles = "Teacher,Admin")]
    [HttpGet("{id:guid}/submissions")]
    public async Task<IActionResult> GetSubmissionsForAssignment(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetSubmissionsForAssignmentQuery(id, CurrentUserId, CurrentUserRole), cancellationToken);
        return HandleResult(result);
    }
}
