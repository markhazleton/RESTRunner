using System.Text;

namespace RESTRunner.Services.HttpClientRunner;
/// <summary>
/// 
/// </summary>
public class ExecuteRunnerService : MustInitialize<CompareRunner>, IExecuteRunner
{
    private readonly Object ConsoleWriterLock = new();
    private readonly CompareRunner runner;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="therunner"></param>
    public ExecuteRunnerService(CompareRunner therunner) : base(therunner)
    {
        runner = therunner;
    }

    private async Task<CompareResult?> GetResponseAsync(CompareInstance env, CompareRequest req, CompareUser user)
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

        using HttpClient client = HttpClientFactory.Create();
        Stopwatch stopw = new();
        HttpResponseMessage response;
        if (req?.RequestMethod == HttpVerb.GET)
        {
            stopw.Start();
            Uri requestUri = new($"{env.BaseUrl}{req.Path}");
            response = await client.GetAsync(requestUri);
            stopw.Stop();
            LogMessage($"{env.Name}:{(int)response.StatusCode} IN:{stopw.ElapsedMilliseconds,7:n0}  FOR: {req.RequestMethod}-{env.BaseUrl}{user.GetMergedString(req.Path)}");
            return await GetResultAsync(response, env, req, user, stopw.ElapsedMilliseconds);
        }
        if (req?.RequestMethod == HttpVerb.POST)
        {
            stopw.Start();
            Uri requestUri = new($"{env.BaseUrl}{req.Path}");
            var content = new StringContent(req.BodyTemplate, Encoding.UTF8, "application/json");
            response = await client.PostAsync(requestUri, content);
            stopw.Stop();
            LogMessage($"{env.Name}:{(int)response.StatusCode} IN:{stopw.ElapsedMilliseconds,7:n0}  FOR: {req.RequestMethod}-{env.BaseUrl}{user.GetMergedString(req.Path)}");
            return await GetResultAsync(response, env, req, user, stopw.ElapsedMilliseconds);
        }
        if (req?.RequestMethod == HttpVerb.PUT)
        {
            stopw.Start();
            Uri requestUri = new($"{env.BaseUrl}{req.Path}");
            var content = new StringContent(req.BodyTemplate, Encoding.UTF8, "application/json");
            response = await client.PutAsync(requestUri, content);
            stopw.Stop();
            LogMessage($"{env.Name}:{(int)response.StatusCode} IN:{stopw.ElapsedMilliseconds,7:n0}  FOR: {req.RequestMethod}-{env.BaseUrl}{user.GetMergedString(req.Path)}");
            return await GetResultAsync(response, env, req, user, stopw.ElapsedMilliseconds);
        }
        return null;
    }
    private static async Task<CompareResult> GetResultAsync(HttpResponseMessage response, CompareInstance env, CompareRequest req, CompareUser user, long elapsedMilliseconds)
    {
        string content = await response.Content.ReadAsStringAsync();
        string? shortPath = req?.Path;
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
    public async Task ExecuteRunnerAsync(IOutput output)
    {
        int requestCount = 0;
        var tasks = new List<Task>();
        var semaphore = new SemaphoreSlim(initialCount: 300);
        for (int i = 0; i < 3000; i++)
        {
            LogMessage($"Starting Iteration {i}", ConsoleColor.Yellow);
            foreach (var env in runner.Instances)
            {
                LogMessage($"Starting {env.Name} with {runner.Users.Count} users", ConsoleColor.Yellow);
                foreach (var user in runner.Users)
                {
                    LogMessage($"Starting {user.UserName} with {runner.Requests.Count} requests", ConsoleColor.Yellow);
                    foreach (var req in runner.Requests)
                    {
                        requestCount++;
                        await semaphore.WaitAsync();

                        tasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                var result = await GetResponseAsync(env, req, user);
                                if (result != null) output.WriteInfo(result);
                            }
                            finally
                            {
                                semaphore.Release();
                            }
                        }));
                    }
                }
            }
        }
        Task t = Task.WhenAll(tasks.ToArray());
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