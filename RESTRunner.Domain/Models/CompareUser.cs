namespace RESTRunner.Domain.Models;

/// <summary>
///  The User to use for the Request
/// </summary>
public class CompareUser
{
    /// <summary>
    /// User password for authentication
    /// </summary>
    public string? Password { get; set; }
    
    /// <summary>
    /// Additional user properties
    /// </summary>
    public Dictionary<string, string> Properties { get; set; } = new();
    
    /// <summary>
    /// User name for authentication
    /// </summary>
    public string? UserName { get; set; }
}
