using RequestSpark.Domain.Models;
using RequestSpark.Web.Models;
using System.Collections.Concurrent;
using System.Text.Json;

namespace RequestSpark.Web.Services;

/// <summary>
/// File-backed execution history store used by the real execution pipeline.
/// </summary>
public class ExecutionHistoryStore(
    IFileStorageService fileStorageService,
    ILogger<ExecutionHistoryStore> logger) : IExecutionHistoryStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly ConcurrentDictionary<string, ExecutionHistory> _executionHistory = new();

    public async Task SaveAsync(ExecutionHistory history, CancellationToken ct = default)
    {
        _executionHistory.AddOrUpdate(history.Id, history, (_, _) => history);

        try
        {
            var historyFileName = $"history_{history.Id}.json";
            var historyJson = JsonSerializer.Serialize(history, SerializerOptions);
            history.LogFilePath = await fileStorageService.SaveLogAsync(historyFileName, historyJson);
            _executionHistory[history.Id] = history;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to persist execution history {ExecutionId}", history.Id);
        }
    }

    public async Task<ExecutionHistory?> GetAsync(string executionId, CancellationToken ct = default)
    {
        if (_executionHistory.TryGetValue(executionId, out var history))
        {
            return history;
        }

        var historyFilePath = Path.Combine(
            fileStorageService.GetDirectoryPath("logs"),
            $"history_{executionId}.json");

        var content = await fileStorageService.ReadFileAsync(historyFilePath);
        if (content == null)
        {
            return null;
        }

        try
        {
            history = JsonSerializer.Deserialize<ExecutionHistory>(content, SerializerOptions);
            if (history != null)
            {
                history.LogFilePath ??= historyFilePath;
                _executionHistory.TryAdd(executionId, history);
            }

            return history;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to deserialize history file for execution {ExecutionId}", executionId);
            return null;
        }
    }

    public async Task<bool> DeleteAsync(string executionId, CancellationToken ct = default)
    {
        if (!_executionHistory.TryRemove(executionId, out var execution))
        {
            execution = await GetAsync(executionId, ct);
            if (execution == null)
            {
                return false;
            }

            _executionHistory.TryRemove(executionId, out _);
        }

        logger.LogInformation("Deleted execution history {ExecutionId}", executionId);

        try
        {
            if (!string.IsNullOrEmpty(execution.ResultsFilePath) && File.Exists(execution.ResultsFilePath))
            {
                File.Delete(execution.ResultsFilePath);
            }

            if (!string.IsNullOrEmpty(execution.LogFilePath) && File.Exists(execution.LogFilePath))
            {
                File.Delete(execution.LogFilePath);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete execution files for {ExecutionId}", executionId);
        }

        return true;
    }

    public Task<List<ExecutionHistory>> GetPageAsync(int pageSize = 50, int pageNumber = 1, string? configurationId = null)
    {
        var query = _executionHistory.Values.AsEnumerable();

        if (!string.IsNullOrEmpty(configurationId))
        {
            query = query.Where(e => e.ConfigurationId == configurationId);
        }

        var result = query
            .OrderByDescending(e => e.StartTime)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<List<ExecutionHistory>> GetRecentAsync(int count = 10)
    {
        var recentExecutions = _executionHistory.Values
            .OrderByDescending(e => e.StartTime)
            .Take(count)
            .ToList();
        return Task.FromResult(recentExecutions);
    }

    public Task<ExecutionStatistics> GetAggregatedStatisticsAsync(DateTime startDate, DateTime endDate, string? configurationId = null)
    {
        var executions = _executionHistory.Values.Where(e =>
            e.StartTime >= startDate &&
            e.StartTime <= endDate &&
            (configurationId == null || e.ConfigurationId == configurationId) &&
            e.Statistics != null);

        if (!executions.Any())
        {
            return Task.FromResult(new ExecutionStatistics());
        }

        var stats = new ExecutionStatistics
        {
            StartTime = startDate,
            EndTime = endDate
        };

        foreach (var execution in executions)
        {
            if (execution.Statistics == null)
            {
                continue;
            }

            for (int i = 0; i < execution.Statistics.TotalRequests; i++)
            {
                stats.IncrementTotalRequests();
            }

            for (int i = 0; i < execution.Statistics.SuccessfulRequests; i++)
            {
                stats.IncrementSuccessfulRequests();
            }

            for (int i = 0; i < execution.Statistics.FailedRequests; i++)
            {
                stats.IncrementFailedRequests();
            }

            stats.AddResponseTime(execution.Statistics.MinResponseTime);
            stats.AddResponseTime(execution.Statistics.MaxResponseTime);
            stats.AddResponseTime((long)execution.Statistics.AverageResponseTime);
        }

        stats.FinalizeStatistics();
        return Task.FromResult(stats);
    }
}