namespace RESTRunner.Web.Models;

/// <summary>
/// Represents a real-time test execution with progress tracking
/// </summary>
public class TestExecution
{
    /// <summary>
    /// Unique identifier for this execution
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Reference to the configuration being executed
    /// </summary>
    public string ConfigurationId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the configuration
    /// </summary>
    public string ConfigurationName { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the execution
    /// </summary>
    public ExecutionStatus Status { get; set; } = ExecutionStatus.Pending;

    /// <summary>
    /// When the execution started
    /// </summary>
    public DateTime StartTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Progress percentage (0-100)
    /// </summary>
    public double ProgressPercentage { get; set; }

    /// <summary>
    /// Current phase of execution
    /// </summary>
    public string CurrentPhase { get; set; } = "Initializing";

    /// <summary>
    /// Number of requests completed so far
    /// </summary>
    public int CompletedRequests { get; set; }

    /// <summary>
    /// Total number of requests to be made
    /// </summary>
    public int TotalRequests { get; set; }

    /// <summary>
    /// Number of successful requests so far
    /// </summary>
    public int SuccessfulRequests { get; set; }

    /// <summary>
    /// Number of failed requests so far
    /// </summary>
    public int FailedRequests { get; set; }

    /// <summary>
    /// Current average response time
    /// </summary>
    public double CurrentAverageResponseTime { get; set; }

    /// <summary>
    /// Current requests per second
    /// </summary>
    public double CurrentRequestsPerSecond { get; set; }

    /// <summary>
    /// User who initiated this execution
    /// </summary>
    public string ExecutedBy { get; set; } = "System";

    /// <summary>
    /// Cancellation token source for stopping execution
    /// </summary>
    public CancellationTokenSource? CancellationTokenSource { get; set; }

    /// <summary>
    /// Error message if execution failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime LastUpdate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Update progress information
    /// </summary>
    /// <param name="completed">Number of completed requests</param>
    /// <param name="successful">Number of successful requests</param>
    /// <param name="failed">Number of failed requests</param>
    /// <param name="averageResponseTime">Current average response time</param>
    /// <param name="currentPhase">Current execution phase</param>
    public void UpdateProgress(int completed, int successful, int failed, double averageResponseTime, string currentPhase = "")
    {
        CompletedRequests = completed;
        SuccessfulRequests = successful;
        FailedRequests = failed;
        CurrentAverageResponseTime = averageResponseTime;
        
        if (!string.IsNullOrEmpty(currentPhase))
            CurrentPhase = currentPhase;

        if (TotalRequests > 0)
            ProgressPercentage = Math.Min(100.0, (double)completed / TotalRequests * 100.0);

        // Calculate requests per second
        var elapsed = DateTime.UtcNow - StartTime;
        if (elapsed.TotalSeconds > 0)
            CurrentRequestsPerSecond = completed / elapsed.TotalSeconds;

        LastUpdate = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark execution as completed
    /// </summary>
    public void MarkCompleted()
    {
        Status = ExecutionStatus.Completed;
        ProgressPercentage = 100.0;
        CurrentPhase = "Completed";
        LastUpdate = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark execution as failed
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    public void MarkFailed(string errorMessage)
    {
        Status = ExecutionStatus.Failed;
        ErrorMessage = errorMessage;
        CurrentPhase = "Failed";
        LastUpdate = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark execution as cancelled
    /// </summary>
    public void MarkCancelled()
    {
        Status = ExecutionStatus.Cancelled;
        CurrentPhase = "Cancelled";
        LastUpdate = DateTime.UtcNow;
        CancellationTokenSource?.Cancel();
    }

    /// <summary>
    /// Whether the execution can be cancelled
    /// </summary>
    public bool CanBeCancelled => Status == ExecutionStatus.Running || Status == ExecutionStatus.Pending;
}