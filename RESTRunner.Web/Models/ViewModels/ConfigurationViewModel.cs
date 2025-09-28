using System.ComponentModel.DataAnnotations;

namespace RESTRunner.Web.Models.ViewModels;

/// <summary>
/// View model for creating or editing a test configuration
/// </summary>
public class ConfigurationViewModel
{
    /// <summary>
    /// Configuration ID (empty for new configurations)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the configuration
    /// </summary>
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters")]
    [Display(Name = "Configuration Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description
    /// </summary>
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    /// <summary>
    /// Number of iterations to run
    /// </summary>
    [Required]
    [Range(1, 1000, ErrorMessage = "Iterations must be between 1 and 1000")]
    [Display(Name = "Number of Iterations")]
    public int Iterations { get; set; } = 100;

    /// <summary>
    /// Maximum concurrent requests
    /// </summary>
    [Required]
    [Range(1, 100, ErrorMessage = "Max concurrency must be between 1 and 100")]
    [Display(Name = "Max Concurrent Requests")]
    public int MaxConcurrency { get; set; } = 10;

    /// <summary>
    /// Selected collection ID
    /// </summary>
    [Display(Name = "Postman Collection")]
    public string? CollectionId { get; set; }

    /// <summary>
    /// Tags (comma-separated)
    /// </summary>
    [Display(Name = "Tags (comma-separated)")]
    public string? TagsString { get; set; }

    /// <summary>
    /// Whether the configuration is active
    /// </summary>
    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// JSON representation of instances
    /// </summary>
    [Display(Name = "Test Instances (JSON)")]
    public string InstancesJson { get; set; } = "[]";

    /// <summary>
    /// JSON representation of users
    /// </summary>
    [Display(Name = "Test Users (JSON)")]
    public string UsersJson { get; set; } = "[]";

    /// <summary>
    /// JSON representation of requests
    /// </summary>
    [Display(Name = "Test Requests (JSON)")]
    public string RequestsJson { get; set; } = "[]";

    /// <summary>
    /// Available collections for selection
    /// </summary>
    public List<CollectionOption> AvailableCollections { get; set; } = new();

    /// <summary>
    /// Convert tags string to list
    /// </summary>
    public List<string> GetTags()
    {
        if (string.IsNullOrWhiteSpace(TagsString))
            return new List<string>();

        return TagsString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(t => t.Trim())
                        .Where(t => !string.IsNullOrEmpty(t))
                        .ToList();
    }

    /// <summary>
    /// Set tags from list
    /// </summary>
    public void SetTags(List<string> tags)
    {
        TagsString = string.Join(", ", tags);
    }
}

/// <summary>
/// Option for collection selection
/// </summary>
public class CollectionOption
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public int RequestCount { get; set; }
}