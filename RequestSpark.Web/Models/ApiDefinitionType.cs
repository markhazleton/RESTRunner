namespace RequestSpark.Web.Models;

/// <summary>
/// Supported API definition sources that can generate RequestSpark request models.
/// </summary>
public enum ApiDefinitionType
{
    None = 0,
    PostmanCollection = 1,
    OpenApiSpec = 2
}

