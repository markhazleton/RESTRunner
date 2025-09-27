namespace RESTRunner.Domain.Models;

/// <summary>
/// Represents the result of a REST request comparison
/// </summary>
public record CompareResult
{
    /// <summary>
    /// How long did it take for the request to come back (in milliseconds)?
    /// </summary>
    public long Duration { get; init; }
    
    /// <summary>
    /// A deterministic HASH for the content returned from the request
    /// </summary>
    public int? Hash { get; init; }
    
    /// <summary>
    /// Name of Instance used for request
    /// </summary>
    public string? Instance { get; init; }
    
    /// <summary>
    /// When was the run completed (UTC)?
    /// </summary>
    public DateTime LastRunDate { get; init; } = DateTime.UtcNow;
    
    /// <summary>
    /// The path for the request
    /// </summary>
    public string? Request { get; init; }
    
    /// <summary>
    /// Code from the request response (i.e. 200 for OK)
    /// </summary>
    public string? ResultCode { get; init; }
    
    /// <summary>
    /// Session value for tracking
    /// </summary>
    public string? SessionId { get; init; }
    
    /// <summary>
    /// The description of the status from the request response
    /// </summary>
    public string? StatusDescription { get; init; }
    
    /// <summary>
    /// Was the request successful
    /// </summary>
    public bool Success { get; init; }
    
    /// <summary>
    /// User Name
    /// </summary>
    public string? UserName { get; init; }
    
    /// <summary>
    /// HttpVerb (GET/POST/DELETE/PUT)
    /// </summary>
    public string? Verb { get; init; }
    
    /// <summary>
    /// Additional metadata about the request/response
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }
    
    /// <summary>
    /// Indicates if this result represents an error condition
    /// </summary>
    public bool IsError => !Success || !string.IsNullOrEmpty(StatusDescription);
    
    /// <summary>
    /// Gets a formatted string representation of the result for logging
    /// </summary>
    /// <returns>Formatted string representation</returns>
    public override string ToString()
    {
        var status = Success ? "SUCCESS" : "FAILED";
        var duration = Duration > 0 ? $" ({Duration}ms)" : "";
        return $"[{status}] {Verb} {Request} -> {ResultCode}{duration}";
    }
    
    /// <summary>
    /// Creates a successful result
    /// </summary>
    /// <param name="instance">Instance name</param>
    /// <param name="request">Request path</param>
    /// <param name="verb">HTTP verb</param>
    /// <param name="resultCode">HTTP status code</param>
    /// <param name="duration">Request duration in milliseconds</param>
    /// <param name="sessionId">Session ID</param>
    /// <param name="hash">Content hash</param>
    /// <returns>A successful CompareResult</returns>
    public static CompareResult CreateSuccess(string instance, string request, string verb, 
        string resultCode, long duration, string? sessionId = null, int? hash = null)
    {
        return new CompareResult
        {
            Instance = instance,
            Request = request,
            Verb = verb,
            ResultCode = resultCode,
            Duration = duration,
            Success = true,
            SessionId = sessionId,
            Hash = hash,
            LastRunDate = DateTime.UtcNow
        };
    }
    
    /// <summary>
    /// Creates a failed result
    /// </summary>
    /// <param name="instance">Instance name</param>
    /// <param name="request">Request path</param>
    /// <param name="verb">HTTP verb</param>
    /// <param name="error">Error description</param>
    /// <param name="duration">Request duration in milliseconds</param>
    /// <param name="sessionId">Session ID</param>
    /// <returns>A failed CompareResult</returns>
    public static CompareResult CreateFailure(string instance, string request, string verb, 
        string error, long duration = 0, string? sessionId = null)
    {
        return new CompareResult
        {
            Instance = instance,
            Request = request,
            Verb = verb,
            StatusDescription = error,
            Duration = duration,
            Success = false,
            SessionId = sessionId,
            LastRunDate = DateTime.UtcNow
        };
    }
}
