using Microsoft.AspNetCore.Mvc;
using CleanArchitecture.Blazor.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly IApplicationDbContextFactory _dbContextFactory;

    public TestController(IApplicationDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    [HttpGet("check-data")]
    public async Task<IActionResult> CheckData()
    {
        try
        {
            await using var db = await _dbContextFactory.CreateAsync();
            
            var issueCount = await db.Issues.CountAsync();
            var tenantCount = await db.Tenants.CountAsync();
            
            var sampleIssue = await db.Issues.FirstOrDefaultAsync();
            
            return Ok(new
            {
                IssueCount = issueCount,
                TenantCount = tenantCount,
                SampleIssue = sampleIssue != null ? new
                {
                    sampleIssue.Id,
                    sampleIssue.Title,
                    sampleIssue.Status,
                    sampleIssue.Priority,
                    sampleIssue.Category,
                    sampleIssue.Created
                } : null
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
}