using Microsoft.Extensions.Logging;
using WebSpark.HttpClientUtility.BatchExecution;

namespace RESTRunner.Domain.Services;

/// <summary>
/// Executes a CompareRunner and collects execution statistics
/// </summary>
public class ExecuteRunnerService(
    CompareRunner compareRunner,
    IBatchExecutionService batchExecutionService,
    ILogger<ExecuteRunnerService> logger) : IExecuteRunner
{
    /// <inheritdoc />
    public async Task<ExecutionStatistics> ExecuteRunnerAsync(IOutput output, CancellationToken ct = default)
    {
        var stats = new ExecutionStatistics { StartTime = DateTime.UtcNow };

        try
        {
            var configuration = BuildBatchConfiguration(compareRunner);
            var sink = new RestRunnerBatchSink(output, stats, compareRunner.SessionId);

            logger.LogInformation(
                "Starting REST Runner batch execution with {InstanceCount} instances, {UserCount} users, {RequestCount} requests, {Iterations} iterations",
                configuration.Environments.Count,
                configuration.Users.Count,
                configuration.Requests.Count,
                configuration.Iterations);

            await batchExecutionService.ExecuteAsync(configuration, sink, progress: null, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            logger.LogInformation("REST Runner execution was cancelled");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during REST Runner execution");
        }
        finally
        {
            stats.FinalizeStatistics();
            logger.LogInformation("Execution completed. {Statistics}", stats.ToString());
        }

        return stats;
    }

    private static BatchExecutionConfiguration BuildBatchConfiguration(CompareRunner runner)
    {
        var users = runner.Users.Count > 0
            ? runner.Users
            : [new CompareUser { UserName = "default" }];

        return new BatchExecutionConfiguration
        {
            RunId = runner.SessionId,
            Iterations = Math.Max(runner.Iterations, 1),
            MaxConcurrency = Math.Max(runner.MaxConcurrency, 1),
            Environments = [.. runner.Instances.Select(MapEnvironment)],
            Users = [.. users.Select(MapUser)],
            Requests = [.. runner.Requests.Select(MapRequest)]
        };
    }

    private static BatchEnvironment MapEnvironment(CompareInstance instance)
    {
        return new BatchEnvironment
        {
            Name = instance.Name ?? "Unknown",
            BaseUrl = instance.BaseUrl ?? string.Empty,
            DefaultHeaders = new Dictionary<string, string>()
        };
    }

    private static BatchUserContext MapUser(CompareUser user)
    {
        var properties = user.Properties.Count > 0
            ? new Dictionary<string, string>(user.Properties)
            : new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(user.UserName))
        {
            properties.TryAdd("encoded_user_name", user.UserName);
        }

        return new BatchUserContext
        {
            UserId = user.UserName ?? "default",
            Properties = properties
        };
    }

    private static BatchRequestDefinition MapRequest(CompareRequest request)
    {
        var headers = request.Headers
            .Where(h => !string.IsNullOrWhiteSpace(h.Key))
            .ToDictionary(h => h.Key, h => h.Value ?? string.Empty);

        return new BatchRequestDefinition
        {
            Name = request.Path ?? request.RequestMethod.ToString(),
            Method = request.RequestMethod.ToString(),
            PathTemplate = request.Path ?? string.Empty,
            BodyTemplate = request.Body?.Raw ?? request.BodyTemplate,
            IsBodyCapable = request.RequiresBody(),
            Headers = headers,
            ContentType = "application/json"
        };
    }

    private sealed class RestRunnerBatchSink(IOutput output, ExecutionStatistics stats, string? sessionId) : IBatchExecutionResultSink
    {
        public Task OnResultAsync(BatchExecutionItemResult result, CancellationToken ct)
        {
            stats.IncrementTotalRequests();

            if (result.IsSuccess)
            {
                stats.IncrementSuccessfulRequests();
            }
            else
            {
                stats.IncrementFailedRequests();
            }

            stats.AddResponseTime(result.DurationMilliseconds);

            var methodName = result.HttpMethod ?? "Unknown";
            var environmentName = result.EnvironmentName ?? "Unknown";
            var userName = string.IsNullOrWhiteSpace(result.UserId) ? "Unknown" : result.UserId;
            var statusCode = result.StatusCode?.ToString() ?? "Error";

            stats.RequestsByMethod.AddOrUpdate(methodName, 1, (_, count) => count + 1);
            stats.RequestsByInstance.AddOrUpdate(environmentName, 1, (_, count) => count + 1);
            stats.RequestsByUser.AddOrUpdate(userName, 1, (_, count) => count + 1);
            stats.RequestsByStatusCode.AddOrUpdate(statusCode, 1, (_, count) => count + 1);

            var compareResult = new CompareResult
            {
                UserName = userName,
                SessionId = sessionId,
                Instance = environmentName,
                Verb = methodName,
                Request = result.RequestPath,
                Success = result.IsSuccess,
                ResultCode = statusCode,
                StatusDescription = result.ErrorMessages?.FirstOrDefault(),
                Hash = result.ResponseBodyHash.GetDeterministicHashCode(),
                Duration = result.DurationMilliseconds,
                LastRunDate = result.TimestampUtc.UtcDateTime
            };

            if (result.IsSuccess)
            {
                output.WriteInfo(compareResult);
            }
            else
            {
                output.WriteError(compareResult);
            }

            return Task.CompletedTask;
        }
    }
}


