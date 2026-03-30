using RequestSpark.Web.Models;
using System.Text.Json;

namespace RequestSpark.Web.Services;

public partial class FileOpenApiService
{
    private void ExtractMetaFromRoot(JsonElement root, OpenApiValidationResult result)
    {
        if (root.TryGetProperty("openapi", out var openApiVersion))
        {
            result.SpecFormat = $"OpenAPI {openApiVersion.GetString()}";
            ExtractOpenApi3Meta(root, result);
        }
        else if (root.TryGetProperty("swagger", out var swaggerVersion))
        {
            result.SpecFormat = $"Swagger {swaggerVersion.GetString()}";
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
            result.Title = info.TryGetProperty("title", out var title) ? title.GetString() : null;
            result.Version = info.TryGetProperty("version", out var version) ? version.GetString() : null;
        }

        if (root.TryGetProperty("servers", out var servers))
        {
            foreach (var server in servers.EnumerateArray())
            {
                if (server.TryGetProperty("url", out var urlElement) && urlElement.GetString() is { } url && !string.IsNullOrWhiteSpace(url))
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
            result.Title = info.TryGetProperty("title", out var title) ? title.GetString() : null;
            result.Version = info.TryGetProperty("version", out var version) ? version.GetString() : null;
        }

        var host = root.TryGetProperty("host", out var hostElement) ? hostElement.GetString() : null;
        var basePath = root.TryGetProperty("basePath", out var basePathElement) ? basePathElement.GetString() ?? "/" : "/";
        var scheme = root.TryGetProperty("schemes", out var schemesElement) && schemesElement.GetArrayLength() > 0
            ? schemesElement[0].GetString() ?? "https"
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
        {
            if (tag.TryGetProperty("name", out var nameElement) && nameElement.GetString() is { Length: > 0 } name)
            {
                result.ApiTags.Add(name);
            }
        }
    }

    private static void ExtractSecuritySchemesMeta3(JsonElement root, OpenApiValidationResult result)
    {
        if (root.TryGetProperty("components", out var components) && components.TryGetProperty("securitySchemes", out var schemes))
        {
            foreach (var scheme in schemes.EnumerateObject())
            {
                result.SecuritySchemes.Add(scheme.Name);
            }
        }
    }

    private static void ExtractSecuritySchemesMeta2(JsonElement root, OpenApiValidationResult result)
    {
        if (root.TryGetProperty("securityDefinitions", out var definitions))
        {
            foreach (var definition in definitions.EnumerateObject())
            {
                result.SecuritySchemes.Add(definition.Name);
            }
        }
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
                var upper = method.ToUpperInvariant();
                if (!result.HttpMethods.Contains(upper)) result.HttpMethods.Add(upper);
            }
        }
    }

    private void ParseOpenApi3Structure(JsonElement root, OpenApiStructure structure)
    {
        if (root.TryGetProperty("info", out var info))
        {
            structure.Title = info.TryGetProperty("title", out var title) ? title.GetString() ?? string.Empty : string.Empty;
            structure.Version = info.TryGetProperty("version", out var version) ? version.GetString() ?? string.Empty : string.Empty;
            structure.Description = info.TryGetProperty("description", out var description) ? description.GetString() : null;
        }

        if (root.TryGetProperty("servers", out var servers))
        {
            foreach (var server in servers.EnumerateArray())
            {
                if (server.TryGetProperty("url", out var url) && url.GetString() is { } value)
                {
                    structure.Servers.Add(value);
                }
            }
        }

        var tagDescriptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (root.TryGetProperty("tags", out var tags))
        {
            foreach (var tag in tags.EnumerateArray())
            {
                var name = tag.TryGetProperty("name", out var nameElement) ? nameElement.GetString() ?? string.Empty : string.Empty;
                var description = tag.TryGetProperty("description", out var descriptionElement) ? descriptionElement.GetString() ?? string.Empty : string.Empty;
                if (!string.IsNullOrEmpty(name)) tagDescriptions[name] = description;
            }
        }

        if (root.TryGetProperty("components", out var components) && components.TryGetProperty("securitySchemes", out var securitySchemes))
        {
            foreach (var scheme in securitySchemes.EnumerateObject())
            {
                var type = scheme.Value.TryGetProperty("type", out var schemeType) ? schemeType.GetString() ?? scheme.Name : scheme.Name;
                structure.SecuritySchemes[scheme.Name] = type;
            }
        }

        if (!root.TryGetProperty("paths", out var paths)) return;

        var tagGroups = new Dictionary<string, OpenApiTagGroup>(StringComparer.OrdinalIgnoreCase);
        foreach (var pathItem in paths.EnumerateObject())
        {
            var sharedParams = new List<OpenApiParameterInfo>();
            try
            {
                if (pathItem.Value.TryGetProperty("parameters", out var sharedParamsElement))
                {
                    sharedParams = ParseParameters3(sharedParamsElement);
                }
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
                        Method = method.ToUpperInvariant(),
                        OperationId = operation.TryGetProperty("operationId", out var operationId) ? operationId.GetString() : null,
                        Summary = operation.TryGetProperty("summary", out var summary) ? summary.GetString() : null,
                        Description = operation.TryGetProperty("description", out var description) ? description.GetString() : null,
                        IsDeprecated = operation.TryGetProperty("deprecated", out var deprecated) && deprecated.GetBoolean()
                    };

                    if (operation.TryGetProperty("tags", out var operationTags))
                    {
                        foreach (var tag in operationTags.EnumerateArray())
                        {
                            if (tag.GetString() is { } tagName) endpoint.Tags.Add(tagName);
                        }
                    }

                    var operationParams = operation.TryGetProperty("parameters", out var operationParamsElement)
                        ? ParseParameters3(operationParamsElement)
                        : new List<OpenApiParameterInfo>();

                    var mergedParams = new List<OpenApiParameterInfo>(sharedParams);
                    foreach (var operationParam in operationParams)
                    {
                        var existing = mergedParams.FirstOrDefault(parameter => parameter.Name == operationParam.Name && parameter.In == operationParam.In);
                        if (existing is not null) mergedParams.Remove(existing);
                        mergedParams.Add(operationParam);
                    }
                    endpoint.Parameters = mergedParams;

                    if (operation.TryGetProperty("requestBody", out var requestBody))
                    {
                        endpoint.RequestBody = ParseRequestBody3(requestBody);
                    }

                    if (operation.TryGetProperty("responses", out var responses))
                    {
                        foreach (var response in responses.EnumerateObject())
                        {
                            var responseDescription = response.Value.TryGetProperty("description", out var responseDesc) ? responseDesc.GetString() ?? string.Empty : string.Empty;
                            endpoint.Responses[response.Name] = responseDescription;
                        }
                    }

                    if (operation.TryGetProperty("security", out var security))
                    {
                        foreach (var item in security.EnumerateArray())
                        {
                            foreach (var property in item.EnumerateObject())
                            {
                                endpoint.SecurityRequirements.Add(property.Name);
                            }
                        }
                    }

                    structure.AllEndpoints.Add(endpoint);

                    var groupName = endpoint.Tags.FirstOrDefault() ?? "Default";
                    if (!tagGroups.TryGetValue(groupName, out var group))
                    {
                        group = new OpenApiTagGroup
                        {
                            Name = groupName,
                            Description = tagDescriptions.TryGetValue(groupName, out var groupDescription) ? groupDescription : null
                        };
                        tagGroups[groupName] = group;
                    }

                    group.Endpoints.Add(endpoint);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse operation {Method} {Path} — skipping", method.ToUpperInvariant(), pathItem.Name);
                }
            }
        }

        structure.TagGroups = tagGroups.Values.OrderBy(group => group.Name == "Default" ? "ZZZ" : group.Name).ToList();
    }

    private void ParseSwagger2Structure(JsonElement root, OpenApiStructure structure)
    {
        if (root.TryGetProperty("info", out var info))
        {
            structure.Title = info.TryGetProperty("title", out var title) ? title.GetString() ?? string.Empty : string.Empty;
            structure.Version = info.TryGetProperty("version", out var version) ? version.GetString() ?? string.Empty : string.Empty;
            structure.Description = info.TryGetProperty("description", out var description) ? description.GetString() : null;
        }

        var host = root.TryGetProperty("host", out var hostElement) ? hostElement.GetString() : null;
        var basePath = root.TryGetProperty("basePath", out var basePathElement) ? basePathElement.GetString() ?? "/" : "/";
        var scheme = root.TryGetProperty("schemes", out var schemesElement) && schemesElement.GetArrayLength() > 0
            ? schemesElement[0].GetString() ?? "https"
            : "https";

        if (!string.IsNullOrEmpty(host))
        {
            structure.Servers.Add($"{scheme}://{host}{basePath}");
        }

        var tagDescriptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (root.TryGetProperty("tags", out var tags))
        {
            foreach (var tag in tags.EnumerateArray())
            {
                var name = tag.TryGetProperty("name", out var nameElement) ? nameElement.GetString() ?? string.Empty : string.Empty;
                var description = tag.TryGetProperty("description", out var descriptionElement) ? descriptionElement.GetString() ?? string.Empty : string.Empty;
                if (!string.IsNullOrEmpty(name)) tagDescriptions[name] = description;
            }
        }

        if (root.TryGetProperty("securityDefinitions", out var definitions))
        {
            foreach (var definition in definitions.EnumerateObject())
            {
                structure.SecuritySchemes[definition.Name] = definition.Value.TryGetProperty("type", out var type) ? type.GetString() ?? definition.Name : definition.Name;
            }
        }

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
                    Method = method.ToUpperInvariant(),
                    OperationId = operation.TryGetProperty("operationId", out var operationId) ? operationId.GetString() : null,
                    Summary = operation.TryGetProperty("summary", out var summary) ? summary.GetString() : null,
                    Description = operation.TryGetProperty("description", out var operationDescription) ? operationDescription.GetString() : null,
                };

                if (operation.TryGetProperty("tags", out var operationTags))
                {
                    foreach (var tag in operationTags.EnumerateArray())
                    {
                        if (tag.GetString() is { } tagName) endpoint.Tags.Add(tagName);
                    }
                }

                if (operation.TryGetProperty("parameters", out var parameters))
                {
                    endpoint.Parameters = ParseParameters2(parameters);
                }

                if (operation.TryGetProperty("responses", out var responses))
                {
                    foreach (var response in responses.EnumerateObject())
                    {
                        var responseText = response.Value.TryGetProperty("description", out var responseDescription) ? responseDescription.GetString() ?? string.Empty : string.Empty;
                        endpoint.Responses[response.Name] = responseText;
                    }
                }

                structure.AllEndpoints.Add(endpoint);

                var groupName = endpoint.Tags.FirstOrDefault() ?? "Default";
                if (!tagGroups.TryGetValue(groupName, out var group))
                {
                    group = new OpenApiTagGroup
                    {
                        Name = groupName,
                        Description = tagDescriptions.TryGetValue(groupName, out var groupDescription) ? groupDescription : null
                    };
                    tagGroups[groupName] = group;
                }

                group.Endpoints.Add(endpoint);
            }
        }

        structure.TagGroups = tagGroups.Values.OrderBy(group => group.Name == "Default" ? "ZZZ" : group.Name).ToList();
    }
}
