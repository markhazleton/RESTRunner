namespace RESTRunner.Domain.Outputs;

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
