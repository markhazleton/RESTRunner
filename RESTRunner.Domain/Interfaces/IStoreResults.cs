
namespace RESTRunner.Domain.Interfaces;

/// <summary>
/// Store results Interface
/// </summary>
public interface IStoreResults
{
    /// <summary>
    /// Add a result to our list of results
    /// </summary>
    /// <param name="compareResults"></param>
    public void Add(CompareResult compareResults);
    /// <summary>
    /// The current list of stored results
    /// </summary>
    /// <returns></returns>
    public IEnumerable<CompareResult> Results();
}
