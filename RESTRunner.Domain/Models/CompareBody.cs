namespace RESTRunner.Domain.Models;

/// <summary>
/// Class for Body of REST request
/// </summary>
public class CompareBody
{
    /// <summary>
    /// Body mode - Raw or Dynamic based on Property list
    /// </summary>
    public string? Mode = null;
    /// <summary>
    /// List of Properties for the body to dynamically create the request body
    /// </summary>
    public List<CompareProperty> Properties = new();
    /// <summary>
    /// Raw request body (usually in JSON)
    /// </summary>
    public string? Raw = null;
    /// <summary>
    /// Language
    /// </summary>
    public string? Language = null;
}
