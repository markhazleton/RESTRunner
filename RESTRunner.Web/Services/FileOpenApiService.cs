using RESTRunner.Web.Models;
using System.Text.Json;

namespace RESTRunner.Web.Services;

/// <summary>
/// File-based implementation of IOpenApiService.
/// Handles storage and parsing of OpenAPI 3.x and Swagger 2.0 specifications.
/// </summary>
public partial class FileOpenApiService : IOpenApiService
{
    private static readonly string[] SupportedMethods = ["get", "post", "put", "delete", "patch", "head", "options", "trace"];

    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<FileOpenApiService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public FileOpenApiService(IFileStorageService fileStorage, ILogger<FileOpenApiService> logger)
    {
        _fileStorage = fileStorage;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    // ── CRUD ────────────────────────────────────────────────────────────────

    public async Task<List<OpenApiSpec>> GetAllAsync()
    {
        try
        {
            var files = await _fileStorage.ListFilesAsync("openapi", "*_metadata.json");
            var specs = new List<OpenApiSpec>();

            foreach (var file in files)
            {
                var content = await _fileStorage.ReadFileAsync(file);
                if (content is null) continue;
                try
                {
                    var spec = JsonSerializer.Deserialize<OpenApiSpec>(content, _jsonOptions);
                    if (spec is not null) specs.Add(spec);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize OpenAPI metadata: {File}", file);
                }
            }

            return specs.OrderBy(s => s.Title).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all OpenAPI specs");
            return new List<OpenApiSpec>();
        }
    }

    public async Task<List<OpenApiSpec>> GetActiveAsync()
    {
        var all = await GetAllAsync();
        return all.Where(s => s.IsActive).ToList();
    }

    public async Task<OpenApiSpec?> GetByIdAsync(string id)
    {
        try
        {
            var metaPath = Path.Combine(_fileStorage.GetDirectoryPath("openapi"), $"{id}_metadata.json");
            var content = await _fileStorage.ReadFileAsync(metaPath);
            if (content is null) return null;
            return JsonSerializer.Deserialize<OpenApiSpec>(content, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get OpenAPI spec by ID: {Id}", id);
            return null;
        }
    }

    public async Task<OpenApiSpec> UploadAsync(IFormFile file, OpenApiSpec metadata)
    {
        try
        {
            metadata.Id = Guid.NewGuid().ToString();
            metadata.UploadedAt = DateTime.UtcNow;

            // Save spec file
            var specFileName = $"{metadata.Id}_spec.json";
            var specPath = await _fileStorage.SaveOpenApiAsync(specFileName, file);
            metadata.FilePath = specPath;

            // Compute hash
            using var stream = file.OpenReadStream();
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashBytes = await sha256.ComputeHashAsync(stream);
            metadata.ContentHash = Convert.ToBase64String(hashBytes);

            // Save metadata
            var metaJson = JsonSerializer.Serialize(metadata, _jsonOptions);
            await _fileStorage.SaveOpenApiAsync($"{metadata.Id}_metadata.json", metaJson);

            _logger.LogInformation("Uploaded OpenAPI spec: {Title} ({Id})", metadata.Title, metadata.Id);
            return metadata;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload OpenAPI spec: {Title}", metadata.Title);
            throw;
        }
    }

    public async Task<OpenApiSpec> UpdateAsync(OpenApiSpec spec)
    {
        try
        {
            var metaJson = JsonSerializer.Serialize(spec, _jsonOptions);
            await _fileStorage.SaveOpenApiAsync($"{spec.Id}_metadata.json", metaJson);
            _logger.LogInformation("Updated OpenAPI spec: {Title} ({Id})", spec.Title, spec.Id);
            return spec;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update OpenAPI spec: {Id}", spec.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        try
        {
            var spec = await GetByIdAsync(id);
            if (spec is null) return false;

            if (!string.IsNullOrEmpty(spec.FilePath) && await _fileStorage.FileExistsAsync(spec.FilePath))
                await _fileStorage.DeleteFileAsync(spec.FilePath);

            var metaPath = Path.Combine(_fileStorage.GetDirectoryPath("openapi"), $"{id}_metadata.json");
            var deleted = await _fileStorage.DeleteFileAsync(metaPath);

            if (deleted) _logger.LogInformation("Deleted OpenAPI spec: {Id}", id);
            return deleted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete OpenAPI spec: {Id}", id);
            return false;
        }
    }

    public async Task<string?> GetContentAsync(string id)
    {
        try
        {
            var spec = await GetByIdAsync(id);
            if (spec is null || string.IsNullOrEmpty(spec.FilePath)) return null;
            return await _fileStorage.ReadFileAsync(spec.FilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get OpenAPI content: {Id}", id);
            return null;
        }
    }

    public async Task<List<OpenApiSpec>> SearchAsync(string searchTerm)
    {
        var all = await GetAllAsync();
        return all.Where(s =>
            s.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            (s.Description?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
        ).ToList();
    }

    // ── Validation ───────────────────────────────────────────────────────────

    public async Task<OpenApiValidationResult> ValidateAsync(IFormFile file)
    {
        try
        {
            using var reader = new StreamReader(file.OpenReadStream());
            return await ValidateAsync(await reader.ReadToEndAsync());
        }
        catch (Exception ex)
        {
            return new OpenApiValidationResult { IsValid = false, Errors = { $"Failed to read file: {ex.Message}" } };
        }
    }

    public Task<OpenApiValidationResult> ValidateAsync(string json)
    {
        var result = new OpenApiValidationResult();
        try
        {
            var root = JsonSerializer.Deserialize<JsonElement>(json);
            ExtractMetaFromRoot(root, result);
            result.IsValid = result.Errors.Count == 0;
        }
        catch (JsonException ex)
        {
            result.IsValid = false;
            result.Errors.Add($"Invalid JSON: {ex.Message}");
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.Errors.Add($"Validation error: {ex.Message}");
        }
        return Task.FromResult(result);
    }

    // ── Structure parsing ────────────────────────────────────────────────────

    public async Task<OpenApiStructure?> GetStructureAsync(string id)
    {
        var content = await GetContentAsync(id);
        if (content is null) return null;

        try
        {
            var root = JsonSerializer.Deserialize<JsonElement>(content);
            var structure = new OpenApiStructure();

            if (root.TryGetProperty("openapi", out var oaVer))
            {
                structure.SpecFormat = $"OpenAPI {oaVer.GetString()}";
                ParseOpenApi3Structure(root, structure);
            }
            else if (root.TryGetProperty("swagger", out var swVer))
            {
                structure.SpecFormat = $"Swagger {swVer.GetString()}";
                ParseSwagger2Structure(root, structure);
            }
            else
            {
                _logger.LogWarning("Unknown spec format for {Id}", id);
                return null;
            }

            return structure;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse structure for spec {Id}", id);
            return null;
        }
    }

    // ── Private: parameter parsers ───────────────────────────────────────────

    /// <summary>
    /// Safely extracts a type string from a JSON Schema "type" property.
    /// Handles both plain strings ("integer") and OpenAPI 3.1 union arrays (["integer","null"]).
    /// </summary>
    private static string ExtractJsonSchemaType(JsonElement? schema)
    {
        if (schema is null) return "string";
        if (!schema.Value.TryGetProperty("type", out var typeEl)) return "string";
        if (typeEl.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in typeEl.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    var s = item.GetString();
                    if (s != null && s != "null") return s;
                }
            }
            return "string";
        }
        return typeEl.ValueKind == JsonValueKind.String ? typeEl.GetString() ?? "string" : "string";
    }

    private static List<OpenApiParameterInfo> ParseParameters3(JsonElement paramsEl)
    {
        var list = new List<OpenApiParameterInfo>();
        foreach (var p in paramsEl.EnumerateArray())
        {
            // Skip $ref parameters (would need to resolve — skip for now)
            if (p.TryGetProperty("$ref", out _)) continue;

            var schema = p.TryGetProperty("schema", out var s) ? s : (JsonElement?)null;
            var param = new OpenApiParameterInfo
            {
                Name = p.TryGetProperty("name", out var n) ? n.GetString() ?? string.Empty : string.Empty,
                In = p.TryGetProperty("in", out var i) ? i.GetString() ?? string.Empty : string.Empty,
                Description = p.TryGetProperty("description", out var d) ? d.GetString() : null,
                Required = p.TryGetProperty("required", out var r) && r.GetBoolean(),
                Type = ExtractJsonSchemaType(schema),
                Format = schema?.TryGetProperty("format", out var f) == true ? f.GetString() : null,
                DefaultValue = schema?.TryGetProperty("default", out var dv) == true ? dv.ToString() : null,
                Example = p.TryGetProperty("example", out var ex) ? ex.ToString()
                    : schema?.TryGetProperty("example", out var sex) == true ? sex.ToString() : null
            };

            if (schema?.TryGetProperty("enum", out var enumEl) == true)
                foreach (var ev in enumEl.EnumerateArray())
                    if (ev.GetString() is { } ev2) param.EnumValues.Add(ev2);

            if (!string.IsNullOrEmpty(param.Name)) list.Add(param);
        }
        return list;
    }

    private static List<OpenApiParameterInfo> ParseParameters2(JsonElement paramsEl)
    {
        var list = new List<OpenApiParameterInfo>();
        foreach (var p in paramsEl.EnumerateArray())
        {
            if (p.TryGetProperty("$ref", out _)) continue;

            var inValue = p.TryGetProperty("in", out var i) ? i.GetString() ?? string.Empty : string.Empty;
            var param = new OpenApiParameterInfo
            {
                Name = p.TryGetProperty("name", out var n) ? n.GetString() ?? string.Empty : string.Empty,
                In = inValue,
                Description = p.TryGetProperty("description", out var d) ? d.GetString() : null,
                Required = p.TryGetProperty("required", out var r) && r.GetBoolean(),
                Type = p.TryGetProperty("type", out var t) ? t.GetString() ?? "string" : "string",
                Format = p.TryGetProperty("format", out var f) ? f.GetString() : null,
                DefaultValue = p.TryGetProperty("default", out var dv) ? dv.ToString() : null,
            };

            if (p.TryGetProperty("enum", out var enumEl))
                foreach (var ev in enumEl.EnumerateArray())
                    if (ev.GetString() is { } ev2) param.EnumValues.Add(ev2);

            if (!string.IsNullOrEmpty(param.Name)) list.Add(param);
        }
        return list;
    }

    private static OpenApiRequestBodyInfo? ParseRequestBody3(JsonElement rb)
    {
        var body = new OpenApiRequestBodyInfo
        {
            Description = rb.TryGetProperty("description", out var d) ? d.GetString() : null,
            Required = rb.TryGetProperty("required", out var req) && req.GetBoolean()
        };

        if (!rb.TryGetProperty("content", out var content)) return body;

        // Prefer application/json, fall back to first content type
        JsonElement? mediaType = null;
        string? contentType = null;

        if (content.TryGetProperty("application/json", out var jsonMedia))
        {
            mediaType = jsonMedia;
            contentType = "application/json";
        }
        else
        {
            foreach (var ct in content.EnumerateObject())
            {
                mediaType = ct.Value;
                contentType = ct.Name;
                break;
            }
        }

        if (mediaType is null) return body;
        body.ContentType = contentType ?? "application/json";

        // Try to get example JSON
        if (mediaType.Value.TryGetProperty("example", out var example))
            body.ExampleJson = FormatJson(example.ToString());
        else if (mediaType.Value.TryGetProperty("examples", out var examples))
        {
            foreach (var ex in examples.EnumerateObject())
            {
                if (ex.Value.TryGetProperty("value", out var val))
                {
                    body.ExampleJson = FormatJson(val.ToString());
                    break;
                }
            }
        }
        else if (mediaType.Value.TryGetProperty("schema", out var schema) && schema.TryGetProperty("example", out var schEx))
            body.ExampleJson = FormatJson(schEx.ToString());

        // Schema as hint
        if (mediaType.Value.TryGetProperty("schema", out var schemaEl))
            body.SchemaJson = FormatJson(schemaEl.ToString());

        return body;
    }

    private static string FormatJson(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions { WriteIndented = true });
        }
        catch
        {
            return json;
        }
    }
}
