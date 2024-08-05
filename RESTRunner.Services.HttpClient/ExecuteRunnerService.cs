using System.Text;

namespace RESTRunner.Services.HttpClientRunner;

public class ExecuteRunnerService(
    CompareRunner compareRunner, 
    IHttpClientFactory HttpClientFactory) : IExecuteRunner
{
    private readonly HttpClient client = HttpClientFactory.CreateClient();
    private readonly object ConsoleWriterLock = new();

    private async Task<CompareResult?> GetResponseAsync(CompareInstance env, CompareRequest req, CompareUser user, CancellationToken ct = default)
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

        Stopwatch sw = new();
        HttpResponseMessage response;
        if (req?.RequestMethod == HttpVerb.GET)
        {
            sw.Start();
            Uri requestUri = new($"{env.BaseUrl}{req.Path}");
            response = await client.GetAsync(requestUri, ct);
            sw.Stop();
            LogMessage($"{env.Name}:{(int)response.StatusCode} IN:{sw.ElapsedMilliseconds,7:n0}  FOR: {req.RequestMethod}-{env.BaseUrl}{user.GetMergedString(req.Path)}");
            return await GetResultAsync(response, env, req, user, sw.ElapsedMilliseconds, ct);
        }
        if (req?.RequestMethod == HttpVerb.POST)
        {
            sw.Start();
            Uri requestUri = new($"{env.BaseUrl}{req.Path}");
            var content = new StringContent(req?.BodyTemplate ?? string.Empty, Encoding.UTF8, "application/json");
            response = await client.PostAsync(requestUri, content, ct);
            sw.Stop();
            LogMessage($"{env.Name}:{(int)response.StatusCode} IN:{sw.ElapsedMilliseconds,7:n0}  FOR: {req.RequestMethod}-{env.BaseUrl}{user.GetMergedString(req.Path)}");
            return await GetResultAsync(response, env, req, user, sw.ElapsedMilliseconds, ct);
        }
        if (req?.RequestMethod == HttpVerb.PUT)
        {
            sw.Start();
            Uri requestUri = new($"{env.BaseUrl}{req.Path}");
            var content = new StringContent(req?.BodyTemplate ?? string.Empty, Encoding.UTF8, "application/json");
            response = await client.PutAsync(requestUri, content, ct);
            sw.Stop();
            LogMessage($"{env.Name}:{(int)response.StatusCode} IN:{sw.ElapsedMilliseconds,7:n0}  FOR: {req.RequestMethod}-{env.BaseUrl}{user.GetMergedString(req.Path)}");
            return await GetResultAsync(response, env, req, user, sw.ElapsedMilliseconds, ct);
        }
        return null;
    }
    private static async Task<CompareResult> GetResultAsync(HttpResponseMessage response, CompareInstance env, CompareRequest req, CompareUser user, long elapsedMilliseconds, CancellationToken ct = default)
    {
        string content = await response.Content.ReadAsStringAsync(ct);
        string shortPath = req.Path;
        if (string.IsNullOrEmpty(shortPath)) shortPath = req.Path;
        return new CompareResult()
        {
            UserName = user.UserName,
            SessionId = env.SessionId,
            Instance = env.Name,
            Verb = req.RequestMethod.ToString(),
            Request = shortPath,
            Success = response?.IsSuccessStatusCode ?? default,
            ResultCode = response == null ? string.Empty : ((int)response.StatusCode).ToString(),
            StatusDescription = response?.ReasonPhrase,
            Hash = response == null ? 0 : content.GetDeterministicHashCode(),
            Duration = elapsedMilliseconds,
            LastRunDate = DateTime.Now
        };
    }


    private void LogMessage(string message, ConsoleColor consoleColor = ConsoleColor.Green)
    {
        lock (ConsoleWriterLock)
        {
            Console.ForegroundColor = consoleColor;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    /// <summary>
    /// Execute a RESTRunner and Returns Results
    /// </summary>
    /// <returns></returns>
    public async Task ExecuteRunnerAsync(IOutput output, CancellationToken ct = default)
    {
        int requestCount = 0;
        var tasks = new List<Task>();
        var semaphore = new SemaphoreSlim(initialCount: 10);
        for (int i = 0; i < 100; i++)
        {
            LogMessage($"Starting Iteration {i}", ConsoleColor.Yellow);
            foreach (var env in compareRunner.Instances)
            {
                LogMessage($"Starting {env.Name} with {compareRunner.Users.Count} users", ConsoleColor.Yellow);
                foreach (var user in compareRunner.Users)
                {
                    LogMessage($"Starting {user.UserName} with {compareRunner.Requests.Count} requests", ConsoleColor.Yellow);
                    foreach (var req in compareRunner.Requests)
                    {
                        requestCount++;
                        await semaphore.WaitAsync(ct);

                        tasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                var result = await GetResponseAsync(env, req, user, ct);
                                if (result != null) output.WriteInfo(result);
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
        Task t = Task.WhenAll([.. tasks]);
        try
        {
            await t;
        }
        catch
        {
        }
        if (t.Status == TaskStatus.RanToCompletion)
        {
        }
        LogMessage($"Total requestCount:{requestCount}");
        return;
    }
}