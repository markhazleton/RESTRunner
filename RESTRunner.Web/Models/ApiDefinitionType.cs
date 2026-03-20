namespace RESTRunner.Web.Models;

/// <summary>
/// Supported API definition sources that can generate RESTRunner request models.
/// </summary>
public enum ApiDefinitionType
{
    None = 0,
    PostmanCollection = 1,
    OpenApiSpec = 2
}
