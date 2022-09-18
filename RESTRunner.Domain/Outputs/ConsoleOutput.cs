
namespace RESTRunner.Domain.Outputs;

/// <summary>
/// 
/// </summary>
public class ConsoleOutput : IOutput
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="result"></param>
    public void WriteError(CompareResult result)
    {
        ConsoleHelper.WriteError(result.ToString());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="result"></param>
    public void WriteInfo(CompareResult result)
    {
        WriteInfo(new string[] { result.ToString() });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Info"></param>
    public void WriteInfo(String[] Info)
    {
        foreach (string line in Info) Console.WriteLine(line);
    }
}
/// <summary>
/// 
/// </summary>
public static class ConsoleHelper
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="text"></param>
    public static void WriteError(string text)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(text);
        Console.ResetColor();
    }
}
