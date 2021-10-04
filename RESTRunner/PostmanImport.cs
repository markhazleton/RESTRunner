using Newtonsoft.Json;
using RESTRunner.Domain.Models;
using RESTRunner.Postman.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace RESTRunner
{
    public class PostmanImport
    {
        private readonly CompareRunner myRunner;

        public PostmanImport(CompareRunner myRunner)
        {
            this.myRunner = myRunner;
        }

        private static CompareBody Create(Body body)
        {
            if (body == null) return null;
            var cbody = new CompareBody
            {
                Properties = Create(body.Urlencoded)
            };
            return cbody;
        }

        private static List<CompareProperty> Create(List<Urlencoded> encodeList)
        {
            var list = new List<CompareProperty>();
            if (encodeList != null)
                foreach (var encode in encodeList)
                {
                    list.Add(new CompareProperty(key: encode.Key, value: encode.Value, type: encode.Type, name: encode.Description, description: encode.Description));
                }
            return list;
        }

        private static IEnumerable<CompareProperty> Create(List<Header> header)
        {
            var list = new List<CompareProperty>();
            foreach (var headerItem in header)
            {
                list.Add(new CompareProperty(headerItem.Key, headerItem.Value, headerItem.Type, headerItem.Name));
            }
            return list;
        }

        private static CompareRequest GetCompareRequestFromRequest(Request request)
        {
            var req = new CompareRequest
            {
                Path = request?.Url?.Raw?.Replace("{{url}}/", String.Empty),
                BodyTemplate = string.Empty,
                Body = Create(request.Body),
            };

            if (request.Method == "GET")
                req.RequestMethod = HttpVerb.GET;

            if (request.Method == "POST")
                req.RequestMethod = HttpVerb.POST;

            if (request.Method == "PUT")
                req.RequestMethod = HttpVerb.PUT;

            
            req.Headers.AddRange(Create(request.Header));
            return req;
        }

        public static CompareRequest GetRequest(Request request)
        {
            if (request is null) return null;
            return GetCompareRequestFromRequest(request);
        }
        public static IEnumerable<CompareRequest> GetRequests(IEnumerable<Request> requests)
        {
            var list = new List<CompareRequest>();
            if (requests is null) return list;
            foreach (var request in requests)
            {
                list.Add(GetCompareRequestFromRequest(request));
            }
            return list;
        }
        public void LoadFromPostman(string CollectionJSONFile)
        {
            var jsonText = File.ReadAllText(CollectionJSONFile);
            var myDeserializedClass = JsonConvert.DeserializeObject<Root>(jsonText);
            foreach (var item1 in myDeserializedClass.Item)
            {
                LookForRequests(item1);
            }
        }
        private void LookForRequests(PostmanItem ParentItem)
        {
            if (ParentItem is null) return;

            if (ParentItem.Item is null) return;

            foreach (var childItem in ParentItem.Item)
            {
                if (childItem.Request is not null) myRunner.Requests.Add(GetRequest(childItem.Request));

                LookForRequests(childItem);
            }
        }
    }
}
