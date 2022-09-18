
namespace RESTRunner.Domain.Interfaces;

/// <summary>
/// 
/// </summary>
public interface IExecuteRunner
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Task ExecuteRunnerAsync(IOutput output);
}
