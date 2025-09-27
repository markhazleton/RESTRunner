namespace RESTRunner.Domain.Interfaces;

/// <summary>
/// Interface for handling output operations in the REST runner
/// </summary>
public interface IOutput
{
    /// <summary>
    /// Writes error information for a comparison result
    /// </summary>
    /// <param name="responseModel">The comparison result containing error information</param>
    void WriteError(CompareResult responseModel);
    
    /// <summary>
    /// Writes informational output for a comparison result
    /// </summary>
    /// <param name="responseModel">The comparison result containing information to display</param>
    void WriteInfo(CompareResult responseModel);
    
    /// <summary>
    /// Writes informational output as an array of strings
    /// </summary>
    /// <param name="infoString">Array of information strings to display</param>
    void WriteInfo(string[] infoString);
}