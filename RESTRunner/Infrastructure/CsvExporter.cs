using CsvHelper;
using CsvHelper.Configuration;
using RESTRunner.Domain.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace RESTRunner.Infrastructure
{
    public class ExportFileVM
    {
        public string ExportFileName;
        public string ContentType;
        public byte[] Data;
    }
    public static class CsvExporter
    {
        public static void ExportToCsv(this List<CompareResult> list)
        {
            string dirPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            string filePath = Path.Combine(dirPath, "RESTRunner" + ".csv");

            using var streamWriter = new StreamWriter(filePath);
            CsvConfiguration config = new CsvConfiguration(CultureInfo.CurrentCulture);
            using var csvWriter = new CsvWriter(streamWriter, config);
            csvWriter.WriteRecords(list);
        }
    }
}
