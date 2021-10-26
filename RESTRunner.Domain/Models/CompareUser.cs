using System.Collections.Generic;

namespace RESTRunner.Domain.Models
{
    public class CompareUser
    {
        public CompareUser()
        {
            Properties = new Dictionary<string, string>();
        }
        public string UserName;
        public string Password;
        public Dictionary<string, string> Properties;
    }
}
