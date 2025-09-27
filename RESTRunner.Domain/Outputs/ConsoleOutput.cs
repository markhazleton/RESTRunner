namespace RESTRunner.Domain.Outputs;

/// <summary>
/// Console implementation of IOutput for writing results to the console
/// </summary>
public class ConsoleOutput : IOutput
{
    /// <summary>
    /// Writes error information to the console output
    /// </summary>
    /// <param name="result">The comparison result containing error information</param>
    public void WriteError(CompareResult result)
    {
        ConsoleHelper.WriteError(result.ToString());
    }

    /// <summary>
    /// Writes informational output to the console
    /// </summary>
    /// <param name="result">The comparison result to display</param>
    public void WriteInfo(CompareResult result)
    {
        WriteInfo(new string[] { result.ToString() });
    }

    /// <summary>
    /// Writes multiple lines of informational output to the console
    /// </summary>
    /// <param name="info">Array of information strings to display</param>
    public void WriteInfo(string[] info)
    {
        foreach (string line in info) 
            Console.WriteLine(line);
    }
}
