using RESTRunner.Domain.Constants;

namespace RESTRunner.Domain.Models;

/// <summary>
/// The Runner main class
/// </summary>
[Serializable]
public class CompareRunner
{
    /// <summary>
    /// List of all instances to be hit during the REST Runner execution
    /// </summary>
    public List<CompareInstance> Instances { get; set; } = [];
    
    /// <summary>
    /// The last Run Time
    /// </summary>
    public DateTime LastRunTime { get; set; }
    
    /// <summary>
    /// List of Requests to use during the REST Runner execution
    /// </summary>
    public List<CompareRequest> Requests { get; set; } = [];
    
    /// <summary>
    /// Session ID to add to request headers and results for tracking
    /// </summary>
    public string SessionId { get; set; }
    
    /// <summary>
    /// List of users to use during the REST Runner execution
    /// </summary>
    public List<CompareUser> Users { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the CompareRunner class
    /// </summary>
    public CompareRunner()
    {
        SessionId = $"{DomainConstants.SessionIdPrefix}-{DateTime.UtcNow.ToString(DomainConstants.SessionDateFormat)}";
        LastRunTime = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Validates that the runner has the minimum required configuration
    /// </summary>
    /// <returns>True if the runner is valid, false otherwise</returns>
    public bool IsValid() => Instances.Count > 0 && Requests.Count > 0 && 
                           Instances.All(i => i.IsValid()) && 
                           Requests.All(r => r.IsValid());
    
    /// <summary>
    /// Gets the total number of test combinations (instances × requests × users)
    /// </summary>
    /// <returns>The total number of test combinations</returns>
    public int GetTotalTestCount() => Instances.Count * Requests.Count * Math.Max(Users.Count, 1);
}
