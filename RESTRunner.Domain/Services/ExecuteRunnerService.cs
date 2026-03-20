using Microsoft.Extensions.Logging;
using System.Text;

namespace RESTRunner.Domain.Services;

/// <summary>
/// Executes a CompareRunner and collects execution statistics
/// </summary>
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
                    var requestUri = new Uri($"{env.BaseUrl}{req.Path}");
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

            var elapsedMs = sw.ElapsedMilliseconds;
            stats.AddResponseTime(elapsedMs);
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

            stats.IncrementTotalRequests();
            stats.IncrementFailedRequests();
            stats.AddResponseTime(elapsedMs);
            stats.RequestsByMethod.AddOrUpdate(methodName, 1, (_, count) => count + 1);
            stats.RequestsByInstance.AddOrUpdate(env.Name ?? "Unknown", 1, (_, count) => count + 1);
            stats.RequestsByUser.AddOrUpdate(user.UserName ?? "Unknown", 1, (_, count) => count + 1);
            stats.RequestsByStatusCode.AddOrUpdate("Error", 1, (_, count) => count + 1);

            logger.LogError(ex, "HTTP request failed: {Method} {BaseUrl}{Path}", methodName, env.BaseUrl, requestPath);

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

            stats.IncrementTotalRequests();
            stats.IncrementFailedRequests();
            stats.AddResponseTime(elapsedMs);
            stats.RequestsByMethod.AddOrUpdate(methodName, 1, (_, count) => count + 1);
            stats.RequestsByInstance.AddOrUpdate(env.Name ?? "Unknown", 1, (_, count) => count + 1);
            stats.RequestsByUser.AddOrUpdate(user.UserName ?? "Unknown", 1, (_, count) => count + 1);
            stats.RequestsByStatusCode.AddOrUpdate("Exception", 1, (_, count) => count + 1);

            logger.LogError(ex, "Unexpected error during request: {Method} {BaseUrl}{Path}", methodName, env.BaseUrl, requestPath);

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

        return new CompareResult()
        {
            UserName = user.UserName,
            SessionId = env.SessionId,
            Instance = env.Name,
            Verb = req.RequestMethod.ToString(),
            Request = req.Path ?? string.Empty,
            Success = response.IsSuccessStatusCode,
            ResultCode = ((int)response.StatusCode).ToString(),
            StatusDescription = response.ReasonPhrase,
            Hash = content.GetDeterministicHashCode(),
            Duration = elapsedMilliseconds,
            LastRunDate = DateTime.Now
        };
    }

    public async Task<ExecutionStatistics> ExecuteRunnerAsync(IOutput output, CancellationToken ct = default)
    {
        var stats = new ExecutionStatistics { StartTime = DateTime.UtcNow };
        var tasks = new List<Task>();
        var semaphore = new SemaphoreSlim(initialCount: compareRunner.MaxConcurrency);

        try
        {
            logger.LogInformation("Starting REST Runner execution with {InstanceCount} instances, {UserCount} users, {RequestCount} requests",
                compareRunner.Instances.Count, compareRunner.Users.Count, compareRunner.Requests.Count);

            for (int i = 0; i < compareRunner.Iterations; i++)
            {
                ct.ThrowIfCancellationRequested();

                foreach (var env in compareRunner.Instances)
                {
                    foreach (var user in compareRunner.Users)
                    {
                        foreach (var req in compareRunner.Requests)
                        {
                            await semaphore.WaitAsync(ct);

                            tasks.Add(Task.Run(async () =>
                            {
                                try
                                {
                                    var result = await GetResponseAsync(env, req, user, stats, ct);
                                    if (result != null)
                                        output.WriteInfo(result);
                                }
                                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                                {
                                    // Expected when cancellation is requested
                                }
                                catch (Exception ex)
                                {
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

            await Task.WhenAll(tasks);
            logger.LogInformation("All tasks completed");
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
}


