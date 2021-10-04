using RESTRunner.Domain.Interfaces;
using RESTRunner.Domain.Models;
using System.Collections.Generic;

namespace RESTRunner.Services
{
    public class StoreResultsService : IStoreResults
    {
        private readonly List<CompareResults> results = new();
        public void Add(CompareResults compareResults)
        {
            results.Add(compareResults);
        }
        public IEnumerable<CompareResults> Results()
        {
            return results;
        }
    }
}
