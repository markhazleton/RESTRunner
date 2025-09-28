using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using RESTRunner.Domain.Models;

namespace RESTRunner.Web.Models;

/// <summary>
/// Represents a test configuration that can be stored and managed through the web interface
/// </summary>
public class TestConfiguration
{
    /// <summary>
    /// Unique identifier for the configuration
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Display name for the configuration
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of what this configuration tests
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// The actual CompareRunner configuration
    /// </summary>
    [Required]
    public CompareRunner Runner { get; set; } = new();

    /// <summary>
    /// Associated Postman collection file name (if any)
    /// </summary>
    public string? CollectionFileName { get; set; }

    /// <summary>
    /// Number of iterations to run (default from console app is 100)
    /// </summary>
    [Range(1, 1000)]
    public int Iterations { get; set; } = 100;

    /// <summary>
    /// Maximum concurrent requests (default from console app is 10)
    /// </summary>
    [Range(1, 100)]
    public int MaxConcurrency { get; set; } = 10;

    /// <summary>
    /// Tags for organizing configurations
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// When this configuration was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this configuration was last modified
    /// </summary>
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User who created this configuration
    /// </summary>
    public string CreatedBy { get; set; } = "System";

    /// <summary>
    /// Whether this configuration is active/enabled
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Validates the configuration
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    public bool IsValid() => !string.IsNullOrEmpty(Name) && Runner.IsValid();

    /// <summary>
    /// Gets the total number of test combinations for this configuration
    /// </summary>
    /// <returns>Total test combinations</returns>
    public int GetTotalTestCount() => Runner.GetTotalTestCount() * Iterations;
}