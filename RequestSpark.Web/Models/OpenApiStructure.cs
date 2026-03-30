namespace RequestSpark.Web.Models;

/// <summary>Full parsed representation of an OpenAPI / Swagger specification</summary>
public class OpenApiStructure
{
    public string Title { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SpecFormat { get; set; } = string.Empty;
    public List<string> Servers { get; set; } = new();
    public List<OpenApiTagGroup> TagGroups { get; set; } = new();
    public List<OpenApiEndpointInfo> AllEndpoints { get; set; } = new();
    public Dictionary<string, string> SecuritySchemes { get; set; } = new();
}

public class OpenApiTagGroup
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<OpenApiEndpointInfo> Endpoints { get; set; } = new();
}

public class OpenApiEndpointInfo
{
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string? OperationId { get; set; }
    public string? Summary { get; set; }
    public string? Description { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<OpenApiParameterInfo> Parameters { get; set; } = new();
    public OpenApiRequestBodyInfo? RequestBody { get; set; }
    public Dictionary<string, string> Responses { get; set; } = new();
    public bool IsDeprecated { get; set; }
    public List<string> SecurityRequirements { get; set; } = new();

    // Derived helpers
    public bool HasPathParams => Parameters.Any(p => p.In == "path");
    public bool HasQueryParams => Parameters.Any(p => p.In == "query");
    public bool HasHeaderParams => Parameters.Any(p => p.In == "header");
    public bool HasBody => RequestBody != null && new[] { "post", "put", "patch" }.Contains(Method.ToLower());
    public string UniqueKey => $"{Method.ToUpper()}:{Path}";
}

public class OpenApiParameterInfo
{
    public string Name { get; set; } = string.Empty;

    /// <summary>path | query | header | cookie</summary>
    public string In { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool Required { get; set; }
    public string Type { get; set; } = "string";
    public string? Format { get; set; }
    public string? DefaultValue { get; set; }
    public string? Example { get; set; }
    public List<string> EnumValues { get; set; } = new();
}

public class OpenApiRequestBodyInfo
{
    public string? Description { get; set; }
    public bool Required { get; set; }
    public string ContentType { get; set; } = "application/json";
    public string? SchemaJson { get; set; }
    public string? ExampleJson { get; set; }
}

