using RESTRunner.Domain.Models;
using System.Collections.Generic;

namespace RESTRunner.Domain.Interfaces
{
    public interface IStoreResults
    {
        public void Add(CompareResult compareResults);
        public IEnumerable<CompareResult> Results();
    }
}
