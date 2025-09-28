using System.ComponentModel.DataAnnotations;

namespace RESTRunner.Web.Models;

/// <summary>
/// Metadata about a Postman collection
/// </summary>
public class CollectionMetadata
{
    /// <summary>
    /// Unique identifier for this collection
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Display name for the collection
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Original filename of the uploaded collection
    /// </summary>
    [Required]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// File path where the collection is stored
    /// </summary>
    [Required]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the collection
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Postman collection ID (from the collection.json)
    /// </summary>
    public string? PostmanId { get; set; }

    /// <summary>
    /// Collection schema version
    /// </summary>
    public string? SchemaVersion { get; set; }

    /// <summary>
    /// Number of requests in the collection
    /// </summary>
    public int RequestCount { get; set; }

    /// <summary>
    /// When this collection was uploaded
    /// </summary>
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User who uploaded this collection
    /// </summary>
    public string UploadedBy { get; set; } = "System";

    /// <summary>
    /// Tags for organizing collections
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Whether this collection is currently active/usable
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Hash of the collection content for change detection
    /// </summary>
    public string? ContentHash { get; set; }

    /// <summary>
    /// Environment variables found in the collection
    /// </summary>
    public List<string> EnvironmentVariables { get; set; } = new();

    /// <summary>
    /// HTTP methods used in the collection
    /// </summary>
    public List<string> HttpMethods { get; set; } = new();

    /// <summary>
    /// Validates the collection metadata
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    public bool IsValid() => !string.IsNullOrEmpty(Name) && 
                           !string.IsNullOrEmpty(FileName) && 
                           !string.IsNullOrEmpty(FilePath) &&
                           File.Exists(FilePath);
}