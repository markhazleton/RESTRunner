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

    public class CompareBody
    {
        public string Mode;
        public List<CompareProperty> Properties;
        public string Raw;
        public string Language;
    }


    public class CompareProperty
    {
        public string Key;
        public string Value;
        public string Type;
        public string Name;
        public string Description;

        public CompareProperty(string key, string value, string type, string name, string description = null)
        {
            Key = key;
            Value = value;
            Type = type;
            Name = name;
            Description = description ?? string.Empty;
        }
    }
}
