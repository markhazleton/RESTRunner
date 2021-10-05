using System.Collections.Generic;

namespace RESTRunner.Domain.Models
{
    public class CompareBody
    {
        public string Mode;
        public List<CompareProperty> Properties;
        public string Raw;
        public string Language;
    }
}
