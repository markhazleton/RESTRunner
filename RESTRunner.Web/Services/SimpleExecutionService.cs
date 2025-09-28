using RESTRunner.Web.Models;
using RESTRunner.Domain.Models;

namespace RESTRunner.Web.Services;

/// <summary>
/// Simple implementation of execution service for Phase 2
/// This will be enhanced in Phase 3 with real execution capabilities
/// </summary>
public class SimpleExecutionService : IExecutionService
{
    private readonly ILogger<SimpleExecutionService> _logger;
    private readonly List<ExecutionHistory> _executionHistory = new();
    private readonly List<TestExecution> _runningExecutions = new();

    public SimpleExecutionService(ILogger<SimpleExecutionService> logger)
    {
        _logger = logger;
        
        // Add some sample data for demonstration
        SeedSampleData();
    }

    public async Task<TestExecution> StartExecutionAsync(string configurationId, string executedBy = "System")
    {
        var execution = new TestExecution
        {
            ConfigurationId = configurationId,
            ConfigurationName = $"Config-{configurationId[..8]}", // Show first 8 chars of ID
            ExecutedBy = executedBy,
            Status = ExecutionStatus.Pending,
            StartTime = DateTime.UtcNow,
            TotalRequests = 100, // Mock data
            CurrentPhase = "Preparing execution..."
        };

        _runningExecutions.Add(execution);
        _logger.LogInformation("Started execution {ExecutionId} for configuration {ConfigurationId}", execution.Id, configurationId);
        
        return execution;
    }

    public async Task<TestExecution?> GetExecutionAsync(string executionId)
    {
        return _runningExecutions.FirstOrDefault(e => e.Id == executionId);
    }

    public async Task<List<TestExecution>> GetRunningExecutionsAsync()
    {
        return _runningExecutions.Where(e => e.Status == ExecutionStatus.Running || e.Status == ExecutionStatus.Pending).ToList();
    }

    public async Task<bool> CancelExecutionAsync(string executionId, string cancelledBy = "System")
    {
        var execution = _runningExecutions.FirstOrDefault(e => e.Id == executionId);
        if (execution != null && execution.CanBeCancelled)
        {
            execution.MarkCancelled();
            _logger.LogInformation("Cancelled execution {ExecutionId} by {CancelledBy}", executionId, cancelledBy);
            return true;
        }
        return false;
    }

