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
                    // Content = response.Content,
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
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", BodyTemplate, ParameterType.RequestBody);
            return request;
        }
        private static RestRequest PutRequest(string token, string SessionId, string BodyTemplate)
        {
            RestRequest request = new(Method.PUT);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Cookie", "ARRAffinity=382294a0db0654f4c3e275f730098da0f3a8328292bcd98467167211a9acbe7b; ARRAffinitySameSite=382294a0db0654f4c3e275f730098da0f3a8328292bcd98467167211a9acbe7b");
            var body = @"{" + "\n" + @"  ""age"": 22," + "\n" + @"  ""country"": ""USA"", " + "\n" + @"  ""department"": 4,
" + "\n" +
            @"  ""id"": 1,
" + "\n" +
            @"  ""name"": ""Monte Warden"",
" + "\n" +
            @"  ""state"": ""TX""
" + "\n" +
            @"}";
            request.AddParameter("application/json", body, ParameterType.RequestBody);


            //request.AddParameter("application/json", BodyTemplate, ParameterType.RequestBody);
            return request;
        }

        /// <summary>
        /// GetClientToken
        /// </summary>
        /// <param name="client"></param>
        /// <param name="userName"></param>
        /// <param name="userPassword"></param>
        /// <param name="baseUrl"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public static async Task<string> GetClientToken(this RestClient client, string userName, string userPassword, string baseUrl, string clientId, string client_secret)
        {
            client.BaseUrl = new Uri(baseUrl);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("grant_type", "client_credentials");
            request.AddParameter("client_id", clientId);
            request.AddParameter("client_secret", client_secret);
            IRestResponse response = await client.ExecuteAsync(request);
            var token = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content)["access_token"].ToString();
            if (token.Length == 0)
            {
                throw new AuthenticationException("API authentication failed.");
            }
            return token;
        }

        /// <summary>
        /// GetResponse
        /// </summary>
        /// <param name="client"></param>
        /// <param name="env"></param>
        /// <param name="req"></param>
        /// <param name="SessionId"></param>
        /// <returns></returns>
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
            else if (req.RequestMethod == HttpVerb.PUT)
            {
                if (req.RequiresClientToken)
                {
                    var response = await client.ExecuteAsync(PutRequest(env.ClientToken, SessionId, req.BodyTemplate)).ConfigureAwait(false);
                    Results = GetResult(response, env, req);
                }
                else
                {
                    var response = await client.ExecuteAsync(PutRequest(env.ClientToken, SessionId, req.BodyTemplate)).ConfigureAwait(false);
                    Results = GetResult(response, env, req);
                }
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
        public static async Task<string> GetUserToken(this RestClient client, string SessionId, string userName, string userPassword, string baseUrl, string client_id, string client_secret)
        {
            client.BaseUrl = new Uri(baseUrl);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("username", userName);
            request.AddParameter("password", userPassword);
            request.AddParameter("grant_type", "password");
            request.AddParameter("client_id", client_id);
            request.AddParameter("client_secret", client_secret);
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
