
namespace RESTRunner.Domain.Models;

/// <summary>
/// REST Request class
/// </summary>
public class CompareRequest
{
    /// <summary>
    /// The path for the request
    /// </summary>
    public string? Path;
    /// <summary>
    /// The method (POST/GET/PUT/DELETE) also known as HttpVerb
    /// </summary>
    public HttpVerb RequestMethod;
    /// <summary>
    /// The template of the body of the request (for POST/PUT)
    /// </summary>
    public string? BodyTemplate;
    /// <summary>
    /// Boolean for if the request requires a client token for authorization
    /// </summary>
    public bool RequiresClientToken;
    /// <summary>
    /// List of properties to add tot he request header
    /// </summary>
    public List<CompareProperty> Headers = new();
    /// <summary>
    /// The body of the Request
    /// </summary>
    public CompareBody? Body = new();
}

