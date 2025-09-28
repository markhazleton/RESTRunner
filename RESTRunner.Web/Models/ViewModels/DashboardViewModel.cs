namespace RESTRunner.Web.Models.ViewModels;

/// <summary>
/// View model for the dashboard/index page
/// </summary>
public class DashboardViewModel
{
    /// <summary>
    /// Recent test executions
    /// </summary>
    public List<ExecutionSummary> RecentExecutions { get; set; } = new();

    /// <summary>
    /// Available configurations
    /// </summary>
    public List<ConfigurationSummary> Configurations { get; set; } = new();

    /// <summary>
    /// Available collections
    /// </summary>
    public List<CollectionSummary> Collections { get; set; } = new();

    /// <summary>
    /// Currently running executions
    /// </summary>
    public List<TestExecution> RunningExecutions { get; set; } = new();

    /// <summary>
    /// System statistics
    /// </summary>
    public SystemStatistics Statistics { get; set; } = new();
}

/// <summary>
/// Summary of an execution for dashboard display
/// </summary>
public class ExecutionSummary
{
    public string Id { get; set; } = string.Empty;
    public string ConfigurationName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public ExecutionStatus Status { get; set; }
    public TimeSpan? Duration { get; set; }
    public double? SuccessRate { get; set; }
    public double? AverageResponseTime { get; set; }
    public int? TotalRequests { get; set; }
    public string ExecutedBy { get; set; } = string.Empty;
}

/// <summary>
/// Summary of a configuration for dashboard display
/// </summary>
public class ConfigurationSummary
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TotalTestCount { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsActive { get; set; }
    public List<string> Tags { get; set; } = new();
    public int ExecutionCount { get; set; }
    public DateTime? LastExecuted { get; set; }
}

/// <summary>
/// Summary of a collection for dashboard display
/// </summary>
public class CollectionSummary
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public int RequestCount { get; set; }
    public DateTime UploadedAt { get; set; }
    public bool IsActive { get; set; }
    public long FileSize { get; set; }
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// System-wide statistics
/// </summary>
public class SystemStatistics
{
    public int TotalConfigurations { get; set; }
    public int ActiveConfigurations { get; set; }
    public int TotalCollections { get; set; }
    public int TotalExecutions { get; set; }
    public int RunningExecutions { get; set; }
    public DateTime? LastExecution { get; set; }
    public double AverageSuccessRate { get; set; }
    public TimeSpan SystemUptime { get; set; }
}