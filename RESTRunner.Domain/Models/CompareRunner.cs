
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
    public List<CompareInstance> Instances;
    /// <summary>
    /// The last Run Time
    /// </summary>
    public DateTime LastRunTime;
    /// <summary>
    /// List of Requests to use during the REST Runner execution
    /// </summary>
    public List<CompareRequest> Requests;
    /// <summary>
    /// Session ID to add to request headers and results for tracking
    /// </summary>
    public string SessionId;
    /// <summary>
    /// List of users to use during the REST Runner execution
    /// </summary>
    public List<CompareUser> Users;
    /// <summary>
    /// 
    /// </summary>
    /// <summary>
    /// 
    /// </summary>
    public CompareRunner()
    {
        Instances = new List<CompareInstance>();
        Requests = new List<CompareRequest>();
        Users = new List<CompareUser>();
        SessionId = $"RESTRunner-{DateTime.Now.ToShortDateString()}";
        LastRunTime = DateTime.Now;
    }
}
