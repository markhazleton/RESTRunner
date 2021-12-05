namespace RESTRunner.Domain.Models;

/// <summary>
/// Compare Property Class
/// </summary>
public class CompareProperty
{
    /// <summary>
    /// Key
    /// </summary>
    public string Key;
    /// <summary>
    /// Value
    /// </summary>
    public string Value;
    /// <summary>
    /// Type
    /// </summary>
    public string Type;
    /// <summary>
    /// Name
    /// </summary>
    public string Name;
    /// <summary>
    /// Description
    /// </summary>
    public string Description;

    /// <summary>
    /// Compare Property Constructor
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <param name="description"></param>
    public CompareProperty(string key, string value, string type, string name, string? description = null)
    {
        Key = key;
        Value = value;
        Type = type;
        Name = name;
        Description = description ?? string.Empty;
    }
}
