namespace RESTRunner.Domain.Interfaces;

/// <summary>
/// Interface for executing REST runner operations
/// </summary>
public interface IExecuteRunner
{
    /// <summary>
    /// Executes the REST runner asynchronously with the specified output handler
    /// </summary>
    /// <param name="output">The output handler to write results to</param>
    /// <param name="ct">Cancellation token to cancel the operation</param>
    /// <returns>A task representing the asynchronous operation with execution statistics</returns>
    Task<ExecutionStatistics> ExecuteRunnerAsync(IOutput output, CancellationToken ct = default);
}
