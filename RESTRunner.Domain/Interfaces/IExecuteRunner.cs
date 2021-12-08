namespace RESTRunner.Domain.Interfaces;

public interface IExecuteRunner
{
    public Task<IEnumerable<CompareResult>> ExecuteRunnerAsync(IStoreResults storeResults);
}
