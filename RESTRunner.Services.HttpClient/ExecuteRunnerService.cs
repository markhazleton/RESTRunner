
using System.Text;

namespace RESTRunner.Services.HttpClientRunner;
/// <summary>
/// 
/// </summary>
public class ExecuteRunnerService : MustInitialize<CompareRunner>, IExecuteRunner
{
    private readonly CompareRunner runner;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="therunner"></param>
    public ExecuteRunnerService(CompareRunner therunner) : base(therunner)
    {
        runner = therunner;
    }

    private static async Task<CompareResult?> GetResponseAsync(CompareInstance env, CompareRequest req, CompareUser user)
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
        HttpResponseMessage response = new();
        HttpClient client = new();
        Stopwatch stopw = new();
        if (req?.RequestMethod == HttpVerb.GET)
        {
            stopw.Start();
            Uri requestUri = new($"{env.BaseUrl}{req.Path}");
            response = await client.GetAsync(requestUri);
            Console.WriteLine($"{(int)response.StatusCode} IN:{stopw.ElapsedMilliseconds,7:n0}  FOR: {req.RequestMethod}-{env.BaseUrl}{user.GetMergedString(req.Path)}");
            stopw.Stop();
            return await GetResultAsync(response, env, req, user, stopw.ElapsedMilliseconds);
        }
        if (req?.RequestMethod == HttpVerb.POST)
        {
            stopw.Start();
            Uri requestUri = new($"{env.BaseUrl}{req.Path}");
            var content = new StringContent(req.BodyTemplate, Encoding.UTF8, "application/json");
            response = await client.PostAsync(requestUri, content);
            Console.WriteLine($"{(int)response.StatusCode} IN:{stopw.ElapsedMilliseconds,7:n0}  FOR: {req.RequestMethod}-{env.BaseUrl}{user.GetMergedString(req.Path)}");
            stopw.Stop();
            return await GetResultAsync(response, env, req, user, stopw.ElapsedMilliseconds);
        }
        if (req?.RequestMethod == HttpVerb.PUT)
        {
            stopw.Start();
            Uri requestUri = new($"{env.BaseUrl}{req.Path}");
            var content = new StringContent(req.BodyTemplate, Encoding.UTF8, "application/json");
            response = await client.PutAsync(requestUri, content);
            Console.WriteLine($"{(int)response.StatusCode} IN:{stopw.ElapsedMilliseconds,7:n0}  FOR: {req.RequestMethod}-{env.BaseUrl}{user.GetMergedString(req.Path)}");
            stopw.Stop();
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
            LastRunDate = DateTime.Now,
            Content = content
        };
    }

    /// <summary>
    /// Execute a RESTRunner and Returns Results
    /// </summary>
    /// <returns></returns>
    public async Task ExecuteRunnerAsync(IOutput output)
    {
        int requestCount = 0;
        var tasks = new List<Task>();
        var semaphore = new SemaphoreSlim(initialCount: 50);
        for (int i = 0; i < 100; i++)
        {
            foreach (var env in runner.Instances)
            {
                foreach (var user in runner.Users)
                {
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
        Console.WriteLine($"Total requestCount:{requestCount}");
        return;
    }
}