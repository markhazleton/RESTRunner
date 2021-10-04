using RESTRunner.Domain.Models;
using System.Collections.Generic;

namespace RESTRunner.Domain.Interfaces
{
    public interface IStoreResults
    {
        public void Add(CompareResults compareResults);
        public IEnumerable<CompareResults> Results();
    }
}
