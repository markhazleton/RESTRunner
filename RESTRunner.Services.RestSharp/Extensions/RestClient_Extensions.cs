using Newtonsoft.Json;
using RESTRunner.Domain.Extensions;
using RESTRunner.Domain.Models;
using RestSharp;
using System.Diagnostics;
using System.Security.Authentication;

namespace RESTRunner.Extensions;

/// <summary>
/// 
/// </summary>
public static class RestClient_Extensions
{
    private static Method GetMethod(HttpVerb verb)
    {
        if (verb == HttpVerb.POST) return Method.POST;
        if (verb == HttpVerb.PUT) return Method.PUT;
        if (verb == HttpVerb.DELETE) return Method.DELETE;
        return Method.GET;
    }
    private static RestRequest GetRequest(this RestClient client, CompareInstance env, CompareRequest req, CompareUser user)
    {
        client.BaseUrl = new Uri($"{env.BaseUrl}{user.GetMergedString(req.Path)}");
        RestRequest request = new(GetMethod(req.RequestMethod));
        request.AddHeader("Authorization", $"bearer {(req.RequiresClientToken ? env.ClientToken : env.UserToken)}");
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Accept", "*/*");
        foreach (var header in req.Headers)
        {
            if (header.Key.CaseInsensitiveContains("Authorization")) continue;
            if (header.Key.CaseInsensitiveContains("Content-Type")) continue;
            if (header.Key.CaseInsensitiveContains("Accept")) continue;
            request.AddHeader(header.Key, header.Value);
        }
        if (req.RequestMethod != HttpVerb.GET)
        {
            string reqBody = req?.BodyTemplate;
            if (req?.Body?.Raw is not null)
            {
                reqBody = req.Body.Raw;
            }
            reqBody = user.GetMergedString(reqBody);
            request.AddParameter("application/json", reqBody, ParameterType.RequestBody);
        }
        return request;
    }

    private static CompareResult GetResult(IRestResponse response, CompareInstance env, CompareRequest req, CompareUser user, long elapsedMilliseconds)
    {
        string? shortPath = response?.ResponseUri?.LocalPath;
        if (string.IsNullOrEmpty(shortPath)) shortPath = req.Path;
        return new CompareResult()
        {
            UserName = user.UserName,
            SessionId = env.SessionId,
            Instance = env.Name,
            Verb = req.RequestMethod.ToString(),
            Request = shortPath,
            Success = response?.IsSuccessful??default,
            ResultCode = response == null ? string.Empty : ((int)response.StatusCode).ToString(),
            StatusDescription = response?.StatusDescription,
            Hash = response==null?0:response.Content.GetDeterministicHashCode(),
            Duration = elapsedMilliseconds,
            LastRunDate = DateTime.Now.ToShortTimeString(),
            Content = response?.Content
        };
    }

    /// <summary>
    /// GetClientToken
    /// </summary>
    /// <param name="client"></param>
    /// <param name="baseUrl"></param>
    /// <param name="clientId"></param>
    /// <param name="client_secret"></param>
    /// <returns></returns>
    /// <exception cref="AuthenticationException"></exception>
    public static async Task<string> GetClientToken(this RestClient client, string baseUrl, string clientId, string client_secret)
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
    /// 
    /// </summary>
    /// <param name="client"></param>
    /// <param name="env"></param>
    /// <param name="req"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public static CompareResult GetResponse(this RestClient client, CompareInstance env, CompareRequest req, CompareUser user)
    {
        Stopwatch stopw = new();
        stopw.Start();
        var response = client.Execute(client.GetRequest(env, req, user));
        stopw.Stop();
        Console.WriteLine($"{(int)response.StatusCode} IN:{stopw.ElapsedMilliseconds,7:n0}  FOR: {req.RequestMethod}-{env.BaseUrl}{user.GetMergedString(req.Path)}");
        return GetResult(response, env, req, user, stopw.ElapsedMilliseconds);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="client"></param>
    /// <param name="userName"></param>
    /// <param name="userPassword"></param>
    /// <param name="baseUrl"></param>
    /// <param name="client_id"></param>
    /// <param name="client_secret"></param>
    /// <returns></returns>
    /// <exception cref="AuthenticationException"></exception>
    public static async Task<string> GetUserToken(this RestClient client, string userName, string userPassword, string baseUrl, string client_id, string client_secret)
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
