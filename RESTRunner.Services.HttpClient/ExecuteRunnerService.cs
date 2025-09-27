using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;

namespace RESTRunner.Services.HttpClientRunner;

public class ExecuteRunnerService(
    CompareRunner compareRunner,
    IHttpClientFactory HttpClientFactory,
    ILogger<ExecuteRunnerService> logger) : IExecuteRunner
{
    private readonly HttpClient client = HttpClientFactory.CreateClient();

    private async Task<CompareResult?> GetResponseAsync(CompareInstance env, CompareRequest req, CompareUser user, ExecutionStatistics stats, CancellationToken ct = default)
    {
        if (!string.IsNullOrEmpty(req.BodyTemplate))
        {
            foreach (var prop in user.Properties)
            {
                req.BodyTemplate = req.BodyTemplate.Replace($"{{{prop.Key}}}", prop.Value);
            }
        }
        if (!string.IsNullOrEmpty(req.Path))
        {
            req.Path = req.Path.Replace(@"{{encoded_user_name}}", user.UserName);
            foreach (var prop in user.Properties)
            {
                req.Path = req.Path.Replace($"{{{prop.Key}}}", prop.Value);
            }
        }
        if (req?.Body?.Raw is not null)
        {
            req.BodyTemplate = req.Body.Raw;
        }
        if (req?.BodyTemplate is not null)
        {
            foreach (var prop in user.Properties)
            {
                req.BodyTemplate = req.BodyTemplate.Replace($"{{{prop.Key}}}", prop.Value);
            }
        }

        if (req is null) return null;

        Stopwatch sw = new();
        HttpResponseMessage? response = null;
        string requestPath = req.Path ?? string.Empty;
        string methodName = req.RequestMethod.ToString();
        
        try
        {
            switch (req.RequestMethod)
            {
                case HttpVerb.GET:
                    sw.Start();
                    Uri requestUri = new($"{env.BaseUrl}{req.Path}");
                    response = await client.GetAsync(requestUri, ct);
                    sw.Stop();
                    break;
                    
                case HttpVerb.POST:
                    sw.Start();
                    requestUri = new($"{env.BaseUrl}{req.Path}");
                    var content = new StringContent(req.BodyTemplate ?? string.Empty, Encoding.UTF8, "application/json");
                    response = await client.PostAsync(requestUri, content, ct);
                    sw.Stop();
                    break;
                    
                case HttpVerb.PUT:
                    sw.Start();
                    requestUri = new($"{env.BaseUrl}{req.Path}");
                    content = new StringContent(req.BodyTemplate ?? string.Empty, Encoding.UTF8, "application/json");
                    response = await client.PutAsync(requestUri, content, ct);
                    sw.Stop();
                    break;
                    
                case HttpVerb.DELETE:
                    sw.Start();
                    requestUri = new($"{env.BaseUrl}{req.Path}");
                    response = await client.DeleteAsync(requestUri, ct);
                    sw.Stop();
                    break;
                    
                case HttpVerb.HEAD:
                    sw.Start();
                    requestUri = new($"{env.BaseUrl}{req.Path}");
                    using (var headRequest = new HttpRequestMessage(HttpMethod.Head, requestUri))
                    {
                        response = await client.SendAsync(headRequest, ct);
                    }
                    sw.Stop();
                    break;
                    
                case HttpVerb.OPTIONS:
                    sw.Start();
                    requestUri = new($"{env.BaseUrl}{req.Path}");
                    using (var optionsRequest = new HttpRequestMessage(HttpMethod.Options, requestUri))
                    {
                        response = await client.SendAsync(optionsRequest, ct);
                    }
                    sw.Stop();
                    break;
                    
                case HttpVerb.PATCH:
                    sw.Start();
                    requestUri = new($"{env.BaseUrl}{req.Path}");
                    content = new StringContent(req.BodyTemplate ?? string.Empty, Encoding.UTF8, "application/json");
                    using (var patchRequest = new HttpRequestMessage(HttpMethod.Patch, requestUri) { Content = content })
                    {
                        response = await client.SendAsync(patchRequest, ct);
                    }
                    sw.Stop();
                    break;
                    
                case HttpVerb.MERGE:
                case HttpVerb.COPY:
                    // For less common HTTP methods, use SendAsync with custom HttpMethod
                    sw.Start();
                    requestUri = new($"{env.BaseUrl}{req.Path}");
                    var customMethod = new HttpMethod(req.RequestMethod.ToString());
                    using (var customRequest = new HttpRequestMessage(customMethod, requestUri))
                    {
                        if (req.RequiresBody())
                        {
                            customRequest.Content = new StringContent(req.BodyTemplate ?? string.Empty, Encoding.UTF8, "application/json");
                        }
                        response = await client.SendAsync(customRequest, ct);
                    }
                    sw.Stop();
                    break;
                    
                default:
                    logger.LogWarning("Unsupported HTTP method: {Method}", req.RequestMethod);
                    return null;
            }

            // Update statistics
            var elapsedMs = sw.ElapsedMilliseconds;
            stats.AddResponseTime(elapsedMs);
            
            // Thread-safe increment operations
            stats.IncrementTotalRequests();
            stats.RequestsByMethod.AddOrUpdate(methodName, 1, (_, count) => count + 1);
            stats.RequestsByInstance.AddOrUpdate(env.Name ?? "Unknown", 1, (_, count) => count + 1);
            stats.RequestsByUser.AddOrUpdate(user.UserName ?? "Unknown", 1, (_, count) => count + 1);
            
            if (response != null)
            {
                var statusCode = ((int)response.StatusCode).ToString();
                stats.RequestsByStatusCode.AddOrUpdate(statusCode, 1, (_, count) => count + 1);
                
                if (response.IsSuccessStatusCode)
                {
                    stats.IncrementSuccessfulRequests();
                }
                else
                {
                    stats.IncrementFailedRequests();
                }

                logger.LogInformation("{InstanceName}:{StatusCode} IN:{ElapsedMilliseconds,7:n0}ms FOR: {RequestMethod}-{BaseUrl}{RequestPath}", 
                    env.Name, (int)response.StatusCode, elapsedMs, req.RequestMethod, env.BaseUrl, requestPath);
            }

            return await GetResultAsync(response, env, req, user, elapsedMs, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            logger.LogInformation("Request cancelled: {Method} {BaseUrl}{Path}", methodName, env.BaseUrl, requestPath);
            throw;
        }
        catch (HttpRequestException ex)
        {
            sw.Stop();
            var elapsedMs = sw.ElapsedMilliseconds;
            
            // Update statistics for failed request
            stats.IncrementTotalRequests();
            stats.IncrementFailedRequests();
            stats.AddResponseTime(elapsedMs);
            stats.RequestsByMethod.AddOrUpdate(methodName, 1, (_, count) => count + 1);
            stats.RequestsByInstance.AddOrUpdate(env.Name ?? "Unknown", 1, (_, count) => count + 1);
            stats.RequestsByUser.AddOrUpdate(user.UserName ?? "Unknown", 1, (_, count) => count + 1);
            stats.RequestsByStatusCode.AddOrUpdate("Error", 1, (_, count) => count + 1);
            
            logger.LogError(ex, "HTTP request failed: {Method} {BaseUrl}{Path}", methodName, env.BaseUrl, requestPath);
            
            // Return error result
            return CompareResult.CreateFailure(
                env.Name ?? "Unknown", 
                requestPath, 
                methodName, 
                ex.Message, 
                elapsedMs, 
                env.SessionId);
        }
        catch (Exception ex)
        {
            sw.Stop();
            var elapsedMs = sw.ElapsedMilliseconds;
            
            // Update statistics for failed request
            stats.IncrementTotalRequests();
            stats.IncrementFailedRequests();
            stats.AddResponseTime(elapsedMs);
            stats.RequestsByMethod.AddOrUpdate(methodName, 1, (_, count) => count + 1);
            stats.RequestsByInstance.AddOrUpdate(env.Name ?? "Unknown", 1, (_, count) => count + 1);
            stats.RequestsByUser.AddOrUpdate(user.UserName ?? "Unknown", 1, (_, count) => count + 1);
            stats.RequestsByStatusCode.AddOrUpdate("Exception", 1, (_, count) => count + 1);
            
            logger.LogError(ex, "Unexpected error during request: {Method} {BaseUrl}{Path}", methodName, env.BaseUrl, requestPath);
            
            // Return error result
            return CompareResult.CreateFailure(
                env.Name ?? "Unknown", 
                requestPath, 
                methodName, 
                ex.Message, 
                elapsedMs, 
                env.SessionId);
        }
        finally
        {
            response?.Dispose();
        }
    }

    private static async Task<CompareResult> GetResultAsync(HttpResponseMessage? response, CompareInstance env, CompareRequest req, CompareUser user, long elapsedMilliseconds, CancellationToken ct = default)
    {
        if (response == null)
        {
            return CompareResult.CreateFailure(
                env.Name ?? "Unknown",
                req.Path ?? string.Empty,
                req.RequestMethod.ToString(),
                "No response received",
                elapsedMilliseconds,
                env.SessionId);
        }

        string content = await response.Content.ReadAsStringAsync(ct);
        string shortPath = req.Path ?? string.Empty;
        
        return new CompareResult()
        {
            UserName = user.UserName,
            SessionId = env.SessionId,
            Instance = env.Name,
            Verb = req.RequestMethod.ToString(),
            Request = shortPath,
            Success = response.IsSuccessStatusCode,
            ResultCode = ((int)response.StatusCode).ToString(),
            StatusDescription = response.ReasonPhrase,
            Hash = content.GetDeterministicHashCode(),
            Duration = elapsedMilliseconds,
            LastRunDate = DateTime.Now
        };
    }

    /// <summary>
    /// Execute a RESTRunner and Returns Results with Statistics
    /// </summary>
    /// <param name="output">The output handler to write results to</param>
    /// <param name="ct">Cancellation token to cancel the operation</param>
    /// <returns>Execution statistics</returns>
    public async Task<ExecutionStatistics> ExecuteRunnerAsync(IOutput output, CancellationToken ct = default)
    {
        var stats = new ExecutionStatistics
        {
            StartTime = DateTime.UtcNow
        };

        var tasks = new List<Task>();
        var semaphore = new SemaphoreSlim(initialCount: 10);
        
        try
        {
            logger.LogInformation("Starting REST Runner execution with {InstanceCount} instances, {UserCount} users, {RequestCount} requests", 
                compareRunner.Instances.Count, compareRunner.Users.Count, compareRunner.Requests.Count);

            for (int i = 0; i < 100; i++)
            {
                ct.ThrowIfCancellationRequested();
                
                logger.LogInformation("Starting Iteration {Iteration}", i);
                
                foreach (var env in compareRunner.Instances)
                {
                    logger.LogInformation("Starting {InstanceName} with {UserCount} users", env.Name, compareRunner.Users.Count);
                    
                    foreach (var user in compareRunner.Users)
                    {
                        logger.LogInformation("Starting {UserName} with {RequestCount} requests", user.UserName, compareRunner.Requests.Count);
                        
                        foreach (var req in compareRunner.Requests)
                        {
                            await semaphore.WaitAsync(ct);

                            tasks.Add(Task.Run(async () =>
                            {
                                try
                                {
                                    var result = await GetResponseAsync(env, req, user, stats, ct);
                                    if (result != null)
                                    {
                                        output.WriteInfo(result);
                                    }
                                }
                                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                                {
                                    // Expected when cancellation is requested
                                }
                                catch (Exception ex)
                                {
                                    logger.LogError(ex, "Error processing request for {Instance} - {User} - {Method} {Path}", 
                                        env.Name, user.UserName, req.RequestMethod, req.Path);
                                    
                                    // Write error result to output
                                    var errorResult = CompareResult.CreateFailure(
                                        env.Name ?? "Unknown",
                                        req.Path ?? string.Empty,
                                        req.RequestMethod.ToString(),
                                        ex.Message,
                                        0,
                                        env.SessionId);
                                    
                                    output.WriteError(errorResult);
                                }
                                finally
                                {
                                    semaphore.Release();
                                }
                            }, ct));
                        }
                    }
                }
            }

            // Wait for all tasks to complete
            logger.LogInformation("Waiting for {TaskCount} tasks to complete", tasks.Count);
            await Task.WhenAll(tasks);
            
            logger.LogInformation("All tasks completed successfully");
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
            // Finalize statistics
            stats.FinalizeStatistics();
            
            // Log final statistics
            logger.LogInformation("Execution completed. {Statistics}", stats.ToString());
            logger.LogInformation("Detailed Statistics - P50: {P50}ms, P95: {P95}ms, P99: {P99}ms", 
                stats.GetResponseTimePercentile(50),
                stats.GetResponseTimePercentile(95),
                stats.GetResponseTimePercentile(99));
        }

        return stats;
    }
}