using RequestSpark.Domain.Models;
using RequestSpark.Web.Models;

namespace RequestSpark.Web.Services;

/// <summary>
/// Maps external API definition sources to RequestSpark domain request models.
/// </summary>
public interface IApiDefinitionMappingService
{
    /// <summary>
    /// Maps the selected source to CompareRequest items.
    /// </summary>
    Task<ApiDefinitionMappingResult> MapRequestsAsync(ApiDefinitionType sourceType, string sourceId);
}

/// <summary>
/// Result of mapping an API definition source to domain requests.
/// </summary>
public class ApiDefinitionMappingResult
{
    public string SourceName { get; set; } = string.Empty;
    public List<CompareRequest> Requests { get; set; } = new();
}

