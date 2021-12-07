namespace RESTRunner.Domain.Extensions;

/// <summary>
/// CompareRunner_Extensions
/// </summary>
public static class CompareRunner_Extensions
{
    /// <summary>
    /// Save to File
    /// </summary>
    /// <param name="runner"></param>
    public static void SaveToFile(this CompareRunner runner)
    {
        string dirPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        try
        {
            DataContractJsonSerializer js = new(typeof(CompareRunner));
            MemoryStream msObj = new();
            js.WriteObject(msObj, runner);
            msObj.Position = 0;
            StreamReader sr = new(msObj);
            string json = sr.ReadToEnd();
            sr.Close();
            msObj.Close();
            File.WriteAllText(Path.Combine(dirPath, "CompareRunner.json"), json);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
