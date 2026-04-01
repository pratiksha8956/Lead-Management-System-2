using LeadManagement.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeadManagement.Api.Controllers;

[ApiController]
[Route("api/analytics")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly ICachedAnalyticsService _analytics;

    public AnalyticsController(ICachedAnalyticsService analytics)
    {
        _analytics = analytics;
    }

    [HttpGet("by-source")]
    public async Task<IActionResult> BySource(CancellationToken cancellationToken)
    {
        var s = await _analytics.GetSnapshotAsync(cancellationToken);
        return Ok(s.BySource);
    }

    [HttpGet("by-status")]
    public async Task<IActionResult> ByStatus(CancellationToken cancellationToken)
    {
        var s = await _analytics.GetSnapshotAsync(cancellationToken);
        return Ok(s.ByStatus);
    }

    [HttpGet("conversion-rate")]
    public async Task<IActionResult> ConversionRate(CancellationToken cancellationToken)
    {
        var s = await _analytics.GetSnapshotAsync(cancellationToken);
        return Ok(new { conversionRate = s.ConversionRate });
    }

    [HttpGet("by-salesrep")]
    public async Task<IActionResult> BySalesRep(CancellationToken cancellationToken)
    {
        var s = await _analytics.GetSnapshotAsync(cancellationToken);
        return Ok(s.BySalesRep);
    }
}
