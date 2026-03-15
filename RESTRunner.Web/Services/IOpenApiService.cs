using RESTRunner.Web.Models;

namespace RESTRunner.Web.Services;

/// <summary>Service for managing OpenAPI / Swagger specifications</summary>
public interface IOpenApiService
{
    Task<List<OpenApiSpec>> GetAllAsync();
    Task<List<OpenApiSpec>> GetActiveAsync();
    Task<OpenApiSpec?> GetByIdAsync(string id);
    Task<OpenApiSpec> UploadAsync(IFormFile file, OpenApiSpec metadata);
    Task<OpenApiSpec> UpdateAsync(OpenApiSpec spec);
    Task<bool> DeleteAsync(string id);
    Task<string?> GetContentAsync(string id);
    Task<OpenApiStructure?> GetStructureAsync(string id);
    Task<OpenApiValidationResult> ValidateAsync(IFormFile file);
    Task<OpenApiValidationResult> ValidateAsync(string json);
    Task<List<OpenApiSpec>> SearchAsync(string searchTerm);
}

/// <summary>Result of validating an OpenAPI / Swagger file</summary>
public class OpenApiValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public string? Title { get; set; }
    public string? Version { get; set; }
    public string? SpecFormat { get; set; }
    public int EndpointCount { get; set; }
    public List<string> HttpMethods { get; set; } = new();
    public List<string> ApiTags { get; set; } = new();
    public List<string> Servers { get; set; } = new();
    public string? DefaultBaseUrl { get; set; }
    public List<string> SecuritySchemes { get; set; } = new();
}
