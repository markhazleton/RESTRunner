using System.ComponentModel.DataAnnotations;
using RESTRunner.Domain.Models;

namespace RESTRunner.Web.Models;

/// <summary>
/// Represents the history of a test execution
/// </summary>
public class ExecutionHistory
{
    /// <summary>
    /// Unique identifier for this execution
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Reference to the configuration that was executed
    /// </summary>
    [Required]
    public string ConfigurationId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the configuration at the time of execution
    /// </summary>
    [Required]
    public string ConfigurationName { get; set; } = string.Empty;

    /// <summary>
    /// When the execution started
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// When the execution ended
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Current status of the execution
    /// </summary>
    public ExecutionStatus Status { get; set; } = ExecutionStatus.Pending;

    /// <summary>
    /// The execution statistics (null while running)
    /// </summary>
    public ExecutionStatistics? Statistics { get; set; }

    /// <summary>
    /// Error message if execution failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// User who initiated this execution
    /// </summary>
    public string ExecutedBy { get; set; } = "System";

    /// <summary>
    /// File path to the CSV results (if completed successfully)
    /// </summary>
    public string? ResultsFilePath { get; set; }

    /// <summary>
    /// File path to the execution log
    /// </summary>
    public string? LogFilePath { get; set; }

    /// <summary>
    /// Tags inherited from the configuration
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Total duration of execution
    /// </summary>
    public TimeSpan? Duration => EndTime.HasValue && StartTime != default 
        ? EndTime.Value - StartTime 
        : null;

    /// <summary>
    /// Whether the execution completed successfully
    /// </summary>
    public bool IsSuccessful => Status == ExecutionStatus.Completed && Statistics != null;

    /// <summary>
    /// Whether the execution is currently running
    /// </summary>
    public bool IsRunning => Status == ExecutionStatus.Running;

    /// <summary>
    /// Whether the execution has finished (successfully or not)
    /// </summary>
    public bool IsFinished => Status is ExecutionStatus.Completed or ExecutionStatus.Failed or ExecutionStatus.Cancelled;
}

/// <summary>
/// Status of a test execution
/// </summary>
public enum ExecutionStatus
{
    /// <summary>
    /// Execution is queued but not started
    /// </summary>
    Pending,
    
    /// <summary>
    /// Execution is currently running
    /// </summary>
    Running,
    
    /// <summary>
    /// Execution completed successfully
    /// </summary>
    Completed,
    
    /// <summary>
    /// Execution failed with error
    /// </summary>
    Failed,
    
    /// <summary>
    /// Execution was cancelled by user
    /// </summary>
    Cancelled
}