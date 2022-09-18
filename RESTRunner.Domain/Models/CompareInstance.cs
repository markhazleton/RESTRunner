
namespace RESTRunner.Domain.Models;

/// <summary>
/// Class to describe the Target of the REST Request (i.e. Server)
/// </summary>
public class CompareInstance
{
    /// <summary>
    /// The base url for this instances (target for REST Request)
    /// </summary>
    public string? BaseUrl;
    /// <summary>
    /// The name of the request target instance
    /// </summary>
    public string? Name;
    /// <summary>
    /// Holds value for User Token used for Authorization of the request
    /// </summary>
    public string? UserToken;
    /// <summary>
    /// Holds value for Client Token used for Authorization of the request
    /// </summary>
    public string? ClientToken;
    /// <summary>
    /// Session ID for tracking purposes, added to request header
    /// </summary>
    public string? SessionId;
    /// <summary>
    /// To String override for formatted output
    /// </summary>
    /// <returns></returns>
    public override string ToString() { return $"{Name}:{BaseUrl}"; }
}
