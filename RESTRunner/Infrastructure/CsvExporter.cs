﻿
namespace RESTRunner.Infrastructure;
/// <summary>
/// Extension class to Export CompareResult list to a CSV file
/// </summary>
public static class CsvExporter
{
    /// <summary>
    /// Export CompareResult list to a CSV file
    /// </summary>
    /// <param name="list"></param>
    public static void ExportToCsv(this List<CompareResult> list)
    {
        string dirPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        string filePath = Path.Combine(dirPath, "RESTRunner" + ".csv");

        using var streamWriter = new StreamWriter(filePath);
        CsvConfiguration config = new(CultureInfo.CurrentCulture);
        using var csvWriter = new CsvWriter(streamWriter, config);
        csvWriter.WriteRecords(list);
    }
}
