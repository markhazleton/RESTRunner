using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using RESTRunner.Domain.Extensions;
using RESTRunner.Domain.Interfaces;
using RESTRunner.Domain.Models;
using RESTRunner.Domain.Outputs;
using RESTRunner.Services.HttpClientRunner;
using RESTRunner.Web.Hubs;
using RESTRunner.Web.Models;

namespace RESTRunner.Web.Services;

/// <summary>
/// Real implementation of execution service with actual test execution capabilities
/// </summary>
public class RealExecutionService : IExecutionService
{
    private readonly IConfigurationService _configurationService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IFileStorageService _fileStorageService;
    private readonly IHubContext<ExecutionHub> _hubContext;
    private readonly ILogger<RealExecutionService> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ConcurrentDictionary<string, TestExecution> _runningExecutions = new();
    private readonly ConcurrentDictionary<string, ExecutionHistory> _executionHistory = new();

    public RealExecutionService(
        IConfigurationService configurationService,
        IHttpClientFactory httpClientFactory,
        IFileStorageService fileStorageService,
        IHubContext<ExecutionHub> hubContext,
        ILoggerFactory loggerFactory,
        ILogger<RealExecutionService> logger)
    {
        _configurationService = configurationService;
        _httpClientFactory = httpClientFactory;
        _fileStorageService = fileStorageService;
        _hubContext = hubContext;
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    public async Task<TestExecution> StartExecutionAsync(string configurationId, string executedBy = "System")
    {
        try
        {
            // Load configuration
            var config = await _configurationService.GetByIdAsync(configurationId);
            if (config == null)
            {
                throw new InvalidOperationException($"Configuration {configurationId} not found");
            }

            // Validate configuration
            if (!config.IsValid())
            {
                throw new InvalidOperationException("Configuration is not valid");
            }

            // Create execution tracking object
            var execution = new TestExecution
            {
                ConfigurationId = configurationId,
                ConfigurationName = config.Name,
                ExecutedBy = executedBy,
                Status = ExecutionStatus.Pending,
                StartTime = DateTime.UtcNow,
                TotalRequests = config.GetTotalTestCount(),
                CurrentPhase = "Initializing...",
                CancellationTokenSource = new CancellationTokenSource()
            };

            // Add to running executions
            _runningExecutions.TryAdd(execution.Id, execution);

            _logger.LogInformation("Started execution {ExecutionId} for configuration {ConfigurationName}", 
                execution.Id, config.Name);

            // Broadcast execution started
            await _hubContext.Clients.All.SendAsync("ExecutionStarted", execution);

            // Start execution in background
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

            // Create execution history record
            history = new ExecutionHistory
            {
                Id = execution.Id,
                ConfigurationId = execution.ConfigurationId,
                ConfigurationName = execution.ConfigurationName,
                StartTime = execution.StartTime,
                Status = ExecutionStatus.Running,
                ExecutedBy = execution.ExecutedBy,
                Tags = config.Tags
            };

            // Create ExecuteRunnerService with the configuration's CompareRunner
            var executeRunnerLogger = _loggerFactory.CreateLogger<ExecuteRunnerService>();
            var executeRunner = new ExecuteRunnerService(
                config.Runner,
                _httpClientFactory,
                executeRunnerLogger);

            // Create output handler for CSV results
            var resultsFileName = $"execution_{execution.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            var resultsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Data", "results");
            
            // Ensure results directory exists
            if (!Directory.Exists(resultsDirectory))
            {
                Directory.CreateDirectory(resultsDirectory);
            }
            
            resultsFilePath = Path.Combine(resultsDirectory, resultsFileName);

            var output = new CsvOutput(resultsFilePath);

            execution.CurrentPhase = "Executing requests...";
            execution.LastUpdate = DateTime.UtcNow;

            // Create progress reporter
            var progressReporter = new Progress<ExecutionProgress>(progress =>
            {
                execution.UpdateProgress(
                    progress.CompletedRequests,
                    progress.SuccessfulRequests,
                    progress.FailedRequests,
                    progress.AverageResponseTime,
                    progress.CurrentPhase);
                
                // Broadcast progress via SignalR
                _ = _hubContext.Clients.Group($"execution_{execution.Id}")
                    .SendAsync("ProgressUpdate", execution);
            });

            // Execute with cancellation support and progress reporting
            var statistics = await ExecuteWithProgressAsync(
                executeRunner,
                output,
                config,
                execution.CancellationTokenSource!.Token,
                progressReporter);

            // Finalize statistics
            statistics.FinalizeStatistics();

            // Update execution
            execution.MarkCompleted();
            history.Statistics = statistics;
            history.EndTime = DateTime.UtcNow;
            history.Status = ExecutionStatus.Completed;
            history.ResultsFilePath = resultsFilePath;

            // Broadcast completion via SignalR
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
            
            // Broadcast cancellation via SignalR
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

            // Broadcast failure via SignalR
            await _hubContext.Clients.Group($"execution_{execution.Id}")
                .SendAsync("ExecutionFailed", new { ExecutionId = execution.Id, ErrorMessage = errorMessage, Timestamp = DateTime.UtcNow });

            _logger.LogError(ex, "Execution {ExecutionId} failed", execution.Id);
        }
        finally
        {
            // Save execution history
            if (history != null)
            {
                _executionHistory.TryAdd(history.Id, history);
                
                // Persist to file
                try
                {
                    var historyFileName = $"history_{history.Id}.json";
                    var historyJson = System.Text.Json.JsonSerializer.Serialize(history, new System.Text.Json.JsonSerializerOptions 
                    { 
                        WriteIndented = true 
                    });
                    var historyFilePath = await _fileStorageService.SaveLogAsync(historyFileName, historyJson);
                    history.LogFilePath = historyFilePath;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to persist execution history {ExecutionId}", history.Id);
                }
            }

            // Remove from running executions
            _runningExecutions.TryRemove(execution.Id, out _);

            // Clean up cancellation token
            execution.CancellationTokenSource?.Dispose();
        }
    }

