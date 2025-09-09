namespace CleanArchitecture.Blazor.Application.Features.Conversations.Caching;

/// <summary>
/// Static class for managing cache keys and expiration for Conversation-related data.
/// </summary>
public static class ConversationCacheKey
{
    public const string GetAllCacheKey = "all-Conversations";
    public static string GetPaginationCacheKey(string parameters) {
        return $"ConversationCacheKey:ConversationsWithPaginationQuery,{parameters}";
    }
    public static string GetExportCacheKey(string parameters) {
        return $"ConversationCacheKey:ExportCacheKey,{parameters}";
    }
    public static string GetByIdCacheKey(string parameters) {
        return $"ConversationCacheKey:GetByIdCacheKey,{parameters}";
    }
    public static string GetDetailCacheKey(string parameters) {
        return $"ConversationCacheKey:GetDetailCacheKey,{parameters}";
    }
    public static string GetCacheKey(string parameters) {
        return $"ConversationCacheKey:{parameters}";
    }
    public static IEnumerable<string>? Tags => new string[] { "conversation" };
    public static void Refresh()
    {
        FusionCacheFactory.RemoveByTags(Tags);
    }
}
