using Assignly.Application.Features.Submissions.Commands.GradeSubmission;
using Assignly.Application.Features.Submissions.Commands.SetSubmissionStatus;
using Assignly.Application.Features.Submissions.Commands.UpdateSubmission;
using Assignly.Application.Features.Submissions.Queries.GetMySubmissions;
using Assignly.Application.Features.Submissions.Queries.GetSubmissionById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assignly.Host.Controllers;

[Route("api/submissions")]
public class SubmissionsController : BaseApiController
{
    private readonly IMediator _mediator;

    public SubmissionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize(Roles = "Student")]
    [HttpGet("mine")]
    public async Task<IActionResult> GetMySubmissions(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMySubmissionsQuery(CurrentUserId), cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSubmissionById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSubmissionByIdQuery(id, CurrentUserId, CurrentUserRole), cancellationToken);
        return HandleResult(result);
    }

    [Authorize(Roles = "Student")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateSubmission(Guid id, [FromBody] UpdateSubmissionCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command with { Id = id, StudentId = CurrentUserId }, cancellationToken);
        return HandleResult(result);
    }

    [Authorize(Roles = "Teacher")]
    [HttpPatch("{id:guid}/grade")]
    public async Task<IActionResult> GradeSubmission(Guid id, [FromBody] GradeSubmissionCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command with { Id = id, TeacherId = CurrentUserId }, cancellationToken);
        return HandleResult(result);
    }

    [Authorize(Roles = "Teacher")]
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> SetSubmissionStatus(Guid id, [FromBody] SetSubmissionStatusCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command with { Id = id, TeacherId = CurrentUserId }, cancellationToken);
        return HandleResult(result);
    }
}
