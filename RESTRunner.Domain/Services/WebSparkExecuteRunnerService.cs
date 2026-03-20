using Microsoft.Extensions.Logging;
using WebSpark.HttpClientUtility;

namespace RESTRunner.Domain.Services;

/// <summary>
/// Implements IExecuteRunner using WebSpark.HttpClientUtility batch execution service
/// </summary>
public class WebSparkExecuteRunnerService(
    CompareRunner compareRunner,
    IBatchExecutionService batchExecutionService,
    ILogger<WebSparkExecuteRunnerService> logger) : IExecuteRunner
{
    /// <summary>
    /// Executes the runner using WebSpark's batch execution service
    /// </summary>
    public async Task<ExecutionStatistics> ExecuteRunnerAsync(IOutput output, CancellationToken ct = default)
    {
        var stats = new ExecutionStatistics();
        stats.StartTime = DateTime.UtcNow;

        try
        {
            output.WriteHeader("RESTRunner Batch Execution Started");
            output.WriteInfo($"Total Tests: {compareRunner.GetTotalTestCount()}");
            output.WriteInfo($"Instances: {compareRunner.Instances.Count}");
            output.WriteInfo($"Users: {compareRunner.Users.Count}");
            output.WriteInfo($"Requests: {compareRunner.Requests.Count}");

            // Convert RESTRunner configuration to WebSpark batch configuration
            var batchConfig = compareRunner.ToBatchExecutionConfiguration(
                iterations: compareRunner.Iterations,
                maxConcurrency: compareRunner.MaxConcurrency);

            // Execute using WebSpark's batch service
            var batchResult = await batchExecutionService.ExecuteAsync(batchConfig, ct);

            // Convert WebSpark results to our ExecutionStatistics model
            ConvertResultsToStatistics(batchResult, stats);

            stats.EndTime = DateTime.UtcNow;

            // Write results using our output handler
            WriteResultsToOutput(output, stats);

            return stats;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            logger.LogInformation("Batch execution was cancelled");
            stats.EndTime = DateTime.UtcNow;
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during batch execution");
            stats.EndTime = DateTime.UtcNow;
            output.WriteError($"Execution failed: {ex.Message}");
            throw;
        }
    }

    private void ConvertResultsToStatistics(BatchExecutionResult batchResult, ExecutionStatistics stats)
    {
        if (batchResult.Results == null || !batchResult.Results.Any())
        {
            return;
        }

        foreach (var result in batchResult.Results)
        {
            // Increment request counters
            stats.IncrementTotalRequests();

            // Add response time
            stats.AddResponseTime(result.ResponseTimeMs);

            // Track by method
            if (!string.IsNullOrEmpty(result.Method))
            {
                stats.RequestsByMethod.AddOrUpdate(result.Method, 1, (_, count) => count + 1);
            }

            // Track by environment
            if (!string.IsNullOrEmpty(result.EnvironmentName))
            {
                stats.RequestsByInstance.AddOrUpdate(result.EnvironmentName, 1, (_, count) => count + 1);
            }

            // Track by user
            if (!string.IsNullOrEmpty(result.UserId))
            {
                stats.RequestsByUser.AddOrUpdate(result.UserId, 1, (_, count) => count + 1);
            }

            // Track by status code
            if (result.StatusCode.HasValue)
            {
                var statusCode = result.StatusCode.Value.ToString();
                stats.RequestsByStatusCode.AddOrUpdate(statusCode, 1, (_, count) => count + 1);

                // Track success/failure
                if (result.StatusCode >= 200 && result.StatusCode < 300)
                {
                    stats.IncrementSuccessfulRequests();
                }
                else
                {
                    stats.IncrementFailedRequests();
                }
            }
            else
            {
                // No status code means the request failed
                stats.IncrementFailedRequests();
                stats.RequestsByStatusCode.AddOrUpdate("Error", 1, (_, count) => count + 1);
            }
        }

        stats.FinalizeStatistics();
    }

    private void WriteResultsToOutput(IOutput output, ExecutionStatistics stats)
    {
        output.WriteInfo("");
        output.WriteInfo("=== Execution Summary ===");
        output.WriteInfo($"Total Requests: {stats.TotalRequests}");
        output.WriteInfo($"Successful: {stats.SuccessfulRequests} ({stats.SuccessRate:F2}%)");
        output.WriteInfo($"Failed: {stats.FailedRequests}");
        output.WriteInfo("");
        output.WriteInfo($"Average Response Time: {stats.AverageResponseTime:F2}ms");
        output.WriteInfo($"Min Response Time: {stats.MinResponseTime}ms");
        output.WriteInfo($"Max Response Time: {stats.MaxResponseTime}ms");
        output.WriteInfo($"Duration: {stats.TotalDuration.TotalSeconds:F2}s");
        output.WriteInfo($"Requests/Second: {stats.RequestsPerSecond:F2}");
    }
}
