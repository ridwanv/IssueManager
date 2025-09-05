using System.Text;
using System.Text.Json;
using IssueManager.Bot.Models;

namespace IssueManager.Bot.Services;

/// <summary>
/// Service for communicating with the IssueManager API
/// </summary>
public class IssueManagerApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<IssueManagerApiClient> _logger;
    private readonly string _baseUrl;

    public IssueManagerApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<IssueManagerApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = configuration.GetValue<string>("IssueManagerApi:BaseUrl") ?? "https://localhost:7001";
        
        // Configure HttpClient
        _httpClient.BaseAddress = new Uri(_baseUrl);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "IssueManager-Bot/1.0");
    }

    /// <summary>
    /// Creates a new issue using the API
    /// </summary>
    /// <param name="command">The create issue command</param>
    /// <returns>Result with the created issue ID</returns>
    public async Task<Result<Guid>> CreateIssueAsync(CreateIssueCommand command)
    {
        try
        {
            _logger.LogInformation("Creating issue via API for title: {Title}", command.Title);

            var json = JsonSerializer.Serialize(command, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/issues", content);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<Result<Guid>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                _logger.LogInformation("Issue created successfully via API with ID: {IssueId}", result?.Data);
                return result ?? await Result<Guid>.FailureAsync("Failed to deserialize API response");
            }
            else
            {
                _logger.LogError("API call failed with status {StatusCode}: {Response}", response.StatusCode, responseContent);
                
                // Try to parse error response
                try
                {
                    var errorResult = JsonSerializer.Deserialize<Result<Guid>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                    
                    if (errorResult != null && !string.IsNullOrEmpty(errorResult.ErrorMessage))
                    {
                        return errorResult;
                    }
                }
                catch
                {
                    // If error parsing fails, return generic error
                }

                return await Result<Guid>.FailureAsync($"API call failed with status {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed when creating issue");
            return await Result<Guid>.FailureAsync("Failed to connect to Issue Manager API");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request timeout when creating issue");
            return await Result<Guid>.FailureAsync("Request timeout - please try again");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when creating issue via API");
            return await Result<Guid>.FailureAsync("An unexpected error occurred");
        }
    }

    /// <summary>
    /// Gets an issue by ID using the API
    /// </summary>
    /// <param name="id">The issue ID</param>
    /// <returns>Result with the issue details</returns>
    public async Task<Result<IssueDto>> GetIssueAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting issue {IssueId} via API", id);

            var response = await _httpClient.GetAsync($"/api/issues/{id}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<Result<IssueDto>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                return result ?? await Result<IssueDto>.FailureAsync("Failed to deserialize API response");
            }
            else
            {
                _logger.LogError("API call failed with status {StatusCode}: {Response}", response.StatusCode, responseContent);
                return await Result<IssueDto>.FailureAsync($"API call failed with status {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting issue {IssueId} via API", id);
            return await Result<IssueDto>.FailureAsync("Failed to retrieve issue");
        }
    }

    /// <summary>
    /// Gets a paginated list of issues using the API
    /// </summary>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="keyword">Search keyword</param>
    /// <param name="status">Filter by status</param>
    /// <param name="priority">Filter by priority</param>
    /// <param name="category">Filter by category</param>
    /// <returns>Paginated list of issues</returns>
    public async Task<PaginatedData<IssueListDto>?> GetIssuesAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? keyword = null,
        IssueStatus? status = null,
        IssuePriority? priority = null,
        IssueCategory? category = null)
    {
        try
        {
            var queryParams = new List<string>
            {
                $"pageNumber={pageNumber}",
                $"pageSize={pageSize}"
            };

            if (!string.IsNullOrEmpty(keyword))
                queryParams.Add($"keyword={Uri.EscapeDataString(keyword)}");

            if (status.HasValue)
                queryParams.Add($"status={status.Value}");

            if (priority.HasValue)
                queryParams.Add($"priority={priority.Value}");

            if (category.HasValue)
                queryParams.Add($"category={category.Value}");

            var queryString = string.Join("&", queryParams);
            var response = await _httpClient.GetAsync($"/api/issues?{queryString}");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PaginatedData<IssueListDto>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                return result;
            }
            else
            {
                _logger.LogError("API call failed with status {StatusCode}", response.StatusCode);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting issues via API");
            return null;
        }
    }
}