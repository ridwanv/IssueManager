#nullable enable
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
            var response = await _httpClient.PostAsync("/api/issues/intake", content);

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

    /// <summary>
    /// Escalates a conversation to human agents using the API
    /// </summary>
    /// <param name="conversationId">Bot Framework conversation ID</param>
    /// <param name="reason">Escalation reason</param>
    /// <param name="conversationTranscript">Optional conversation transcript</param>
    /// <param name="whatsAppPhoneNumber">WhatsApp phone number</param>
    /// <returns>Result with the created conversation database ID</returns>
    public async Task<Result<int>> EscalateConversationAsync(
        string conversationId, 
        string reason, 
        string? conversationTranscript = null,
        string? whatsAppPhoneNumber = null)
    {
        try
        {
            _logger.LogInformation("Escalating conversation {ConversationId} via API. Reason: {Reason}", 
                conversationId, reason);

            var escalationRequest = new
            {
                ConversationId = conversationId,
                Reason = reason,
                ConversationTranscript = conversationTranscript,
                WhatsAppPhoneNumber = whatsAppPhoneNumber
            };

            var json = JsonSerializer.Serialize(escalationRequest, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/conversations/escalate", content);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<Result<int>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                _logger.LogInformation("Conversation escalated successfully via API. Database ID: {DbId}", result?.Data);
                return result ?? await Result<int>.FailureAsync("Failed to deserialize API response");
            }
            else
            {
                _logger.LogError("Escalation API call failed with status {StatusCode}: {Response}", 
                    response.StatusCode, responseContent);
                
                // Try to parse error response
                try
                {
                    var errorResult = JsonSerializer.Deserialize<Result<int>>(responseContent, new JsonSerializerOptions
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

                return await Result<int>.FailureAsync($"Escalation API call failed with status {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed when escalating conversation {ConversationId}", conversationId);
            return await Result<int>.FailureAsync("Failed to connect to Issue Manager API");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request timeout when escalating conversation {ConversationId}", conversationId);
            return await Result<int>.FailureAsync("Escalation request timeout - please try again");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when escalating conversation {ConversationId} via API", conversationId);
            return await Result<int>.FailureAsync("An unexpected error occurred during escalation");
        }
    }

    /// <summary>
    /// Stores a conversation message using the API
    /// </summary>
    /// <param name="message">The message to store</param>
    /// <returns>Result with the created message ID</returns>
    public async Task<Result<int>> AddConversationMessageAsync(ConversationMessageCreateDto message)
    {
        try
        {
            _logger.LogInformation("Storing message for conversation {ConversationId} via API. Role: {Role}", 
                message.BotFrameworkConversationId, message.Role);

            var json = JsonSerializer.Serialize(message, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, System.Net.Mime.MediaTypeNames.Application.Json);
            var response = await _httpClient.PostAsync("/api/conversations/messages", content);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<Result<int>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                _logger.LogInformation("Message stored successfully via API. Message ID: {MessageId}", result?.Data);
                return result ?? await Result<int>.FailureAsync("Failed to deserialize API response");
            }
            else
            {
                _logger.LogError("Message storage API call failed with status {StatusCode}: {Response}", 
                    response.StatusCode, responseContent);
                
                // Try to parse error response
                try
                {
                    var errorResult = JsonSerializer.Deserialize<Result<int>>(responseContent, new JsonSerializerOptions
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

                return await Result<int>.FailureAsync($"Message storage API call failed with status {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed when storing message for conversation {ConversationId}", message.BotFrameworkConversationId);
            return await Result<int>.FailureAsync("Failed to connect to Issue Manager API");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request timeout when storing message for conversation {ConversationId}", message.BotFrameworkConversationId);
            return await Result<int>.FailureAsync("Message storage request timeout - please try again");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when storing message for conversation {ConversationId} via API", message.BotFrameworkConversationId);
            return await Result<int>.FailureAsync("An unexpected error occurred during message storage");
        }
    }

    /// <summary>
    /// Retrieves conversation messages using the API
    /// </summary>
    /// <param name="conversationId">Bot Framework conversation ID</param>
    /// <param name="limit">Maximum number of messages to return</param>
    /// <param name="since">Only return messages after this timestamp</param>
    /// <returns>Result with the list of messages</returns>
    public async Task<Result<List<ConversationMessageDto>>> GetConversationMessagesAsync(
        string conversationId, 
        int? limit = null, 
        DateTime? since = null)
    {
        try
        {
            _logger.LogInformation("Retrieving messages for conversation {ConversationId} via API", conversationId);

            var queryParams = new List<string>();
            if (limit.HasValue)
                queryParams.Add($"limit={limit.Value}");
            if (since.HasValue)
                queryParams.Add($"since={since.Value:yyyy-MM-ddTHH:mm:ssZ}");

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var response = await _httpClient.GetAsync($"/api/conversations/messages/{Uri.EscapeDataString(conversationId)}{queryString}");

            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<Result<List<ConversationMessageDto>>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                _logger.LogInformation("Retrieved {MessageCount} messages for conversation {ConversationId}", 
                    result?.Data?.Count ?? 0, conversationId);
                return result ?? await Result<List<ConversationMessageDto>>.FailureAsync("Failed to deserialize API response");
            }
            else
            {
                _logger.LogError("Message retrieval API call failed with status {StatusCode}: {Response}", 
                    response.StatusCode, responseContent);
                return await Result<List<ConversationMessageDto>>.FailureAsync($"Message retrieval API call failed with status {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving messages for conversation {ConversationId} via API", conversationId);
            return await Result<List<ConversationMessageDto>>.FailureAsync("Failed to retrieve conversation messages");
        }
    }
}