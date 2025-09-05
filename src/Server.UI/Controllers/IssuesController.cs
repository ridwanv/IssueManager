using Microsoft.AspNetCore.Mvc;
using MediatR;
using CleanArchitecture.Blazor.Application.Features.Issues.Commands.Create;
using CleanArchitecture.Blazor.Application.Features.Issues.Commands.Update;
using CleanArchitecture.Blazor.Application.Features.Issues.Commands.Delete;
using CleanArchitecture.Blazor.Application.Features.Issues.Queries.GetById;
using CleanArchitecture.Blazor.Application.Features.Issues.Queries.GetIssues;
using CleanArchitecture.Blazor.Application.Features.Issues.DTOs;
using CleanArchitecture.Blazor.Application.Common.Models;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Server.UI.Controllers;

[ApiController]
[Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
public class IssuesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<IssuesController> _logger;

    public IssuesController(IMediator mediator, ILogger<IssuesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all issues with pagination and filtering
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="keyword">Search keyword</param>
    /// <param name="orderBy">Order by field (default: Created)</param>
    /// <param name="sortDirection">Sort direction (Ascending/Descending, default: Descending)</param>
    /// <param name="status">Filter by status</param>
    /// <param name="priority">Filter by priority</param>
    /// <param name="category">Filter by category</param>
    /// <returns>Paginated list of issues</returns>
    [HttpGet]
    public async Task<ActionResult<PaginatedData<IssueListDto>>> GetIssues(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? keyword = null,
        [FromQuery] string orderBy = "Created",
        [FromQuery] string sortDirection = "Descending",
        [FromQuery] IssueStatus? status = null,
        [FromQuery] IssuePriority? priority = null,
        [FromQuery] IssueCategory? category = null)
    {
        try
        {
            var query = new GetIssuesQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Keyword = keyword,
                OrderBy = orderBy,
                SortDirection = sortDirection,
                StatusFilter = status,
                PriorityFilter = priority,
                CategoryFilter = category
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving issues");
            return StatusCode(500, "An error occurred while retrieving issues");
        }
    }

    /// <summary>
    /// Get a specific issue by JIRA Key
    /// </summary>
    /// <param name="jiraKey">JIRA Key (e.g., SUP-123)</param>
    /// <returns>Issue details</returns>
    [HttpGet("by-jira-key/{jiraKey}")]
    public async Task<ActionResult<Result<IssueDto>>> GetIssueByJiraKey(string jiraKey)
    {
        try
        {
            var query = new GetIssuesQuery 
            { 
                PageNumber = 1, 
                PageSize = 1, 
                Keyword = jiraKey  // Use keyword search to find by JIRA key
            };
            
            var result = await _mediator.Send(query);
            
            if (result?.Items?.Any() == true)
            {
                // Find exact match by JiraKey
                var issue = result.Items.FirstOrDefault(i => i.JiraKey == jiraKey);
                if (issue != null)
                {
                    // Get full details
                    var detailQuery = new GetIssueByIdQuery { Id = issue.Id };
                    var detailResult = await _mediator.Send(detailQuery);
                    return Ok(detailResult);
                }
            }
            
            return NotFound(await Result<IssueDto>.FailureAsync($"Issue with JIRA key '{jiraKey}' not found"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving issue by JIRA key {JiraKey}", jiraKey);
            return StatusCode(500, "An error occurred while retrieving the issue");
        }
    }

    /// <summary>
    /// Get a specific issue by ID
    /// </summary>
    /// <param name="id">Issue ID</param>
    /// <returns>Issue details</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Result<IssueDto>>> GetIssue(Guid id)
    {
        try
        {
            var query = new GetIssueByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            
            if (!result.Succeeded)
            {
                return NotFound(result);
            }
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving issue {IssueId}", id);
            return StatusCode(500, "An error occurred while retrieving the issue");
        }
    }

    /// <summary>
    /// Create a new issue
    /// </summary>
    /// <param name="command">Create issue command</param>
    /// <returns>Created issue ID</returns>
    [HttpPost]
    public async Task<ActionResult<Result<Guid>>> CreateIssue([FromBody] CreateIssueCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            
            if (!result.Succeeded)
            {
                return BadRequest(result);
            }
            
            return CreatedAtAction(
                nameof(GetIssue), 
                new { id = result.Data }, 
                result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating issue");
            return StatusCode(500, "An error occurred while creating the issue");
        }
    }

    /// <summary>
    /// Update an existing issue
    /// </summary>
    /// <param name="id">Issue ID</param>
    /// <param name="command">Update issue command</param>
    /// <returns>Updated issue ID</returns>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Result<Guid>>> UpdateIssue(Guid id, [FromBody] UpdateIssueCommand command)
    {
        try
        {
            if (id != command.Id)
            {
                return BadRequest("Issue ID in URL does not match the ID in the request body");
            }

            var result = await _mediator.Send(command);
            
            if (!result.Succeeded)
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating issue {IssueId}", id);
            return StatusCode(500, "An error occurred while updating the issue");
        }
    }

    /// <summary>
    /// Delete an issue
    /// </summary>
    /// <param name="id">Issue ID</param>
    /// <returns>Deleted issue ID</returns>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<Result<Guid>>> DeleteIssue(Guid id)
    {
        try
        {
            var command = new DeleteIssueCommand { Id = id };
            var result = await _mediator.Send(command);
            
            if (!result.Succeeded)
            {
                return NotFound(result);
            }
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting issue {IssueId}", id);
            return StatusCode(500, "An error occurred while deleting the issue");
        }
    }
}