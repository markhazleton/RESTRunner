using Newtonsoft.Json;
using RESTRunner.Domain.Interfaces;
using RESTRunner.Domain.Models;
using RESTRunner.Extensions;
using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RESTRunner.Services
{
    public class ExecuteRunnerService : MustInitialize<CompareRunner>, IExecuteRunner
    {
        private readonly CompareRunner runner;

        public ExecuteRunnerService(CompareRunner therunner) : base(therunner)
        {
            runner = therunner;
        }

        private async Task<CompareResults> GetResponseAsync(CompareInstance env, CompareRequest req, CompareUser user)
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

            if (req.Body?.Properties?.Count > 0)
            {
                var x = req.Body.Properties.Select(s => new { key = s.Key, value = s.Value });
                var y = JsonConvert.SerializeObject(x);

            }
            // TODO: Is Token Still Valid 
            // TODO: Polly to Check 

            return await client.GetResponse(env, req, runner.SessionId);
        }
        private async Task GetTokenForEachInstance()
        {
            var user = runner.Users.FirstOrDefault();
            if (user == null)
            {
                user = new CompareUser();
            }

            foreach (var inst in runner.Instances)
            {
                var client = new RestClient() { Timeout = -1 };
                inst.UserToken = await client.GetUserToken(runner.SessionId, user.UserName, user.Password);
                inst.ClientToken = await client.GetClientToken(runner.SessionId, user.UserName, user.Password);
            }
        }

        /// <summary>
        /// Execute a RESTRunner and Returns Results
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<CompareResults>> ExecuteRunnerAsync()
        {
          //  await GetTokenForEachInstance();

            var tasks = new List<Task>();
            foreach (var user in runner.Users)
            {
                foreach (var req in runner.Requests)
                {
                    foreach (var env in runner.Instances)
                    {
                        tasks.Add(
                            Task.Run(
                                async () =>
                                {
                                    runner.StoreResults.Add(await GetResponseAsync(env, req, user));
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
            return runner.StoreResults.Results();
        }
    }
}
