using RESTRunner.Domain.Interfaces;
using RESTRunner.Domain.Models;
using System.Collections.Generic;

namespace RESTRunner.Services
{
    public class StoreResultsService : IStoreResults
    {
        private readonly List<CompareResult> results = new();
        public void Add(CompareResult compareResults)
        {
            results.Add(compareResults);
        }
        public IEnumerable<CompareResult> Results()
        {
            return results;
        }
    }
}
