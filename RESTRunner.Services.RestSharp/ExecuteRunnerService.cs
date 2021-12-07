namespace RESTRunner.Services;

/// <summary>
/// ExecuteRunnerService 
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
    private static CompareResult GetResponse(CompareInstance env, CompareRequest req, CompareUser user)
    {
        var client = new RestClient() { Timeout = -1 };

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
        // TODO: Is Token Still Valid 
        // TODO: Polly to Check 
        return client.GetResponse(env, req, user);
    }

    /// <summary>
    /// Execute a RESTRunner and Returns Results
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<CompareResult>> ExecuteRunnerAsync()
    {
        var storeResults = new List<CompareResult>();
        var tasks = new List<Task>();
        var throttler = new SemaphoreSlim(initialCount: 30);
        for (int i = 0; i < 2000; i++)
        {
            foreach (var user in runner.Users)
            {
                foreach (var req in runner.Requests)
                {
                    foreach (var env in runner.Instances)
                    {
                        await throttler.WaitAsync();

                        tasks.Add(Task.Run(() =>
                                {
                                    try
                                    {
                                        storeResults.Add(GetResponse(env, req, user));
                                    }
                                    finally
                                    {
                                        throttler.Release();
                                    }
                                }));
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
            }
        }
        return storeResults;
    }
}
