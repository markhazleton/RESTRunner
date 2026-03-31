using Microsoft.AspNetCore.SignalR;
using RequestSpark.Domain.Extensions;
using RequestSpark.Domain.Interfaces;
using RequestSpark.Domain.Models;
using RequestSpark.Domain.Outputs;
using RequestSpark.Web.Hubs;
using RequestSpark.Web.Models;
using System.Collections.Concurrent;

namespace RequestSpark.Web.Services;

/// <summary>
/// Real implementation of execution service with actual test execution capabilities
/// </summary>
public class RealExecutionService : IExecutionService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IExecutionHistoryStore _executionHistoryStore;
    private readonly IHubContext<ExecutionHub> _hubContext;
    private readonly ILogger<RealExecutionService> _logger;
    private readonly ConcurrentDictionary<string, TestExecution> _runningExecutions = new();

    public RealExecutionService(
        IServiceScopeFactory scopeFactory,
        IHttpClientFactory httpClientFactory,
        IExecutionHistoryStore executionHistoryStore,
        IHubContext<ExecutionHub> hubContext,
        ILogger<RealExecutionService> logger)
    {
        _scopeFactory = scopeFactory;
        _httpClientFactory = httpClientFactory;
        _executionHistoryStore = executionHistoryStore;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task<TestExecution> StartExecutionAsync(string configurationId, string executedBy = "System")
    {
        try
        {
            TestConfiguration? config;
            using (var scope = _scopeFactory.CreateScope())
            {
                var configService = scope.ServiceProvider.GetRequiredService<IConfigurationService>();
                config = await configService.GetByIdAsync(configurationId);
            }

            if (config == null)
            {
                throw new InvalidOperationException($"Configuration {configurationId} not found");
            }

            if (!config.IsValid())
            {
                throw new InvalidOperationException("Configuration is not valid");
            }

            var execution = new TestExecution
            {
                ConfigurationId = configurationId,
                ConfigurationName = config.Name,
                SourceType = config.SourceType,
                SourceId = config.SourceId,
                SourceName = config.SourceName,
                ExecutedBy = executedBy,
                Status = ExecutionStatus.Pending,
                StartTime = DateTime.UtcNow,
                TotalRequests = config.GetTotalTestCount(),
                CurrentPhase = "Initializing...",
                CancellationTokenSource = new CancellationTokenSource()
            };

            _runningExecutions.TryAdd(execution.Id, execution);

            _logger.LogInformation("Started execution {ExecutionId} for configuration {ConfigurationName}",
                execution.Id, config.Name);

            await _hubContext.Clients.All.SendAsync("ExecutionStarted", execution);

            _ = Task.Run(async () => await ExecuteAsync(execution, config));

            return execution;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start execution for configuration {ConfigurationId}", configurationId);
            throw;
        }
    }

    private async Task ExecuteAsync(TestExecution execution, TestConfiguration config)
    {
        ExecutionHistory? history = null;
        string? resultsFilePath = null;

        try
        {
            execution.Status = ExecutionStatus.Running;
            execution.CurrentPhase = "Preparing execution environment...";
            execution.LastUpdate = DateTime.UtcNow;

            _logger.LogInformation("Starting execution {ExecutionId} for configuration {ConfigurationName}",
                execution.Id, execution.ConfigurationName);

            history = new ExecutionHistory
            {
                Id = execution.Id,
                ConfigurationId = execution.ConfigurationId,
                ConfigurationName = execution.ConfigurationName,
                SourceType = execution.SourceType,
                SourceId = execution.SourceId,
                SourceName = execution.SourceName,
                StartTime = execution.StartTime,
                Status = ExecutionStatus.Running,
                ExecutedBy = execution.ExecutedBy,
                Tags = config.Tags
            };

            var resultsFileName = $"execution_{execution.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            var resultsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Data", "results");

            if (!Directory.Exists(resultsDirectory))
            {
                Directory.CreateDirectory(resultsDirectory);
            }

            resultsFilePath = Path.Combine(resultsDirectory, resultsFileName);
            var output = new CsvOutput(resultsFilePath);

            execution.CurrentPhase = "Executing requests...";
            execution.LastUpdate = DateTime.UtcNow;

            var progressReporter = new Progress<ExecutionProgress>(progress =>
            {
                execution.UpdateProgress(
                    progress.CompletedRequests,
                    progress.SuccessfulRequests,
                    progress.FailedRequests,
                    progress.AverageResponseTime,
                    progress.CurrentPhase);

                _ = _hubContext.Clients.Group($"execution_{execution.Id}")
                    .SendAsync("ProgressUpdate", execution);
            });

            var statistics = await ExecuteWithProgressAsync(
                output,
                config,
                execution.CancellationTokenSource!.Token,
                progressReporter);

            statistics.FinalizeStatistics();

            execution.MarkCompleted();
            history.Statistics = statistics;
            history.EndTime = DateTime.UtcNow;
            history.Status = ExecutionStatus.Completed;
            history.ResultsFilePath = resultsFilePath;

            await _hubContext.Clients.Group($"execution_{execution.Id}")
                .SendAsync("ExecutionCompleted", history);

            _logger.LogInformation("Execution {ExecutionId} completed successfully. Total: {Total}, Success: {Success}, Failed: {Failed}",
                execution.Id, statistics.TotalRequests, statistics.SuccessfulRequests, statistics.FailedRequests);
        }
        catch (OperationCanceledException)
        {
            execution.MarkCancelled();
            if (history != null)
            {
                history.EndTime = DateTime.UtcNow;
                history.Status = ExecutionStatus.Cancelled;
            }

            await _hubContext.Clients.Group($"execution_{execution.Id}")
                .SendAsync("ExecutionCancelled", new { ExecutionId = execution.Id, Timestamp = DateTime.UtcNow });

            _logger.LogInformation("Execution {ExecutionId} was cancelled", execution.Id);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Execution failed: {ex.Message}";
            execution.MarkFailed(errorMessage);

            if (history != null)
            {
                history.EndTime = DateTime.UtcNow;
                history.Status = ExecutionStatus.Failed;
                history.ErrorMessage = errorMessage;
            }

            await _hubContext.Clients.Group($"execution_{execution.Id}")
                .SendAsync("ExecutionFailed", new { ExecutionId = execution.Id, ErrorMessage = errorMessage, Timestamp = DateTime.UtcNow });

            _logger.LogError(ex, "Execution {ExecutionId} failed", execution.Id);
        }
        finally
        {
            if (history != null)
            {
                await _executionHistoryStore.SaveAsync(history);
            }

            _runningExecutions.TryRemove(execution.Id, out _);
            execution.CancellationTokenSource?.Dispose();
        }
    }

    private async Task<ExecutionStatistics> ExecuteWithProgressAsync(
        IOutput output,
        TestConfiguration config,
        CancellationToken cancellationToken,
        IProgress<ExecutionProgress> progress)
    {
        var statistics = new ExecutionStatistics
        {
            StartTime = DateTime.UtcNow
        };

        var totalTests = config.GetTotalTestCount();
        var completedTests = 0;
        var semaphore = new SemaphoreSlim(config.MaxConcurrency);
        var tasks = new List<Task>();

        try
        {
            progress.Report(new ExecutionProgress
            {
                CompletedRequests = 0,
                SuccessfulRequests = 0,
                FailedRequests = 0,
                AverageResponseTime = 0,
                CurrentPhase = "Starting execution..."
            });

            for (int iteration = 0; iteration < config.Iterations; iteration++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                progress.Report(new ExecutionProgress
                {
                    CompletedRequests = completedTests,
                    SuccessfulRequests = statistics.SuccessfulRequests,
                    FailedRequests = statistics.FailedRequests,
                    AverageResponseTime = statistics.CurrentAverageResponseTime,
                    CurrentPhase = $"Iteration {iteration + 1}/{config.Iterations}"
                });

                foreach (var instance in config.Runner.Instances)
                {
                    foreach (var user in config.Runner.Users)
                    {
                        foreach (var request in config.Runner.Requests)
                        {
                            await semaphore.WaitAsync(cancellationToken);

                            tasks.Add(Task.Run(async () =>
                            {
                                try
                                {
                                    var result = await ExecuteRequestAsync(
                                        instance,
                                        request,
                                        user,
                                        statistics,
                                        cancellationToken);

                                    if (result != null)
                                    {
                                        output.WriteInfo(result);
                                    }

                                    Interlocked.Increment(ref completedTests);

                                    if (completedTests % 10 == 0)
                                    {
                                        progress.Report(new ExecutionProgress
                                        {
                                            CompletedRequests = completedTests,
                                            SuccessfulRequests = statistics.SuccessfulRequests,
                                            FailedRequests = statistics.FailedRequests,
                                            AverageResponseTime = statistics.CurrentAverageResponseTime,
                                            CurrentPhase = $"Iteration {iteration + 1}/{config.Iterations}"
                                        });
                                    }
                                }
                                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                                {
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Error executing request");
                                    statistics.IncrementFailedRequests();
                                }
                                finally
                                {
                                    semaphore.Release();
                                }
                            }, cancellationToken));
                        }
                    }
                }
            }

            await Task.WhenAll(tasks);

            progress.Report(new ExecutionProgress
            {
                CompletedRequests = completedTests,
                SuccessfulRequests = statistics.SuccessfulRequests,
                FailedRequests = statistics.FailedRequests,
                AverageResponseTime = statistics.CurrentAverageResponseTime,
                CurrentPhase = "Finalizing results..."
            });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Execution cancelled");
            throw;
        }

        return statistics;
    }

    private async Task<CompareResult?> ExecuteRequestAsync(
        CompareInstance instance,
        CompareRequest request,
        CompareUser user,
        ExecutionStatistics statistics,
        CancellationToken cancellationToken)
    {
        using var client = _httpClientFactory.CreateClient();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var processedPath = request.Path ?? string.Empty;
            var processedBody = request.BodyTemplate ?? string.Empty;

            processedPath = processedPath.Replace("{{encoded_user_name}}", user.UserName);
            foreach (var prop in user.Properties)
            {
                processedPath = processedPath.Replace($"{{{{{prop.Key}}}}}", prop.Value);
                processedBody = processedBody.Replace($"{{{{{prop.Key}}}}}", prop.Value);
            }

            var requestUri = new Uri($"{instance.BaseUrl}{processedPath}");
            HttpResponseMessage? response = null;

            switch (request.RequestMethod)
            {
                case HttpVerb.GET:
                    response = await client.GetAsync(requestUri, cancellationToken);
                    break;
                case HttpVerb.POST:
                    var postContent = new System.Net.Http.StringContent(processedBody, System.Text.Encoding.UTF8, "application/json");
                    response = await client.PostAsync(requestUri, postContent, cancellationToken);
                    break;
                case HttpVerb.PUT:
                    var putContent = new System.Net.Http.StringContent(processedBody, System.Text.Encoding.UTF8, "application/json");
                    response = await client.PutAsync(requestUri, putContent, cancellationToken);
                    break;
                case HttpVerb.DELETE:
                    response = await client.DeleteAsync(requestUri, cancellationToken);
                    break;
                default:
                    _logger.LogWarning("Unsupported HTTP method: {Method}", request.RequestMethod);
                    return null;
            }

            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            statistics.IncrementTotalRequests();
            statistics.AddResponseTime(elapsedMs);
            statistics.RequestsByMethod.AddOrUpdate(request.RequestMethod.ToString(), 1, (_, count) => count + 1);
            statistics.RequestsByInstance.AddOrUpdate(instance.Name ?? "Unknown", 1, (_, count) => count + 1);
            statistics.RequestsByUser.AddOrUpdate(user.UserName ?? "Unknown", 1, (_, count) => count + 1);

            if (response != null)
            {
                var statusCode = ((int)response.StatusCode).ToString();
                statistics.RequestsByStatusCode.AddOrUpdate(statusCode, 1, (_, count) => count + 1);

                if (response.IsSuccessStatusCode)
                {
                    statistics.IncrementSuccessfulRequests();
                }
                else
                {
                    statistics.IncrementFailedRequests();
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                return new CompareResult
                {
                    UserName = user.UserName,
                    SessionId = instance.SessionId,
                    Instance = instance.Name,
                    Verb = request.RequestMethod.ToString(),
                    Request = processedPath,
                    Success = response.IsSuccessStatusCode,
                    ResultCode = statusCode,
                    StatusDescription = response.ReasonPhrase,
                    Hash = content.GetDeterministicHashCode(),
                    Duration = elapsedMs,
                    LastRunDate = DateTime.Now
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            statistics.IncrementTotalRequests();
            statistics.IncrementFailedRequests();
            statistics.AddResponseTime(elapsedMs);

            _logger.LogError(ex, "Request failed: {Method} {BaseUrl}{Path}",
                request.RequestMethod, instance.BaseUrl, request.Path);

            return CompareResult.CreateFailure(
                instance.Name ?? "Unknown",
                request.Path ?? string.Empty,
                request.RequestMethod.ToString(),
                ex.Message,
                elapsedMs,
                instance.SessionId);
        }
    }

    public Task<TestExecution?> GetExecutionAsync(string executionId)
    {
        _runningExecutions.TryGetValue(executionId, out var execution);
        return Task.FromResult(execution);
    }

    public Task<List<TestExecution>> GetRunningExecutionsAsync()
    {
        var runningExecutions = _runningExecutions.Values
            .Where(e => e.Status == ExecutionStatus.Running || e.Status == ExecutionStatus.Pending)
            .ToList();
        return Task.FromResult(runningExecutions);
    }

    public Task<bool> CancelExecutionAsync(string executionId, string cancelledBy = "System")
    {
        if (_runningExecutions.TryGetValue(executionId, out var execution))
        {
            if (execution.CanBeCancelled)
            {
                execution.CancellationTokenSource?.Cancel();
                _logger.LogInformation("Cancelled execution {ExecutionId} by {CancelledBy}", executionId, cancelledBy);
                return Task.FromResult(true);
            }
        }
        return Task.FromResult(false);
    }

    public Task<List<ExecutionHistory>> GetExecutionHistoryAsync(int pageSize = 50, int pageNumber = 1, string? configurationId = null)
    {
        return _executionHistoryStore.GetPageAsync(pageSize, pageNumber, configurationId);
    }

    public Task<ExecutionHistory?> GetExecutionHistoryAsync(string executionId)
    {
        return _executionHistoryStore.GetAsync(executionId);
    }

    public Task<bool> DeleteExecutionHistoryAsync(string executionId)
    {
        return _executionHistoryStore.DeleteAsync(executionId);
    }

    public Task<List<ExecutionHistory>> GetRecentExecutionsAsync(int count = 10)
    {
        return _executionHistoryStore.GetRecentAsync(count);
    }

    public Task<ExecutionStatistics> GetAggregatedStatisticsAsync(DateTime startDate, DateTime endDate, string? configurationId = null)
    {
        return _executionHistoryStore.GetAggregatedStatisticsAsync(startDate, endDate, configurationId);
    }

    public async Task<string?> ExportExecutionResultsAsync(string executionId, string format = "csv")
    {
        var execution = await GetExecutionHistoryAsync(executionId);
        if (execution == null) return null;

        if (!string.IsNullOrEmpty(execution.ResultsFilePath) && File.Exists(execution.ResultsFilePath))
        {
            return execution.ResultsFilePath;
        }

        _logger.LogWarning("Results file not found for execution {ExecutionId}", executionId);
        return null;
    }
}

/// <summary>
/// Progress information for execution updates
/// </summary>
public class ExecutionProgress
{
    public int CompletedRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public double AverageResponseTime { get; set; }
    public string CurrentPhase { get; set; } = string.Empty;
}

