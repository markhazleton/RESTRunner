
namespace RESTRunner.Postman;

/// <summary>
/// Import Postman Collection into REST Runner model
/// </summary>
public class PostmanImport
{
    private readonly CompareRunner myRunner;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="myRunner"></param>
    public PostmanImport(CompareRunner myRunner)
    {
        this.myRunner = myRunner;
    }

    private static CompareBody? Create(Body? body)
    {
        if (body == null) return null;
        var cbody = new CompareBody() { Mode = "raw" };

        if (string.Compare(body.Mode, "raw", StringComparison.Ordinal) == 0)
        {
            cbody.Raw = body.Raw;
        }
        else
        {
            cbody.Properties = Create(body.Urlencoded);
            cbody.Raw = JsonConvert.SerializeObject(cbody.Properties.Select(s => new { key = s.Key, value = s.Value }));
        }
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

    private static IEnumerable<CompareProperty> Create(List<Header>? header)
    {
        var list = new List<CompareProperty>();
        foreach (var headerItem in header ?? new List<Header>())
        {
            list.Add(new CompareProperty(headerItem.Key, headerItem.Value, headerItem.Type, headerItem.Name));
        }
        return list;
    }

    private static CompareRequest GetCompareRequestFromRequest(Request request)
    {
        if (request == null) return new CompareRequest();

        var req = new CompareRequest
        {
            Path = request?.Url?.Raw?.Replace("{{url}}/", String.Empty).Replace("{{api-url}}/", String.Empty),
            BodyTemplate = string.Empty,
            Body = Create(request?.Body),
        };

        if (string.Compare(request?.Method, "GET", StringComparison.Ordinal) == 0)
            req.RequestMethod = HttpVerb.GET;

        if (string.Compare(request?.Method, "POST", StringComparison.Ordinal) == 0)
            req.RequestMethod = HttpVerb.POST;

        if (string.Compare(request?.Method, "PUT", StringComparison.Ordinal) == 0)
            req.RequestMethod = HttpVerb.PUT;

        if (string.Compare(request?.Method, "DELETE", StringComparison.Ordinal) == 0)
            req.RequestMethod = HttpVerb.DELETE;


        req.Headers.AddRange(Create(request?.Header));
        return req;
    }
    private void LookForRequests(PostmanItem ParentItem)
    {
        if (ParentItem is null) return;

        if (ParentItem.Request is not null) myRunner.Requests.Add(GetRequest(ParentItem.Request));

        if (ParentItem.Item is null) return;

        foreach (var childItem in ParentItem.Item)
        {
            LookForRequests(childItem);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static CompareRequest GetRequest(Request request)
    {
        if (request is null) return new CompareRequest();
        return GetCompareRequestFromRequest(request);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="requests"></param>
    /// <returns></returns>
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
    /// <summary>
    /// 
    /// </summary>
    /// <param name="CollectionJSONFile"></param>
    public void LoadFromPostman(string CollectionJSONFile)
    {
        var jsonText = File.ReadAllText(CollectionJSONFile);
        foreach (var item in JsonConvert.DeserializeObject<Root>(jsonText)?.Item ?? Enumerable.Empty<PostmanItem>())
        {
            LookForRequests(item);
        }
    }
}
