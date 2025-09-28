using RESTRunner.Web.Models;
using RESTRunner.Web.Models.ViewModels;

namespace RESTRunner.Web.Services;

/// <summary>
/// Service for managing Postman collections
/// </summary>
public interface ICollectionService
{
    /// <summary>
    /// Get all collection metadata
    /// </summary>
    /// <returns>List of collection metadata</returns>
    Task<List<CollectionMetadata>> GetAllAsync();

    /// <summary>
    /// Get active collections only
    /// </summary>
    /// <returns>List of active collection metadata</returns>
    Task<List<CollectionMetadata>> GetActiveAsync();

    /// <summary>
    /// Get collection metadata by ID
    /// </summary>
    /// <param name="id">Collection ID</param>
    /// <returns>Collection metadata or null if not found</returns>
    Task<CollectionMetadata?> GetByIdAsync(string id);

    /// <summary>
    /// Upload a new collection
    /// </summary>
    /// <param name="file">Collection file</param>
    /// <param name="metadata">Collection metadata</param>
    /// <returns>Created collection metadata</returns>
    Task<CollectionMetadata> UploadAsync(IFormFile file, CollectionMetadata metadata);

    /// <summary>
    /// Update collection metadata
    /// </summary>
    /// <param name="metadata">Metadata to update</param>
    /// <returns>Updated metadata</returns>
    Task<CollectionMetadata> UpdateAsync(CollectionMetadata metadata);

    /// <summary>
    /// Delete a collection
    /// </summary>
    /// <param name="id">Collection ID</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteAsync(string id);

    /// <summary>
    /// Get collection content as JSON string
    /// </summary>
    /// <param name="id">Collection ID</param>
    /// <returns>JSON content or null if not found</returns>
    Task<string?> GetContentAsync(string id);

    /// <summary>
    /// Parse collection structure
    /// </summary>
    /// <param name="id">Collection ID</param>
    /// <returns>Collection structure or null if not found</returns>
    Task<CollectionStructure?> GetStructureAsync(string id);

    /// <summary>
    /// Validate a collection file
    /// </summary>
    /// <param name="file">Collection file</param>
    /// <returns>Validation result</returns>
    Task<CollectionValidationResult> ValidateAsync(IFormFile file);

    /// <summary>
    /// Validate a collection by content
    /// </summary>
    /// <param name="json">Collection JSON content</param>
    /// <returns>Validation result</returns>
    Task<CollectionValidationResult> ValidateAsync(string json);

    /// <summary>
    /// Get collections by tags
    /// </summary>
    /// <param name="tags">Tags to search for</param>
    /// <returns>List of matching collections</returns>
    Task<List<CollectionMetadata>> GetByTagsAsync(params string[] tags);

    /// <summary>
    /// Search collections by name or description
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <returns>List of matching collections</returns>
    Task<List<CollectionMetadata>> SearchAsync(string searchTerm);
}

/// <summary>
/// Validation result for collections
/// </summary>
public class CollectionValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public string? CollectionName { get; set; }
    public string? PostmanId { get; set; }
    public string? SchemaVersion { get; set; }
    public int RequestCount { get; set; }
    public List<string> EnvironmentVariables { get; set; } = new();
    public List<string> HttpMethods { get; set; } = new();
}