    public async Task<List<ExecutionHistory>> GetExecutionHistoryAsync(int pageSize = 50, int pageNumber = 1, string? configurationId = null)
    {
        var query = _executionHistory.AsQueryable();
        
        if (!string.IsNullOrEmpty(configurationId))
        {
            query = query.Where(e => e.ConfigurationId == configurationId);
        }

        return query
            .OrderByDescending(e => e.StartTime)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public async Task<ExecutionHistory?> GetExecutionHistoryAsync(string executionId)
    {
        return _executionHistory.FirstOrDefault(e => e.Id == executionId);
    }

    public async Task<bool> DeleteExecutionHistoryAsync(string executionId)
    {
        var execution = _executionHistory.FirstOrDefault(e => e.Id == executionId);
        if (execution != null)
        {
            _executionHistory.Remove(execution);
            _logger.LogInformation("Deleted execution history {ExecutionId}", executionId);
            return true;
        }
        return false;
    }

    public async Task<List<ExecutionHistory>> GetRecentExecutionsAsync(int count = 10)
    {
        return _executionHistory
            .OrderByDescending(e => e.StartTime)
            .Take(count)
            .ToList();
    }

    public async Task<ExecutionStatistics> GetAggregatedStatisticsAsync(DateTime startDate, DateTime endDate, string? configurationId = null)
    {
        var executions = _executionHistory.Where(e => 
            e.StartTime >= startDate && 
            e.StartTime <= endDate &&
            (configurationId == null || e.ConfigurationId == configurationId) &&
            e.Statistics != null);

        if (!executions.Any())
        {
            return new ExecutionStatistics();
        }

        // Aggregate statistics from multiple executions
        var stats = new ExecutionStatistics
        {
            StartTime = startDate,
            EndTime = endDate
        };

        foreach (var execution in executions)
        {
            if (execution.Statistics != null)
            {
                // Use the increment methods to add values
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
                
                // Add some sample response times (this is simplified)
                stats.AddResponseTime(execution.Statistics.MinResponseTime);
                stats.AddResponseTime(execution.Statistics.MaxResponseTime);
                stats.AddResponseTime((long)execution.Statistics.AverageResponseTime);
            }
        }

        stats.FinalizeStatistics();
        return stats;
    }

    public async Task<string?> ExportExecutionResultsAsync(string executionId, string format = "csv")
    {
        var execution = await GetExecutionHistoryAsync(executionId);
        if (execution == null) return null;

        // Mock export - in Phase 3 this would generate actual files
        var fileName = $"execution_{executionId}_{DateTime.UtcNow:yyyyMMddHHmmss}.{format}";
        _logger.LogInformation("Exported execution results {ExecutionId} to {FileName}", executionId, fileName);
        
        return fileName;
    }

    private void SeedSampleData()
    {
        // Add some sample execution history for demonstration
        var sampleExecutions = new[]
        {
            new ExecutionHistory
            {
                Id = Guid.NewGuid().ToString(),
                ConfigurationId = "config-1",
                ConfigurationName = "API Health Check",
                StartTime = DateTime.UtcNow.AddHours(-2),
                EndTime = DateTime.UtcNow.AddHours(-2).AddMinutes(5),
                Status = ExecutionStatus.Completed,
                ExecutedBy = "admin",
                Statistics = CreateSampleStatistics(100, 98, 2, 125.5, 45, 890)
            },
            new ExecutionHistory
            {
                Id = Guid.NewGuid().ToString(),
                ConfigurationId = "config-2", 
                ConfigurationName = "Load Test - Production",
                StartTime = DateTime.UtcNow.AddHours(-4),
                EndTime = DateTime.UtcNow.AddHours(-4).AddMinutes(15),
                Status = ExecutionStatus.Completed,
                ExecutedBy = "system",
                Statistics = CreateSampleStatistics(500, 485, 15, 234.8, 89, 2100)
            },
            new ExecutionHistory
            {
                Id = Guid.NewGuid().ToString(),
                ConfigurationId = "config-3",
                ConfigurationName = "Regression Test Suite",
                StartTime = DateTime.UtcNow.AddDays(-1),
                EndTime = DateTime.UtcNow.AddDays(-1).AddMinutes(8),
                Status = ExecutionStatus.Failed,
                ExecutedBy = "testuser",
                ErrorMessage = "Connection timeout to staging environment",
                Statistics = CreateSampleStatistics(200, 145, 55, 456.2, 123, 5000)
            }
        };

        _executionHistory.AddRange(sampleExecutions);
        
        _logger.LogInformation("Seeded {Count} sample execution records", sampleExecutions.Length);
    }

    private ExecutionStatistics CreateSampleStatistics(int total, int successful, int failed, double avgResponseTime, long minResponseTime, long maxResponseTime)
    {
        var stats = new ExecutionStatistics();
        
        // Increment counters using the proper methods
        for (int i = 0; i < total; i++)
        {
            stats.IncrementTotalRequests();
        }
        for (int i = 0; i < successful; i++)
        {
            stats.IncrementSuccessfulRequests();
        }
        for (int i = 0; i < failed; i++)
        {
            stats.IncrementFailedRequests();
        }
        
        // Add sample response times to calculate proper statistics
        stats.AddResponseTime(minResponseTime);
        stats.AddResponseTime(maxResponseTime);
        
        // Add some response times around the average
        var random = new Random();
        for (int i = 0; i < 10; i++)
        {
            var responseTime = (long)(avgResponseTime + random.Next(-50, 50));
            stats.AddResponseTime(Math.Max(1, responseTime)); // Ensure positive response time
        }
        
        stats.FinalizeStatistics();
        return stats;
    }
}