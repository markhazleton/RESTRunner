using System.ComponentModel.DataAnnotations;

namespace RESTRunner.Web.Models.ViewModels;

public class OpenApiUploadViewModel
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Title must be between 2 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>Comma-separated user tags</summary>
    public string TagsString { get; set; } = string.Empty;

    /// <summary>Override the default base URL extracted from the spec</summary>
    public string? DefaultBaseUrl { get; set; }

    [Required(ErrorMessage = "Please select an OpenAPI JSON file")]
    public IFormFile? SpecFile { get; set; }

    public List<string> GetTags() =>
        TagsString?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
        ?? new List<string>();
}

public class OpenApiDetailsViewModel
{
    public OpenApiSpec Spec { get; set; } = new();
    public OpenApiStructure Structure { get; set; } = new();
}

/// <summary>Request body sent by the Test runner UI to execute a single endpoint</summary>
public class OpenApiEndpointExecuteRequest
{
    public string BaseUrl { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = "GET";

    /// <summary>Path parameters already substituted into Path — kept separately for logging</summary>
    public Dictionary<string, string> PathParams { get; set; } = new();
    public Dictionary<string, string> QueryParams { get; set; } = new();
    public Dictionary<string, string> Headers { get; set; } = new();
    public string? Body { get; set; }

    // Auth
    public string AuthType { get; set; } = "none"; // none | bearer | apikey | basic
    public string? BearerToken { get; set; }
    public string? ApiKeyHeader { get; set; }
    public string? ApiKeyValue { get; set; }
    public string? BasicUsername { get; set; }
    public string? BasicPassword { get; set; }
}

/// <summary>Response returned from the execute API</summary>
public class OpenApiEndpointExecuteResult
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public string? ResponseBody { get; set; }
    public Dictionary<string, string> ResponseHeaders { get; set; } = new();
    public long ResponseTimeMs { get; set; }
    public string RequestUrl { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}
