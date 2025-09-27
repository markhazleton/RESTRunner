namespace RESTRunner.Domain.Models;

/// <summary>
/// Compare Property Class for representing key-value pairs with metadata
/// </summary>
public class CompareProperty
{
    /// <summary>
    /// The key identifier for the property
    /// </summary>
    public string Key { get; set; }
    
    /// <summary>
    /// The value of the property
    /// </summary>
    public string Value { get; set; }
    
    /// <summary>
    /// The type of the property
    /// </summary>
    public string Type { get; set; }
    
    /// <summary>
    /// The display name of the property
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Optional description of the property
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Initializes a new instance of the CompareProperty class
    /// </summary>
    /// <param name="key">The key identifier</param>
    /// <param name="value">The property value</param>
    /// <param name="type">The property type</param>
    /// <param name="name">The display name</param>
    /// <param name="description">Optional description</param>
    public CompareProperty(string key, string value, string type, string name, string? description = null)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? string.Empty;
    }
}
