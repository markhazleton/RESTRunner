namespace RESTRunner.Domain.Models
{
    public class CompareInstance
    {
        public string BaseUrl;
        public string Name;
        public string UserToken;
        public string ClientToken;

        public override string ToString() { return $"{Name}:{BaseUrl}"; }
    }
}
