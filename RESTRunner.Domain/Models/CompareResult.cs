
namespace RESTRunner.Domain.Models;

/// <summary>
/// CompareResult
/// </summary>
public record CompareResult
{
    /// <summary>
    /// How long did it take for the request to come back?
    /// </summary>
    public long Duration { get; set; }
    /// <summary>
    /// A deterministic HASH for the content returned from the request
    /// </summary>
    public int? Hash { get; set; }
    /// <summary>
    /// Name of Instance used for request
    /// </summary>
    public string? Instance { get; set; }
    /// <summary>
    /// When was the run completed?
    /// </summary>
    public DateTime LastRunDate { get; set; }
    /// <summary>
    /// The path for the request
    /// </summary>
    public string? Request { get; set; }
    /// <summary>
    /// Code from the request response (i.e. 200 for OK)
    /// </summary>
    public string? ResultCode { get; set; }
    /// <summary>
    /// Session value for tracking
    /// </summary>
    public string? SessionId { get; set; }
    /// <summary>
    /// The description of the status from the request response
    /// </summary>
    public string? StatusDescription { get; set; }
    /// <summary>
    /// Was the request successful
    /// </summary>
    public bool Success { get; set; }
    /// <summary>
    /// User Name
    /// </summary>
    public string? UserName { get; set; }
    /// <summary>
    /// HttpVerb (GET/POST/DELETE/PUT)
    /// </summary>
    public string? Verb { get; set; }

}
