using Newtonsoft.Json;
using RESTRunner.Domain.Extensions;
using RESTRunner.Domain.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace RESTRunner.Extensions
{
    public static class RestClient_Extensions
    {
        private static RestRequest GetRequest(string token, string SessionId)
        {
            RestRequest request = new(Method.GET);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddHeader("bsw-CorrelationId", SessionId);
            request.AddHeader("bsw-SessionId", SessionId);
            request.AddHeader("Authorization", $"bearer {token}");
            return request;
        }

        private static CompareResults GetResult(IRestResponse response, CompareInstance env, CompareRequest req)
        {

            if (response.IsSuccessful)
            {
                return new CompareResults()
                {
                    Instance = env.Name,
                    Verb = req.RequestMethod.ToString(),
                    Request = req.Path,
                    Success = true,
                    ResultCode = ((int)response.StatusCode).ToString(),
                    StatusDescription = response.StatusDescription,
                    Content = response.Content,
                    Hash = response.Content.GetDeterministicHashCode(),
                    LastRunDate = DateTime.Now
                };
            }
            else
            {
                return new CompareResults()
                {
                    Verb = req.RequestMethod.ToString(),
                    Instance = env.Name,
                    Request = req.Path,
                    Success = false,
                    ResultCode = ((int)response.StatusCode).ToString(),
                    StatusDescription = response.StatusDescription,
                    Content = string.Empty,
                    Hash = -1,
                    LastRunDate = DateTime.Now
                };
            }
        }
        private static RestRequest PostRequest(string token, string SessionId, string BodyTemplate)
        {
            RestRequest request = new(Method.POST);
            request.AddHeader("Authorization", $"bearer {token}");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("bsw-CorrelationId", SessionId);
            request.AddHeader("bsw-SessionId", SessionId);
            request.AddParameter("application/json", BodyTemplate, ParameterType.RequestBody);
            return request;
        }

        public static async Task<CompareResults> GetResponse(this RestClient client, CompareInstance env, CompareRequest req, string SessionId)
        {
            client.BaseUrl = new Uri($"{env.BaseUrl}{req.Path}");
            Stopwatch stopw = new();
            var Results = new CompareResults()
            {
                Verb = req.RequestMethod.ToString(),
                Instance = env.Name,
                Request = req.Path,
                Success = false,
                Content = string.Empty,
                Hash = -1,
                LastRunDate = DateTime.Now
            };

            stopw.Start();

            if (req.RequestMethod == HttpVerb.GET)
            {
                var response = await client.ExecuteAsync(GetRequest(env.UserToken, SessionId)).ConfigureAwait(false);
                Results = GetResult(response, env, req);

            }
            else if (req.RequestMethod == HttpVerb.POST)
            {
                if (req.RequiresClientToken)
                {
                    var response = await client.ExecuteAsync(PostRequest(env.ClientToken, SessionId, req.BodyTemplate)).ConfigureAwait(false);
                    Results = GetResult(response, env, req);
                }
                else
                {
                    var response = await client.ExecuteAsync(PostRequest(env.UserToken, SessionId, req.BodyTemplate)).ConfigureAwait(false);
                    Results = GetResult(response, env, req);
                }
            }
            stopw.Stop();
            Results.Duration = stopw.ElapsedMilliseconds;
            return Results;

        }
        public static async Task<string> GetClientToken(this RestClient client, string SessionId, string userName, string userPassword)
        {
            const string baseUrl = "https://api-tst.bswhive.com/oauth/token";
            client.BaseUrl = new Uri(baseUrl);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddHeader("bsw-CorrelationId", SessionId);
            request.AddHeader("bsw-SessionId", SessionId);
            request.AddParameter("grant_type", "client_credentials");
            request.AddParameter("client_id", "MyBSWHealth.Mobile");
            IRestResponse response = await client.ExecuteAsync(request);
            var token = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content)["access_token"].ToString();
            if (token.Length == 0)
            {
                throw new AuthenticationException("API authentication failed.");
            }
            return token;
        }
        public static async Task<string> GetUserToken(this RestClient client, string SessionId, string userName, string userPassword)
        {
            const string baseUrl = "https://api-tst.bswhive.com/oauth/token";
            client.BaseUrl = new Uri(baseUrl);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddHeader("bsw-CorrelationId", SessionId);
            request.AddHeader("bsw-SessionId", SessionId);
            request.AddParameter("username", userName);
            request.AddParameter("password", userPassword);
            request.AddParameter("grant_type", "password");
            request.AddParameter("client_id", "MyBSWHealth.Web");
            request.AddParameter("client_secret", "WeTheBest@Web");
            IRestResponse response = await client.ExecuteAsync(request);
            var token = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content)["access_token"].ToString();
            if (token.Length == 0)
            {
                throw new AuthenticationException("API authentication failed.");
            }
            return token;
        }
    }
}
