using LeadManagement.Application.Features.Interactions.Commands.AddInteraction;
using LeadManagement.Application.Features.Interactions.Queries.GetInteractions;
using LeadManagement.Application.Features.Leads.Commands.ConvertLead;
using LeadManagement.Application.Features.Leads.Commands.CreateLead;
using LeadManagement.Application.Features.Leads.Commands.DeleteLead;
using LeadManagement.Application.Features.Leads.Commands.UpdateLead;
using LeadManagement.Application.Features.Leads.Queries.GetAllLeads;
using LeadManagement.Application.Features.Leads.Queries.GetLeadById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeadManagement.Api.Controllers;

[ApiController]
[Route("api/leads")]
[Authorize]
public class LeadsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<LeadsController> _logger;

    public LeadsController(IMediator mediator, ILogger<LeadsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public record CreateLeadRequest(
        string Name,
        string Email,
        string Phone,
        string Company,
        string Position,
        string Source,
        string Priority,
        int? AssignedSalesRepId);

    public record UpdateLeadRequest(
        string Name,
        string Email,
        string Phone,
        string Company,
        string Position,
        string Status,
        string Source,
        string Priority,
        int? AssignedSalesRepId);

    public record AddInteractionRequest(string Notes, DateTime InteractionDate, DateTime? FollowUpDate);

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 8,
        [FromQuery] string? status = null,
        [FromQuery] string? source = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAllLeadsQuery(page, limit, status, source), cancellationToken);
        return Ok(new
        {
            items = result.Items,
            page = result.Page,
            pageSize = result.PageSize,
            totalCount = result.TotalCount,
            totalPages = result.TotalPages
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var lead = await _mediator.Send(new GetLeadByIdQuery(id), cancellationToken);
        if (lead is null)
            return NotFound(new { message = "Lead not found" });
        return Ok(lead);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLeadRequest body, CancellationToken cancellationToken)
    {
        var dto = await _mediator.Send(
            new CreateLeadCommand(
                body.Name,
                body.Email,
                body.Phone,
                body.Company,
                body.Position,
                body.Source,
                body.Priority,
                body.AssignedSalesRepId),
            cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = dto.LeadId }, dto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateLeadRequest body, CancellationToken cancellationToken)
    {
        var dto = await _mediator.Send(
            new UpdateLeadCommand(
                id,
                body.Name,
                body.Email,
                body.Phone,
                body.Company,
                body.Position,
                body.Status,
                body.Source,
                body.Priority,
                body.AssignedSalesRepId),
            cancellationToken);
        return Ok(dto);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteLeadCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/convert")]
    [Authorize(Roles = "Admin,SalesManager")]
    public async Task<IActionResult> Convert(int id, CancellationToken cancellationToken)
    {
        var dto = await _mediator.Send(new ConvertLeadCommand(id), cancellationToken);
        _logger.LogInformation("Lead {LeadId} converted via API", id);
        return Ok(dto);
    }

    [HttpGet("{id:int}/interactions")]
    public async Task<IActionResult> GetInteractions(int id, CancellationToken cancellationToken)
    {
        var list = await _mediator.Send(new GetInteractionsQuery(id), cancellationToken);
        return Ok(list);
    }

    [HttpPost("{id:int}/interactions")]
    public async Task<IActionResult> AddInteraction(int id, [FromBody] AddInteractionRequest body, CancellationToken cancellationToken)
    {
        var dto = await _mediator.Send(
            new AddInteractionCommand(id, body.Notes, body.InteractionDate, body.FollowUpDate),
            cancellationToken);
        return CreatedAtAction(nameof(GetInteractions), new { id }, dto);
    }
}
