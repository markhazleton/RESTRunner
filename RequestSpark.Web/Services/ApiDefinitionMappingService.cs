using Newtonsoft.Json;
using RequestSpark.Domain.Models;
using RequestSpark.Postman;
using RequestSpark.Postman.Models;
using RequestSpark.Web.Models;

namespace RequestSpark.Web.Services;

/// <summary>
/// Maps Postman collections and OpenAPI specifications into RequestSpark CompareRequest models.
/// </summary>
public class ApiDefinitionMappingService : IApiDefinitionMappingService
{
    private readonly ICollectionService _collectionService;
    private readonly IOpenApiService _openApiService;

    public ApiDefinitionMappingService(ICollectionService collectionService, IOpenApiService openApiService)
    {
        _collectionService = collectionService;
        _openApiService = openApiService;
    }

    public async Task<ApiDefinitionMappingResult> MapRequestsAsync(ApiDefinitionType sourceType, string sourceId)
    {
        if (string.IsNullOrWhiteSpace(sourceId))
        {
            throw new ArgumentException("Source ID is required.", nameof(sourceId));
        }

        return sourceType switch
        {
            ApiDefinitionType.PostmanCollection => await MapPostmanCollectionAsync(sourceId),
            ApiDefinitionType.OpenApiSpec => await MapOpenApiSpecAsync(sourceId),
            _ => throw new ArgumentException("A valid definition source type is required.", nameof(sourceType))
        };
    }

    private async Task<ApiDefinitionMappingResult> MapPostmanCollectionAsync(string sourceId)
    {
        var metadata = await _collectionService.GetByIdAsync(sourceId)
            ?? throw new InvalidOperationException($"Postman collection '{sourceId}' was not found.");

        var content = await _collectionService.GetContentAsync(sourceId)
            ?? throw new InvalidOperationException($"Postman collection content for '{sourceId}' was not found.");

        var root = JsonConvert.DeserializeObject<Root>(content)
            ?? throw new InvalidOperationException("Invalid Postman collection format.");

        var requests = new List<CompareRequest>();
        foreach (var item in root.Item ?? new List<PostmanItem>())
        {
            CollectPostmanRequests(item, requests);
        }

        return new ApiDefinitionMappingResult
        {
            SourceName = metadata.Name,
            Requests = requests
        };
    }

    private static void CollectPostmanRequests(PostmanItem item, List<CompareRequest> requests)
    {
        if (item.Request != null)
        {
            var request = PostmanImport.GetRequest(item.Request);
            if (!string.IsNullOrWhiteSpace(request.Path))
            {
                requests.Add(request);
            }
        }

        foreach (var child in item.Item ?? new List<PostmanItem>())
        {
            CollectPostmanRequests(child, requests);
        }
    }

    private async Task<ApiDefinitionMappingResult> MapOpenApiSpecAsync(string sourceId)
    {
        var spec = await _openApiService.GetByIdAsync(sourceId)
            ?? throw new InvalidOperationException($"OpenAPI specification '{sourceId}' was not found.");

        var structure = await _openApiService.GetStructureAsync(sourceId)
            ?? throw new InvalidOperationException($"OpenAPI structure for '{sourceId}' could not be parsed.");

        var requests = structure.AllEndpoints
            .Select(endpoint => new CompareRequest
            {
                Path = NormalizePath(endpoint.Path),
                RequestMethod = ParseVerb(endpoint.Method),
                BodyTemplate = endpoint.RequestBody?.ExampleJson,
                RequiresClientToken = endpoint.SecurityRequirements.Any()
            })
            .Where(request => !string.IsNullOrWhiteSpace(request.Path))
            .ToList();

        return new ApiDefinitionMappingResult
        {
            SourceName = spec.Title,
            Requests = requests
        };
    }

    private static string NormalizePath(string path) =>
        string.IsNullOrWhiteSpace(path) ? string.Empty : path.TrimStart('/');

    private static HttpVerb ParseVerb(string method)
    {
        if (Enum.TryParse<HttpVerb>(method, true, out var verb))
        {
            return verb;
        }

        return HttpVerb.GET;
    }
}

