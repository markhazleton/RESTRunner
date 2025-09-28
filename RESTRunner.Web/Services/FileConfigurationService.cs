using System.Text.Json;
using RESTRunner.Web.Models;
using RESTRunner.Web.Models.ViewModels;
using RESTRunner.Domain.Models;

namespace RESTRunner.Web.Services;

/// <summary>
/// File-based implementation of configuration service
/// </summary>
public class FileConfigurationService : IConfigurationService
{
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<FileConfigurationService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public FileConfigurationService(IFileStorageService fileStorage, ILogger<FileConfigurationService> logger)
    {
        _fileStorage = fileStorage;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    public async Task<List<TestConfiguration>> GetAllAsync()
    {
        try
        {
            var files = await _fileStorage.ListFilesAsync("configurations", "*.json");
            var configurations = new List<TestConfiguration>();

            foreach (var file in files)
            {
                var content = await _fileStorage.ReadFileAsync(file);
                if (content != null)
                {
                    try
                    {
                        var config = JsonSerializer.Deserialize<TestConfiguration>(content, _jsonOptions);
                        if (config != null)
                            configurations.Add(config);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Failed to deserialize configuration file: {File}", file);
                    }
                }
            }

            return configurations.OrderBy(c => c.Name).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all configurations");
            return new List<TestConfiguration>();
        }
    }

    public async Task<List<TestConfiguration>> GetActiveAsync()
    {
        var all = await GetAllAsync();
        return all.Where(c => c.IsActive).ToList();
    }

    public async Task<TestConfiguration?> GetByIdAsync(string id)
    {
        try
        {
            _logger.LogInformation("Looking for configuration with ID: {Id}", id);
            
            var fileName = $"{id}.json";
            var filePath = Path.Combine(_fileStorage.GetDirectoryPath("configurations"), fileName);
            
            _logger.LogInformation("Checking file path: {FilePath}", filePath);
            
            var content = await _fileStorage.ReadFileAsync(filePath);
            
            if (content == null) 
            {
                _logger.LogWarning("No content found for configuration file: {FilePath}", filePath);
                
                // Let's also check what files actually exist
                var configDir = _fileStorage.GetDirectoryPath("configurations");
                if (Directory.Exists(configDir))
                {
                    var existingFiles = Directory.GetFiles(configDir, "*.json");
                    _logger.LogInformation("Existing configuration files: {Files}", string.Join(", ", existingFiles.Select(Path.GetFileName)));
                }
                else
                {
                    _logger.LogWarning("Configuration directory does not exist: {ConfigDir}", configDir);
                }
                
                return null;
            }

            _logger.LogInformation("Configuration file content loaded, length: {ContentLength}", content.Length);
            
            var config = JsonSerializer.Deserialize<TestConfiguration>(content, _jsonOptions);
            
            if (config != null)
            {
                _logger.LogInformation("Configuration deserialized successfully: {Name}, Runner instances: {Instances}, users: {Users}, requests: {Requests}", 
                    config.Name, config.Runner?.Instances?.Count ?? 0, config.Runner?.Users?.Count ?? 0, config.Runner?.Requests?.Count ?? 0);
            }
            
            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get configuration by ID: {Id}", id);
            return null;
        }
    }

    public async Task<TestConfiguration?> GetByNameAsync(string name)
    {
        var all = await GetAllAsync();
        return all.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<TestConfiguration> CreateAsync(TestConfiguration configuration)
    {
        try
        {
            configuration.Id = Guid.NewGuid().ToString();
            configuration.CreatedAt = DateTime.UtcNow;
            configuration.ModifiedAt = DateTime.UtcNow;

            var json = JsonSerializer.Serialize(configuration, _jsonOptions);
            var fileName = $"{configuration.Id}.json";
            await _fileStorage.SaveConfigurationAsync(fileName, json);

            _logger.LogInformation("Created configuration: {Name} ({Id})", configuration.Name, configuration.Id);
            return configuration;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create configuration: {Name}", configuration.Name);
            throw;
        }
    }

    public async Task<TestConfiguration> UpdateAsync(TestConfiguration configuration)
    {
        try
        {
            configuration.ModifiedAt = DateTime.UtcNow;

            var json = JsonSerializer.Serialize(configuration, _jsonOptions);
            var fileName = $"{configuration.Id}.json";
            await _fileStorage.SaveConfigurationAsync(fileName, json);

            _logger.LogInformation("Updated configuration: {Name} ({Id})", configuration.Name, configuration.Id);
            return configuration;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update configuration: {Name} ({Id})", configuration.Name, configuration.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        try
        {
            var fileName = $"{id}.json";
            var filePath = Path.Combine(_fileStorage.GetDirectoryPath("configurations"), fileName);
            var deleted = await _fileStorage.DeleteFileAsync(filePath);

            if (deleted)
                _logger.LogInformation("Deleted configuration: {Id}", id);

            return deleted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete configuration: {Id}", id);
            return false;
        }
    }

    public async Task<List<TestConfiguration>> GetByTagsAsync(params string[] tags)
    {
        var all = await GetAllAsync();
        return all.Where(c => tags.Any(tag => c.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))).ToList();
    }

    public async Task<List<TestConfiguration>> SearchAsync(string searchTerm)
    {
        var all = await GetAllAsync();
        return all.Where(c => 
            c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            (c.Description?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
        ).ToList();
    }

    public async Task<string?> ExportAsync(string id)
    {
        try
        {
            var config = await GetByIdAsync(id);
            if (config == null) return null;

            return JsonSerializer.Serialize(config, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export configuration: {Id}", id);
            return null;
        }
    }

    public async Task<TestConfiguration> ImportAsync(string json, bool overwriteId = false)
    {
        try
        {
            var config = JsonSerializer.Deserialize<TestConfiguration>(json, _jsonOptions);
            if (config == null)
                throw new ArgumentException("Invalid JSON format");

            if (!overwriteId || string.IsNullOrEmpty(config.Id))
            {
                config.Id = Guid.NewGuid().ToString();
            }

            config.CreatedAt = DateTime.UtcNow;
            config.ModifiedAt = DateTime.UtcNow;

            return await CreateAsync(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import configuration");
            throw;
        }
    }

    public async Task<ValidationResult> ValidateAsync(TestConfiguration configuration)
    {
        var result = new ValidationResult { IsValid = true };

        // Basic validation
        if (string.IsNullOrWhiteSpace(configuration.Name))
        {
            result.Errors.Add("Configuration name is required");
            result.IsValid = false;
        }

        if (configuration.Iterations < 1 || configuration.Iterations > 1000)
        {
            result.Errors.Add("Iterations must be between 1 and 1000");
            result.IsValid = false;
        }

        if (configuration.MaxConcurrency < 1 || configuration.MaxConcurrency > 100)
        {
            result.Errors.Add("Max concurrency must be between 1 and 100");
            result.IsValid = false;
        }

        // Check for duplicate names
        var existing = await GetByNameAsync(configuration.Name);
        if (existing != null && existing.Id != configuration.Id)
        {
            result.Errors.Add("A configuration with this name already exists");
            result.IsValid = false;
        }

        // Validate runner configuration
        if (!configuration.Runner.IsValid())
        {
            result.Errors.Add("Runner configuration is invalid - check instances, requests, and users");
            result.IsValid = false;
        }

        // Warnings
        if (configuration.Iterations > 500)
        {
            result.Warnings.Add("High iteration count may result in long execution times");
        }

        if (configuration.MaxConcurrency > 50)
        {
            result.Warnings.Add("High concurrency may overwhelm target servers");
        }

        return result;
    }
}