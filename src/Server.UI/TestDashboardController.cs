using Microsoft.AspNetCore.Mvc;
using CleanArchitecture.Blazor.Application.Features.Issues.Queries.GetDashboardMetrics;
using CleanArchitecture.Blazor.Application.Features.Issues.Queries.GetPerformanceStats;
using MediatR;

[ApiController]
[Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
public class TestDashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public TestDashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("metrics")]
    public async Task<IActionResult> GetDashboardMetrics()
    {
        try
        {
            var query = new GetDashboardMetricsQuery();
            var result = await _mediator.Send(query);
            
            if (result.Succeeded)
            {
                return Ok(result.Data);
            }
            
            return BadRequest(new { 
                Succeeded = result.Succeeded,
                Errors = result.Errors,
                ErrorCount = result.Errors?.Count ?? 0
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message, StackTrace = ex.StackTrace });
        }
    }

    [HttpGet("performance")]
    public async Task<IActionResult> GetPerformanceStats()
    {
        try
        {
            var query = new GetPerformanceStatsQuery(30);
            var result = await _mediator.Send(query);
            
            if (result.Succeeded)
            {
                return Ok(result.Data);
            }
            
            return BadRequest(new { 
                Succeeded = result.Succeeded,
                Errors = result.Errors,
                ErrorCount = result.Errors?.Count ?? 0
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message, StackTrace = ex.StackTrace });
        }
    }
}