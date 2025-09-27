using RESTRunner.Domain.Constants;

namespace RESTRunner.Domain.Models;

/// <summary>
/// REST Request class
/// </summary>
public class CompareRequest
{
    private string? _path;
    
    /// <summary>
    /// The path for the request
    /// </summary>
    public string? Path 
    { 
        get => _path;
        set => _path = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
    
    /// <summary>
    /// The method (POST/GET/PUT/DELETE) also known as HttpVerb
    /// </summary>
    public HttpVerb RequestMethod { get; set; }
    
    /// <summary>
    /// The template of the body of the request (for POST/PUT)
    /// </summary>
    public string? BodyTemplate { get; set; }
    
    /// <summary>
    /// Boolean for if the request requires a client token for authorization
    /// </summary>
    public bool RequiresClientToken { get; set; }
    
    /// <summary>
    /// List of properties to add to the request header
    /// </summary>
    public List<CompareProperty> Headers { get; set; } = [];
    
    /// <summary>
    /// The body of the Request
    /// </summary>
    public CompareBody? Body { get; set; }
    
    /// <summary>
    /// Validates that the request has the minimum required properties
    /// </summary>
    /// <returns>True if the request is valid, false otherwise</returns>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(Path))
            return false;
            
        if (Path?.Length > DomainConstants.MaxRequestPathLength)
            return false;
            
        // POST/PUT requests with body template should have a body
        if ((RequestMethod == HttpVerb.POST || RequestMethod == HttpVerb.PUT) && 
            !string.IsNullOrWhiteSpace(BodyTemplate) && Body == null)
            return false;
            
        return true;
    }
    
    /// <summary>
    /// Determines if this request needs a request body
    /// </summary>
    /// <returns>True if the HTTP method typically includes a body</returns>
    public bool RequiresBody() => RequestMethod is HttpVerb.POST or HttpVerb.PUT or HttpVerb.PATCH;
}

