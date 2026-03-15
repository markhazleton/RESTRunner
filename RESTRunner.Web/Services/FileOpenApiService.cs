using System.Text.Json;
using RESTRunner.Web.Models;

namespace RESTRunner.Web.Services;

/// <summary>
/// File-based implementation of IOpenApiService.
/// Handles storage and parsing of OpenAPI 3.x and Swagger 2.0 specifications.
/// </summary>
public class FileOpenApiService : IOpenApiService
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

    // ── Private: metadata extraction (shared by validation) ──────────────────

    private void ExtractMetaFromRoot(JsonElement root, OpenApiValidationResult result)
    {
        if (root.TryGetProperty("openapi", out var oaVer))
        {
            result.SpecFormat = $"OpenAPI {oaVer.GetString()}";
            ExtractOpenApi3Meta(root, result);
        }
        else if (root.TryGetProperty("swagger", out var swVer))
        {
            result.SpecFormat = $"Swagger {swVer.GetString()}";
            ExtractSwagger2Meta(root, result);
        }
        else
        {
            result.Errors.Add("Not a valid OpenAPI or Swagger file. Missing 'openapi' or 'swagger' root property.");
        }
    }

    private void ExtractOpenApi3Meta(JsonElement root, OpenApiValidationResult result)
    {
        if (root.TryGetProperty("info", out var info))
        {
            result.Title = info.TryGetProperty("title", out var t) ? t.GetString() : null;
            result.Version = info.TryGetProperty("version", out var v) ? v.GetString() : null;
        }

        if (root.TryGetProperty("servers", out var servers))
        {
            foreach (var srv in servers.EnumerateArray())
            {
                if (srv.TryGetProperty("url", out var u) && u.GetString() is { } url && !string.IsNullOrWhiteSpace(url))
                {
                    result.Servers.Add(url);
                    result.DefaultBaseUrl ??= url;
                }
            }
        }

        ExtractTagsMeta(root, result);
        ExtractSecuritySchemesMeta3(root, result);
        CountPathsMeta(root, result);
    }

    private void ExtractSwagger2Meta(JsonElement root, OpenApiValidationResult result)
    {
        if (root.TryGetProperty("info", out var info))
        {
            result.Title = info.TryGetProperty("title", out var t) ? t.GetString() : null;
            result.Version = info.TryGetProperty("version", out var v) ? v.GetString() : null;
        }

        // Build base URL
        var host = root.TryGetProperty("host", out var h) ? h.GetString() : null;
        var basePath = root.TryGetProperty("basePath", out var bp) ? bp.GetString() ?? "/" : "/";
        var scheme = root.TryGetProperty("schemes", out var sc) && sc.GetArrayLength() > 0
            ? sc[0].GetString() ?? "https"
            : "https";

        if (!string.IsNullOrEmpty(host))
        {
            var url = $"{scheme}://{host}{basePath}";
            result.Servers.Add(url);
            result.DefaultBaseUrl = url;
        }

        ExtractTagsMeta(root, result);
        ExtractSecuritySchemesMeta2(root, result);
        CountPathsMeta(root, result);
    }

    private static void ExtractTagsMeta(JsonElement root, OpenApiValidationResult result)
    {
        if (!root.TryGetProperty("tags", out var tags)) return;
        foreach (var tag in tags.EnumerateArray())
            if (tag.TryGetProperty("name", out var n) && n.GetString() is { Length: > 0 } name)
                result.ApiTags.Add(name);
    }

    private static void ExtractSecuritySchemesMeta3(JsonElement root, OpenApiValidationResult result)
    {
        if (root.TryGetProperty("components", out var comps) && comps.TryGetProperty("securitySchemes", out var schemes))
            foreach (var s in schemes.EnumerateObject())
                result.SecuritySchemes.Add(s.Name);
    }

    private static void ExtractSecuritySchemesMeta2(JsonElement root, OpenApiValidationResult result)
    {
        if (root.TryGetProperty("securityDefinitions", out var defs))
            foreach (var d in defs.EnumerateObject())
                result.SecuritySchemes.Add(d.Name);
    }

    private static void CountPathsMeta(JsonElement root, OpenApiValidationResult result)
    {
        if (!root.TryGetProperty("paths", out var paths)) return;
        foreach (var path in paths.EnumerateObject())
        {
            foreach (var method in SupportedMethods)
            {
                if (!path.Value.TryGetProperty(method, out _)) continue;
                result.EndpointCount++;
                var upper = method.ToUpper();
                if (!result.HttpMethods.Contains(upper)) result.HttpMethods.Add(upper);
            }
        }
    }

    // ── Private: full structure parsing ─────────────────────────────────────

    private void ParseOpenApi3Structure(JsonElement root, OpenApiStructure structure)
    {
        if (root.TryGetProperty("info", out var info))
        {
            structure.Title = info.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "";
            structure.Version = info.TryGetProperty("version", out var v) ? v.GetString() ?? "" : "";
            structure.Description = info.TryGetProperty("description", out var d) ? d.GetString() : null;
        }

        if (root.TryGetProperty("servers", out var servers))
            foreach (var srv in servers.EnumerateArray())
                if (srv.TryGetProperty("url", out var u) && u.GetString() is { } url)
                    structure.Servers.Add(url);

        // Tag descriptions
        var tagDescriptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (root.TryGetProperty("tags", out var tags))
            foreach (var tag in tags.EnumerateArray())
            {
                var name = tag.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                var desc = tag.TryGetProperty("description", out var td) ? td.GetString() ?? "" : "";
                if (!string.IsNullOrEmpty(name)) tagDescriptions[name] = desc;
            }

        // Security schemes
        if (root.TryGetProperty("components", out var comps) && comps.TryGetProperty("securitySchemes", out var schemes))
            foreach (var s in schemes.EnumerateObject())
            {
                var type = s.Value.TryGetProperty("type", out var st) ? st.GetString() ?? s.Name : s.Name;
                structure.SecuritySchemes[s.Name] = type;
            }

        // Paths
        if (!root.TryGetProperty("paths", out var paths)) return;

        var tagGroups = new Dictionary<string, OpenApiTagGroup>(StringComparer.OrdinalIgnoreCase);

        foreach (var pathItem in paths.EnumerateObject())
        {
            // Path-level parameters (shared across all operations)
            var sharedParams = new List<OpenApiParameterInfo>();
            try
            {
                if (pathItem.Value.TryGetProperty("parameters", out var sharedParamsEl))
                    sharedParams = ParseParameters3(sharedParamsEl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse shared parameters for path {Path}", pathItem.Name);
            }

            foreach (var method in SupportedMethods)
            {
                if (!pathItem.Value.TryGetProperty(method, out var operation)) continue;

                try
                {
                    var endpoint = new OpenApiEndpointInfo
                    {
                        Path = pathItem.Name,
                        Method = method.ToUpper(),
                        OperationId = operation.TryGetProperty("operationId", out var oid) ? oid.GetString() : null,
                        Summary = operation.TryGetProperty("summary", out var sum) ? sum.GetString() : null,
                        Description = operation.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                        IsDeprecated = operation.TryGetProperty("deprecated", out var dep) && dep.GetBoolean()
                    };

                    // Tags
                    if (operation.TryGetProperty("tags", out var opTags))
                        foreach (var tag in opTags.EnumerateArray())
                            if (tag.GetString() is { } tn) endpoint.Tags.Add(tn);

                    // Parameters: merge shared then override with operation-level
                    var opParams = operation.TryGetProperty("parameters", out var opParamsEl)
                        ? ParseParameters3(opParamsEl)
                        : new List<OpenApiParameterInfo>();

                    // Operation params override shared params by name+in
                    var merged = new List<OpenApiParameterInfo>(sharedParams);
                    foreach (var op in opParams)
                    {
                        var existing = merged.FirstOrDefault(p => p.Name == op.Name && p.In == op.In);
                        if (existing is not null) merged.Remove(existing);
                        merged.Add(op);
                    }
                    endpoint.Parameters = merged;

                    // Request body
                    if (operation.TryGetProperty("requestBody", out var rb))
                        endpoint.RequestBody = ParseRequestBody3(rb);

                    // Responses
                    if (operation.TryGetProperty("responses", out var responses))
                        foreach (var resp in responses.EnumerateObject())
                        {
                            var respDesc = resp.Value.TryGetProperty("description", out var rd) ? rd.GetString() ?? "" : "";
                            endpoint.Responses[resp.Name] = respDesc;
                        }

                    // Security
                    if (operation.TryGetProperty("security", out var sec))
                        foreach (var s in sec.EnumerateArray())
                            foreach (var sp in s.EnumerateObject())
                                endpoint.SecurityRequirements.Add(sp.Name);

                    structure.AllEndpoints.Add(endpoint);

                    // Group by first tag (or "Default")
                    var groupName = endpoint.Tags.FirstOrDefault() ?? "Default";
                    if (!tagGroups.TryGetValue(groupName, out var group))
                    {
                        group = new OpenApiTagGroup
                        {
                            Name = groupName,
                            Description = tagDescriptions.TryGetValue(groupName, out var gd) ? gd : null
                        };
                        tagGroups[groupName] = group;
                    }
                    group.Endpoints.Add(endpoint);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse operation {Method} {Path} — skipping", method.ToUpper(), pathItem.Name);
                }
            }
        }

        structure.TagGroups = tagGroups.Values.OrderBy(g => g.Name == "Default" ? "ZZZ" : g.Name).ToList();
    }

    private void ParseSwagger2Structure(JsonElement root, OpenApiStructure structure)
    {
        if (root.TryGetProperty("info", out var info))
        {
            structure.Title = info.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "";
            structure.Version = info.TryGetProperty("version", out var v) ? v.GetString() ?? "" : "";
            structure.Description = info.TryGetProperty("description", out var d) ? d.GetString() : null;
        }

        var host = root.TryGetProperty("host", out var h) ? h.GetString() : null;
        var basePath = root.TryGetProperty("basePath", out var bp) ? bp.GetString() ?? "/" : "/";
        var scheme = root.TryGetProperty("schemes", out var sc) && sc.GetArrayLength() > 0
            ? sc[0].GetString() ?? "https" : "https";

        if (!string.IsNullOrEmpty(host))
            structure.Servers.Add($"{scheme}://{host}{basePath}");

        var tagDescriptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (root.TryGetProperty("tags", out var tags))
            foreach (var tag in tags.EnumerateArray())
            {
                var name = tag.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                var desc = tag.TryGetProperty("description", out var td) ? td.GetString() ?? "" : "";
                if (!string.IsNullOrEmpty(name)) tagDescriptions[name] = desc;
            }

        if (root.TryGetProperty("securityDefinitions", out var defs))
            foreach (var d in defs.EnumerateObject())
                structure.SecuritySchemes[d.Name] = d.Value.TryGetProperty("type", out var st) ? st.GetString() ?? d.Name : d.Name;

        if (!root.TryGetProperty("paths", out var paths)) return;

        var tagGroups = new Dictionary<string, OpenApiTagGroup>(StringComparer.OrdinalIgnoreCase);

        foreach (var pathItem in paths.EnumerateObject())
        {
            foreach (var method in SupportedMethods)
            {
                if (!pathItem.Value.TryGetProperty(method, out var operation)) continue;

                var endpoint = new OpenApiEndpointInfo
                {
                    Path = pathItem.Name,
                    Method = method.ToUpper(),
                    OperationId = operation.TryGetProperty("operationId", out var oid) ? oid.GetString() : null,
                    Summary = operation.TryGetProperty("summary", out var sum) ? sum.GetString() : null,
                    Description = operation.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                };

                if (operation.TryGetProperty("tags", out var opTags))
                    foreach (var tag in opTags.EnumerateArray())
                        if (tag.GetString() is { } tn) endpoint.Tags.Add(tn);

                if (operation.TryGetProperty("parameters", out var paramsEl))
                    endpoint.Parameters = ParseParameters2(paramsEl);

                if (operation.TryGetProperty("responses", out var responses))
                    foreach (var resp in responses.EnumerateObject())
                    {
                        var rd = resp.Value.TryGetProperty("description", out var rdEl) ? rdEl.GetString() ?? "" : "";
                        endpoint.Responses[resp.Name] = rd;
                    }

                structure.AllEndpoints.Add(endpoint);

                var groupName = endpoint.Tags.FirstOrDefault() ?? "Default";
                if (!tagGroups.TryGetValue(groupName, out var group))
                {
                    group = new OpenApiTagGroup
                    {
                        Name = groupName,
                        Description = tagDescriptions.TryGetValue(groupName, out var gd) ? gd : null
                    };
                    tagGroups[groupName] = group;
                }
                group.Endpoints.Add(endpoint);
            }
        }

        structure.TagGroups = tagGroups.Values.OrderBy(g => g.Name == "Default" ? "ZZZ" : g.Name).ToList();
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
                Name = p.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "",
                In = p.TryGetProperty("in", out var i) ? i.GetString() ?? "" : "",
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

            var inValue = p.TryGetProperty("in", out var i) ? i.GetString() ?? "" : "";
            var param = new OpenApiParameterInfo
            {
                Name = p.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "",
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
