using System.Collections.Generic;

namespace RESTRunner.Domain.Models
{
    public class CompareRequest
    {
        public string Path;
        public HttpVerb RequestMethod;
        public string BodyTemplate;
        public bool RequiresClientToken;
        public List<CompareProperty> Headers = new();
        public CompareBody Body = new();
    }
}
