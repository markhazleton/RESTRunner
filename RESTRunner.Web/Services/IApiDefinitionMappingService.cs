using RESTRunner.Domain.Models;
using RESTRunner.Web.Models;

namespace RESTRunner.Web.Services;

/// <summary>
/// Maps external API definition sources to RESTRunner domain request models.
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
