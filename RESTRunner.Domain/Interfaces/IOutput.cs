namespace RESTRunner.Domain.Interfaces;
/// <summary>
/// 
/// </summary>
public interface IOutput
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="responseModel"></param>
    void WriteError(CompareResult responseModel);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="responseModel"></param>
    void WriteInfo(CompareResult responseModel);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="InfoString"></param>
    void WriteInfo(string[] InfoString);
}