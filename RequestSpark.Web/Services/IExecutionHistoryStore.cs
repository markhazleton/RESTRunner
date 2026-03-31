using RequestSpark.Domain.Models;
using RequestSpark.Web.Models;

namespace RequestSpark.Web.Services;

/// <summary>
/// Stores, queries, and persists execution history records.
/// </summary>
public interface IExecutionHistoryStore
{
    /// <summary>
    /// Save or update an execution history record and persist it to storage when possible.
    /// </summary>
    /// <param name="history">History record to persist.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SaveAsync(ExecutionHistory history, CancellationToken ct = default);

    /// <summary>
    /// Get a history record by execution id.
    /// </summary>
    /// <param name="executionId">Execution identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Execution history or null when not found.</returns>
    Task<ExecutionHistory?> GetAsync(string executionId, CancellationToken ct = default);

    /// <summary>
    /// Delete a history record and any associated files.
    /// </summary>
    /// <param name="executionId">Execution identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True when the history was deleted.</returns>
    Task<bool> DeleteAsync(string executionId, CancellationToken ct = default);

    /// <summary>
    /// Get paged execution history records.
    /// </summary>
    /// <param name="pageSize">Number of records per page.</param>
    /// <param name="pageNumber">Page number.</param>
    /// <param name="configurationId">Optional configuration filter.</param>
    /// <returns>Paged history results.</returns>
    Task<List<ExecutionHistory>> GetPageAsync(int pageSize = 50, int pageNumber = 1, string? configurationId = null);

    /// <summary>
    /// Get recent execution history records.
    /// </summary>
    /// <param name="count">Maximum number of records to return.</param>
    /// <returns>Recent history results.</returns>
    Task<List<ExecutionHistory>> GetRecentAsync(int count = 10);

    /// <summary>
    /// Aggregate statistics across matching history entries.
    /// </summary>
    /// <param name="startDate">Range start.</param>
    /// <param name="endDate">Range end.</param>
    /// <param name="configurationId">Optional configuration filter.</param>
    /// <returns>Aggregated statistics.</returns>
    Task<ExecutionStatistics> GetAggregatedStatisticsAsync(DateTime startDate, DateTime endDate, string? configurationId = null);
}