namespace RESTRunner.Domain.Models;

/// <summary>
/// Class for Body of REST request
/// </summary>
public class CompareBody
{
    /// <summary>
    /// Body mode - Raw or Dynamic based on Property list
    /// </summary>
    public string? Mode { get; set; }
    
    /// <summary>
    /// List of Properties for the body to dynamically create the request body
    /// </summary>
    public List<CompareProperty> Properties { get; set; } = [];
    
    /// <summary>
    /// Raw request body (usually in JSON)
    /// </summary>
    public string? Raw { get; set; }
    
    /// <summary>
    /// Programming language for syntax highlighting or validation
    /// </summary>
    public string? Language { get; set; }
}