    private async Task<ExecutionStatistics> ExecuteWithProgressAsync(
        IExecuteRunner executeRunner,
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
            // Report initial progress
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
                    AverageResponseTime = statistics.AverageResponseTime,
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
                                    // This would call the actual execution logic
                                    // For now, we'll use the ExecuteRunnerService pattern
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

                                    // Report progress every 10 requests
                                    if (completedTests % 10 == 0)
                                    {
                                        progress.Report(new ExecutionProgress
                                        {
                                            CompletedRequests = completedTests,
                                            SuccessfulRequests = statistics.SuccessfulRequests,
                                            FailedRequests = statistics.FailedRequests,
                                            AverageResponseTime = statistics.AverageResponseTime,
                                            CurrentPhase = $"Iteration {iteration + 1}/{config.Iterations}"
                                        });
                                    }
                                }
                                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                                {
                                    // Expected when cancellation is requested
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
                AverageResponseTime = statistics.AverageResponseTime,
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
        // This mirrors the logic from ExecuteRunnerService.GetResponseAsync
        // For now, we'll create a simple HTTP client call
        using var client = _httpClientFactory.CreateClient();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // Apply template substitutions
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

            // Execute based on HTTP method
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

            // Update statistics
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

    public async Task<TestExecution?> GetExecutionAsync(string executionId)
    {
        _runningExecutions.TryGetValue(executionId, out var execution);
        return execution;
    }

    public async Task<List<TestExecution>> GetRunningExecutionsAsync()
    {
        return _runningExecutions.Values
            .Where(e => e.Status == ExecutionStatus.Running || e.Status == ExecutionStatus.Pending)
            .ToList();
    }

    public async Task<bool> CancelExecutionAsync(string executionId, string cancelledBy = "System")
    {
        if (_runningExecutions.TryGetValue(executionId, out var execution))
        {
            if (execution.CanBeCancelled)
            {
                execution.CancellationTokenSource?.Cancel();
                _logger.LogInformation("Cancelled execution {ExecutionId} by {CancelledBy}", executionId, cancelledBy);
                return true;
            }
        }
        return false;
    }

    public async Task<List<ExecutionHistory>> GetExecutionHistoryAsync(int pageSize = 50, int pageNumber = 1, string? configurationId = null)
    {
        var query = _executionHistory.Values.AsEnumerable();
        
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
        _executionHistory.TryGetValue(executionId, out var history);
        return history;
    }

    public async Task<bool> DeleteExecutionHistoryAsync(string executionId)
    {
        if (_executionHistory.TryRemove(executionId, out var execution))
        {
            _logger.LogInformation("Deleted execution history {ExecutionId}", executionId);
            
            // Also delete physical files
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
                _logger.LogError(ex, "Failed to delete execution files for {ExecutionId}", executionId);
            }
            
            return true;
        }
        return false;
    }

    public async Task<List<ExecutionHistory>> GetRecentExecutionsAsync(int count = 10)
    {
        return _executionHistory.Values
            .OrderByDescending(e => e.StartTime)
            .Take(count)
            .ToList();
    }

    public async Task<ExecutionStatistics> GetAggregatedStatisticsAsync(DateTime startDate, DateTime endDate, string? configurationId = null)
    {
        var executions = _executionHistory.Values.Where(e => 
            e.StartTime >= startDate && 
            e.StartTime <= endDate &&
            (configurationId == null || e.ConfigurationId == configurationId) &&
            e.Statistics != null);

        if (!executions.Any())
        {
            return new ExecutionStatistics();
        }

        var stats = new ExecutionStatistics
        {
            StartTime = startDate,
            EndTime = endDate
        };

        foreach (var execution in executions)
        {
            if (execution.Statistics != null)
            {
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
        }

        stats.FinalizeStatistics();
        return stats;
    }

    public async Task<string?> ExportExecutionResultsAsync(string executionId, string format = "csv")
    {
        var execution = await GetExecutionHistoryAsync(executionId);
        if (execution == null) return null;

        // Return existing results file if available
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
