namespace RESTRunner.Domain.Models
{
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
