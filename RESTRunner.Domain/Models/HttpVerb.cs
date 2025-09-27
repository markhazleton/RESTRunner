namespace RESTRunner.Domain.Models;

/// <summary>
/// HTTP verbs used for REST requests
/// </summary>
public enum HttpVerb
{
    /// <summary>
    /// HTTP GET method - retrieves data from the server
    /// </summary>
    GET = 0,
    
    /// <summary>
    /// HTTP POST method - submits data to the server
    /// </summary>
    POST = 1,
    
    /// <summary>
    /// HTTP PUT method - updates or creates a resource
    /// </summary>
    PUT = 2,
    
    /// <summary>
    /// HTTP DELETE method - removes a resource
    /// </summary>
    DELETE = 3,
    
    /// <summary>
    /// HTTP HEAD method - retrieves headers only
    /// </summary>
    HEAD = 4,
    
    /// <summary>
    /// HTTP OPTIONS method - describes communication options
    /// </summary>
    OPTIONS = 5,
    
    /// <summary>
    /// HTTP PATCH method - applies partial modifications to a resource
    /// </summary>
    PATCH = 6,
    
    /// <summary>
    /// HTTP MERGE method - merges data with existing resource
    /// </summary>
    MERGE = 7,
    
    /// <summary>
    /// HTTP COPY method - creates a duplicate of a resource
    /// </summary>
    COPY = 8
}
