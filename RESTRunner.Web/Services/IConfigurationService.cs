using RESTRunner.Web.Models;
using RESTRunner.Web.Models.ViewModels;

namespace RESTRunner.Web.Services;

/// <summary>
/// Service for managing test configurations
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Get all configurations
    /// </summary>
    /// <returns>List of configurations</returns>
    Task<List<TestConfiguration>> GetAllAsync();

    /// <summary>
    /// Get active configurations only
    /// </summary>
    /// <returns>List of active configurations</returns>
    Task<List<TestConfiguration>> GetActiveAsync();

    /// <summary>
    /// Get configuration by ID
    /// </summary>
    /// <param name="id">Configuration ID</param>
    /// <returns>Configuration or null if not found</returns>
    Task<TestConfiguration?> GetByIdAsync(string id);

    /// <summary>
    /// Get configuration by name
    /// </summary>
    /// <param name="name">Configuration name</param>
    /// <returns>Configuration or null if not found</returns>
    Task<TestConfiguration?> GetByNameAsync(string name);

    /// <summary>
    /// Create a new configuration
    /// </summary>
    /// <param name="configuration">Configuration to create</param>
    /// <returns>Created configuration</returns>
    Task<TestConfiguration> CreateAsync(TestConfiguration configuration);

    /// <summary>
    /// Update an existing configuration
    /// </summary>
    /// <param name="configuration">Configuration to update</param>
    /// <returns>Updated configuration</returns>
    Task<TestConfiguration> UpdateAsync(TestConfiguration configuration);

    /// <summary>
    /// Delete a configuration
    /// </summary>
    /// <param name="id">Configuration ID</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteAsync(string id);

    /// <summary>
    /// Get configurations by tags
    /// </summary>
    /// <param name="tags">Tags to search for</param>
    /// <returns>List of matching configurations</returns>
    Task<List<TestConfiguration>> GetByTagsAsync(params string[] tags);

    /// <summary>
    /// Search configurations by name or description
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <returns>List of matching configurations</returns>
    Task<List<TestConfiguration>> SearchAsync(string searchTerm);

    /// <summary>
    /// Export a configuration to JSON
    /// </summary>
    /// <param name="id">Configuration ID</param>
    /// <returns>JSON string or null if not found</returns>
    Task<string?> ExportAsync(string id);

    /// <summary>
    /// Import a configuration from JSON
    /// </summary>
    /// <param name="json">JSON string</param>
    /// <param name="overwriteId">Whether to overwrite ID if it exists</param>
    /// <returns>Imported configuration</returns>
    Task<TestConfiguration> ImportAsync(string json, bool overwriteId = false);

    /// <summary>
    /// Validate a configuration
    /// </summary>
    /// <param name="configuration">Configuration to validate</param>
    /// <returns>Validation result</returns>
    Task<ValidationResult> ValidateAsync(TestConfiguration configuration);
}

/// <summary>
/// Validation result for configurations
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}