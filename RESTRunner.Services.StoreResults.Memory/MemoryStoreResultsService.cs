using RESTRunner.Domain.Interfaces;
using RESTRunner.Domain.Models;

namespace RESTRunner.Services.StoreResults.Memory;

/// <summary>
/// Store Results Service
/// </summary>
public class MemoryStoreResultsService : IStoreResults
{
    private readonly List<CompareResult> results = new();
    /// <summary>
    /// Add Result to Store Result Service
    /// </summary>
    /// <param name="compareResults"></param>
    public void Add(CompareResult compareResults)
    {
        results.Add(compareResults);
    }
    /// <summary>
    /// List of stored results
    /// </summary>
    /// <returns></returns>
    public IEnumerable<CompareResult> Results()
    {
        return results;
    }
}
