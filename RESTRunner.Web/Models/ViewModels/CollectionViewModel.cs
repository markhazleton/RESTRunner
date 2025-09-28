using System.ComponentModel.DataAnnotations;

namespace RESTRunner.Web.Models.ViewModels;

/// <summary>
/// View model for uploading and managing collections
/// </summary>
public class CollectionUploadViewModel
{
    /// <summary>
    /// Display name for the collection
    /// </summary>
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters")]
    [Display(Name = "Collection Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description
    /// </summary>
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    /// <summary>
    /// Tags (comma-separated)
    /// </summary>
    [Display(Name = "Tags (comma-separated)")]
    public string? TagsString { get; set; }

    /// <summary>
    /// Uploaded file
    /// </summary>
    [Required(ErrorMessage = "Please select a collection file")]
    [Display(Name = "Collection File (.json)")]
    public IFormFile CollectionFile { get; set; } = null!;

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
}

/// <summary>
/// View model for displaying collection details
/// </summary>
public class CollectionDetailsViewModel
{
    public CollectionMetadata Metadata { get; set; } = new();
    public CollectionStructure Structure { get; set; } = new();
    public List<ConfigurationSummary> UsedByConfigurations { get; set; } = new();
}

/// <summary>
/// Represents the structure of a Postman collection
/// </summary>
public class CollectionStructure
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<CollectionFolder> Folders { get; set; } = new();
    public List<CollectionRequest> Requests { get; set; } = new();
    public Dictionary<string, string> Variables { get; set; } = new();
}

/// <summary>
/// Represents a folder in a Postman collection
/// </summary>
public class CollectionFolder
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<CollectionRequest> Requests { get; set; } = new();
    public List<CollectionFolder> SubFolders { get; set; } = new();
}

/// <summary>
/// Represents a request in a Postman collection
/// </summary>
public class CollectionRequest
{
    public string Name { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new();
    public string? Body { get; set; }
    public List<string> Tests { get; set; } = new();
}