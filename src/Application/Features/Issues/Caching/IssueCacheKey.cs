namespace CleanArchitecture.Blazor.Application.Features.Issues.Caching;

/// <summary>
/// Static class for managing cache keys and expiration for Issue-related data.
/// </summary>
public static class IssueCacheKey
{
    public const string GetAllCacheKey = "all-Issues";
    public static string GetPaginationCacheKey(string parameters) {
        return $"IssueCacheKey:IssuesWithPaginationQuery,{parameters}";
    }
    public static string GetExportCacheKey(string parameters) {
        return $"IssueCacheKey:ExportCacheKey,{parameters}";
    }
    public static string GetByIdCacheKey(string parameters) {
        return $"IssueCacheKey:GetByIdCacheKey,{parameters}";
    }
    public static IEnumerable<string>? Tags => new string[] { "issue" };
    public static void Refresh()
    {
        FusionCacheFactory.RemoveByTags(Tags);
    }
}