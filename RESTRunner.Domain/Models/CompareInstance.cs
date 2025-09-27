namespace RESTRunner.Domain.Models;

/// <summary>
/// Class to describe the Target of the REST Request (i.e. Server)
/// </summary>
public class CompareInstance
{
    private string? _baseUrl;
    private string? _name;
    
    /// <summary>
    /// The base url for this instances (target for REST Request)
    /// </summary>
    public string? BaseUrl 
    { 
        get => _baseUrl;
        set
        {
            if (!string.IsNullOrWhiteSpace(value) && Uri.TryCreate(value, UriKind.Absolute, out _))
            {
                _baseUrl = value;
            }
            else if (!string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("BaseUrl must be a valid absolute URL", nameof(BaseUrl));
            }
            else
            {
                _baseUrl = value;
            }
        }
    }
    
    /// <summary>
    /// The name of the request target instance
    /// </summary>
    public string? Name 
    { 
        get => _name;
        set => _name = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
    
    /// <summary>
    /// Holds value for User Token used for Authorization of the request
    /// </summary>
    public string? UserToken { get; set; }
    
    /// <summary>
    /// Holds value for Client Token used for Authorization of the request
    /// </summary>
    public string? ClientToken { get; set; }
    
    /// <summary>
    /// Session ID for tracking purposes, added to request header
    /// </summary>
    public string? SessionId { get; set; }
    
    /// <summary>
    /// Validates that the instance has the minimum required properties
    /// </summary>
    /// <returns>True if the instance is valid, false otherwise</returns>
    public bool IsValid() => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(BaseUrl);
    
    /// <summary>
    /// To String override for formatted output
    /// </summary>
    /// <returns>Formatted string representation of the instance</returns>
    public override string ToString() => $"{Name}:{BaseUrl}";
}
