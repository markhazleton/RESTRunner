using RESTRunner.Web.Models;
using RESTRunner.Domain.Models;

namespace RESTRunner.Web.Services;

/// <summary>
/// Service for managing test executions
/// </summary>
public interface IExecutionService
{
    /// <summary>
    /// Start a new test execution
    /// </summary>
    /// <param name="configurationId">Configuration to execute</param>
    /// <param name="executedBy">User who initiated the execution</param>
    /// <returns>Test execution tracking object</returns>
    Task<TestExecution> StartExecutionAsync(string configurationId, string executedBy = "System");

    /// <summary>
    /// Get a running execution by ID
    /// </summary>
    /// <param name="executionId">Execution ID</param>
    /// <returns>Test execution or null if not found</returns>
    Task<TestExecution?> GetExecutionAsync(string executionId);

    /// <summary>
    /// Get all currently running executions
    /// </summary>
    /// <returns>List of running executions</returns>
    Task<List<TestExecution>> GetRunningExecutionsAsync();

    /// <summary>
    /// Cancel a running execution
    /// </summary>
    /// <param name="executionId">Execution ID to cancel</param>
    /// <param name="cancelledBy">User who cancelled the execution</param>
    /// <returns>True if cancelled, false if not found or already finished</returns>
    Task<bool> CancelExecutionAsync(string executionId, string cancelledBy = "System");

    /// <summary>
    /// Get execution history
    /// </summary>
    /// <param name="pageSize">Number of records per page</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="configurationId">Filter by configuration ID (optional)</param>
    /// <returns>List of execution history records</returns>
    Task<List<ExecutionHistory>> GetExecutionHistoryAsync(int pageSize = 50, int pageNumber = 1, string? configurationId = null);

    /// <summary>
    /// Get execution history by ID
    /// </summary>
    /// <param name="executionId">Execution ID</param>
    /// <returns>Execution history or null if not found</returns>
    Task<ExecutionHistory?> GetExecutionHistoryAsync(string executionId);

    /// <summary>
    /// Delete execution history record
    /// </summary>
    /// <param name="executionId">Execution ID</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteExecutionHistoryAsync(string executionId);

    /// <summary>
    /// Get recent executions
    /// </summary>
    /// <param name="count">Number of recent executions to return</param>
    /// <returns>List of recent execution histories</returns>
    Task<List<ExecutionHistory>> GetRecentExecutionsAsync(int count = 10);

    /// <summary>
    /// Get execution statistics for a date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="configurationId">Filter by configuration ID (optional)</param>
    /// <returns>Aggregated statistics</returns>
    Task<ExecutionStatistics> GetAggregatedStatisticsAsync(DateTime startDate, DateTime endDate, string? configurationId = null);

    /// <summary>
    /// Export execution results
    /// </summary>
    /// <param name="executionId">Execution ID</param>
    /// <param name="format">Export format (csv, json, excel)</param>
    /// <returns>Export file path or null if not found</returns>
    Task<string?> ExportExecutionResultsAsync(string executionId, string format = "csv");
}