using System.ComponentModel.DataAnnotations;

namespace RequestSpark.Web.Models;

/// <summary>
/// Metadata for a stored OpenAPI / Swagger specification
/// </summary>
public class OpenApiSpec
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>API version declared in the spec (e.g. "1.0.0")</summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>Detected format string (e.g. "OpenAPI 3.0.3", "Swagger 2.0")</summary>
    public string SpecFormat { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }

    /// <summary>Primary server URL extracted from the spec (servers[0] or host+basePath)</summary>
    public string? DefaultBaseUrl { get; set; }

    /// <summary>All server URLs declared in the spec</summary>
    public List<string> AvailableServers { get; set; } = new();

    public int EndpointCount { get; set; }
    public List<string> HttpMethods { get; set; } = new();

    /// <summary>Tags declared in the spec</summary>
    public List<string> ApiTags { get; set; } = new();

    /// <summary>User-assigned organisational tags</summary>
    public List<string> UserTags { get; set; } = new();

    public List<string> SecuritySchemes { get; set; } = new();

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public string UploadedBy { get; set; } = "System";
    public bool IsActive { get; set; } = true;
    public string? ContentHash { get; set; }
}

