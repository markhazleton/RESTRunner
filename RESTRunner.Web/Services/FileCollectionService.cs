using System.Text.Json;
using RESTRunner.Web.Models;
using RESTRunner.Web.Models.ViewModels;

namespace RESTRunner.Web.Services;

/// <summary>
/// File-based implementation of collection service
/// </summary>
public class FileCollectionService : ICollectionService
{
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<FileCollectionService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public FileCollectionService(IFileStorageService fileStorage, ILogger<FileCollectionService> logger)
    {
        _fileStorage = fileStorage;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    public async Task<List<CollectionMetadata>> GetAllAsync()
    {
        try
        {
            var files = await _fileStorage.ListFilesAsync("collections", "*_metadata.json");
            var collections = new List<CollectionMetadata>();

            foreach (var file in files)
            {
                var content = await _fileStorage.ReadFileAsync(file);
                if (content != null)
                {
                    try
                    {
                        var metadata = JsonSerializer.Deserialize<CollectionMetadata>(content, _jsonOptions);
                        if (metadata != null)
                            collections.Add(metadata);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Failed to deserialize collection metadata file: {File}", file);
                    }
                }
            }

            return collections.OrderBy(c => c.Name).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all collections");
            return new List<CollectionMetadata>();
        }
    }

    public async Task<List<CollectionMetadata>> GetActiveAsync()
    {
        var all = await GetAllAsync();
        return all.Where(c => c.IsActive).ToList();
    }

    public async Task<CollectionMetadata?> GetByIdAsync(string id)
    {
        try
        {
            var fileName = $"{id}_metadata.json";
            var filePath = Path.Combine(_fileStorage.GetDirectoryPath("collections"), fileName);
            var content = await _fileStorage.ReadFileAsync(filePath);
            
            if (content == null) return null;

            return JsonSerializer.Deserialize<CollectionMetadata>(content, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get collection by ID: {Id}", id);
            return null;
        }
    }

    public async Task<CollectionMetadata> UploadAsync(IFormFile file, CollectionMetadata metadata)
    {
        try
        {
            metadata.Id = Guid.NewGuid().ToString();
            metadata.UploadedAt = DateTime.UtcNow;

            // Save the collection file
            var collectionFileName = $"{metadata.Id}_collection.json";
            var collectionPath = await _fileStorage.SaveCollectionAsync(collectionFileName, file);
            metadata.FilePath = collectionPath;

            // Calculate file hash
            using var stream = file.OpenReadStream();
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashBytes = await sha256.ComputeHashAsync(stream);
            metadata.ContentHash = Convert.ToBase64String(hashBytes);

            // Save metadata
            var metadataJson = JsonSerializer.Serialize(metadata, _jsonOptions);
            var metadataFileName = $"{metadata.Id}_metadata.json";
            await _fileStorage.SaveCollectionAsync(metadataFileName, metadataJson);

            _logger.LogInformation("Uploaded collection: {Name} ({Id})", metadata.Name, metadata.Id);
            return metadata;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload collection: {Name}", metadata.Name);
            throw;
        }
    }

    public async Task<CollectionMetadata> UpdateAsync(CollectionMetadata metadata)
    {
        try
        {
            var metadataJson = JsonSerializer.Serialize(metadata, _jsonOptions);
            var metadataFileName = $"{metadata.Id}_metadata.json";
            await _fileStorage.SaveCollectionAsync(metadataFileName, metadataJson);

            _logger.LogInformation("Updated collection metadata: {Name} ({Id})", metadata.Name, metadata.Id);
            return metadata;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update collection: {Name} ({Id})", metadata.Name, metadata.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        try
        {
            var metadata = await GetByIdAsync(id);
            if (metadata == null) return false;

            // Delete collection file
            if (!string.IsNullOrEmpty(metadata.FilePath) && await _fileStorage.FileExistsAsync(metadata.FilePath))
            {
                await _fileStorage.DeleteFileAsync(metadata.FilePath);
            }

            // Delete metadata file
            var metadataFileName = $"{id}_metadata.json";
            var metadataPath = Path.Combine(_fileStorage.GetDirectoryPath("collections"), metadataFileName);
            var deleted = await _fileStorage.DeleteFileAsync(metadataPath);

            if (deleted)
                _logger.LogInformation("Deleted collection: {Id}", id);

            return deleted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete collection: {Id}", id);
            return false;
        }
    }

    public async Task<string?> GetContentAsync(string id)
    {
        try
        {
            var metadata = await GetByIdAsync(id);
            if (metadata == null || string.IsNullOrEmpty(metadata.FilePath)) return null;

            return await _fileStorage.ReadFileAsync(metadata.FilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get collection content: {Id}", id);
            return null;
        }
    }

    public async Task<CollectionStructure?> GetStructureAsync(string id)
    {
        try
        {
            var content = await GetContentAsync(id);
            if (content == null) return null;

            // Parse the Postman collection and extract structure
            var collection = JsonSerializer.Deserialize<JsonElement>(content);
            
            var structure = new CollectionStructure
            {
                Name = collection.TryGetProperty("info", out var info) && info.TryGetProperty("name", out var name) 
                    ? name.GetString() ?? "Unknown" : "Unknown",
                Description = info.TryGetProperty("description", out var desc) 
                    ? desc.GetString() : null
            };

            // Extract requests and folders (simplified)
            if (collection.TryGetProperty("item", out var items))
            {
                foreach (var item in items.EnumerateArray())
                {
                    ParseCollectionItem(item, structure);
                }
            }

            return structure;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse collection structure: {Id}", id);
            return null;
        }
    }

    public async Task<CollectionValidationResult> ValidateAsync(IFormFile file)
    {
        var result = new CollectionValidationResult();

        try
        {
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();

            return await ValidateAsync(content);
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.Errors.Add($"Failed to read file: {ex.Message}");
            return result;
        }
    }

    public async Task<CollectionValidationResult> ValidateAsync(string json)
    {
        var result = new CollectionValidationResult();

        try
        {
            var collection = JsonSerializer.Deserialize<JsonElement>(json);

            // Validate basic structure
            if (!collection.TryGetProperty("info", out var info))
            {
                result.Errors.Add("Collection must have an 'info' section");
                result.IsValid = false;
                return result;
            }

            // Extract basic information
            if (info.TryGetProperty("name", out var name))
                result.CollectionName = name.GetString();

            if (info.TryGetProperty("_postman_id", out var postmanId))
                result.PostmanId = postmanId.GetString();

            if (info.TryGetProperty("schema", out var schema))
                result.SchemaVersion = schema.GetString();

            // Count requests and extract details
            if (collection.TryGetProperty("item", out var items))
            {
                CountRequests(items, result);
            }

            // Validation rules
            if (string.IsNullOrEmpty(result.CollectionName))
            {
                result.Warnings.Add("Collection name is not specified");
            }

            if (result.RequestCount == 0)
            {
                result.Warnings.Add("Collection contains no requests");
            }

            result.IsValid = result.Errors.Count == 0;
        }
        catch (JsonException ex)
        {
            result.IsValid = false;
            result.Errors.Add($"Invalid JSON format: {ex.Message}");
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.Errors.Add($"Validation failed: {ex.Message}");
        }

        return result;
    }

    public async Task<List<CollectionMetadata>> GetByTagsAsync(params string[] tags)
    {
        var all = await GetAllAsync();
        return all.Where(c => tags.Any(tag => c.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))).ToList();
    }

    public async Task<List<CollectionMetadata>> SearchAsync(string searchTerm)
    {
        var all = await GetAllAsync();
        return all.Where(c => 
            c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            (c.Description?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
        ).ToList();
    }

    #region Helper Methods

    private void ParseCollectionItem(JsonElement item, CollectionStructure structure)
    {
        try
        {
            if (item.TryGetProperty("request", out var request))
            {
                // This is a request
                var collectionRequest = new CollectionRequest();

                if (item.TryGetProperty("name", out var name))
                    collectionRequest.Name = name.GetString() ?? "Unnamed Request";

                if (request.TryGetProperty("method", out var method))
                    collectionRequest.Method = method.GetString() ?? "GET";

                if (request.TryGetProperty("url", out var url))
                {
                    if (url.ValueKind == JsonValueKind.String)
                    {
                        collectionRequest.Url = url.GetString() ?? "";
                    }
                    else if (url.TryGetProperty("raw", out var rawUrl))
                    {
                        collectionRequest.Url = rawUrl.GetString() ?? "";
                    }
                }

                structure.Requests.Add(collectionRequest);
            }
            else if (item.TryGetProperty("item", out var nestedItems))
            {
                // This is a folder
                var folder = new CollectionFolder();
                if (item.TryGetProperty("name", out var folderName))
                    folder.Name = folderName.GetString() ?? "Unnamed Folder";

                foreach (var nestedItem in nestedItems.EnumerateArray())
                {
                    ParseFolderItem(nestedItem, folder);
                }

                structure.Folders.Add(folder);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse collection item");
        }
    }

    private void ParseFolderItem(JsonElement item, CollectionFolder folder)
    {
        try
        {
            if (item.TryGetProperty("request", out var request))
            {
                var collectionRequest = new CollectionRequest();

                if (item.TryGetProperty("name", out var name))
                    collectionRequest.Name = name.GetString() ?? "Unnamed Request";

                if (request.TryGetProperty("method", out var method))
                    collectionRequest.Method = method.GetString() ?? "GET";

                folder.Requests.Add(collectionRequest);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse folder item");
        }
    }

    private void CountRequests(JsonElement items, CollectionValidationResult result)
    {
        foreach (var item in items.EnumerateArray())
        {
            if (item.TryGetProperty("request", out var request))
            {
                result.RequestCount++;

                // Extract HTTP method
                if (request.TryGetProperty("method", out var method))
                {
                    var methodName = method.GetString();
                    if (!string.IsNullOrEmpty(methodName) && !result.HttpMethods.Contains(methodName))
                    {
                        result.HttpMethods.Add(methodName);
                    }
                }

                // Look for environment variables in URL
                if (request.TryGetProperty("url", out var url))
                {
                    var urlString = url.ValueKind == JsonValueKind.String 
                        ? url.GetString() 
                        : url.TryGetProperty("raw", out var rawUrl) ? rawUrl.GetString() : null;

                    if (!string.IsNullOrEmpty(urlString))
                    {
                        ExtractVariables(urlString, result.EnvironmentVariables);
                    }
                }
            }
            else if (item.TryGetProperty("item", out var nestedItems))
            {
                // Recursively count requests in folders
                CountRequests(nestedItems, result);
            }
        }
    }

    private void ExtractVariables(string text, List<string> variables)
    {
        var matches = System.Text.RegularExpressions.Regex.Matches(text, @"\{\{([^}]+)\}\}");
        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var variable = match.Groups[1].Value;
            if (!variables.Contains(variable))
            {
                variables.Add(variable);
            }
        }
    }

    #endregion
}