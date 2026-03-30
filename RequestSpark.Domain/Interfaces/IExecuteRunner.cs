namespace RequestSpark.Domain.Interfaces;

/// <summary>
/// Interface for executing RequestSpark operations
/// </summary>
public interface IExecuteRunner
{
    /// <summary>
    /// Executes RequestSpark asynchronously with the specified output handler
    /// </summary>
    /// <param name="output">The output handler to write results to</param>
    /// <param name="ct">Cancellation token to cancel the operation</param>
    /// <returns>A task representing the asynchronous operation with execution statistics</returns>
    Task<ExecutionStatistics> ExecuteRunnerAsync(IOutput output, CancellationToken ct = default);
}

