namespace RESTRunner.Domain.Models;

/// <summary>
///  The User to use for the Request
/// </summary>
public class CompareUser
{
    /// <summary>
    /// 
    /// </summary>
    public string? Password;
    /// <summary>
    /// 
    /// </summary>
    public Dictionary<string, string> Properties;
    /// <summary>
    /// 
    /// </summary>
    public string? UserName;

    /// <summary>
    /// Constructor
    /// </summary>
    public CompareUser()
    {
        Properties = new Dictionary<string, string>();
    }
}
