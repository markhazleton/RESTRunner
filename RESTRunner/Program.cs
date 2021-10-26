using FileHelpers;
using Newtonsoft.Json;
using RESTRunner.Domain.Models;
using RESTRunner.Extensions;
using RESTRunner.Postman;
using RESTRunner.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RESTRunner
{
    public class Program
    {
        static async Task Main()
        {

            var myRunner = new CompareRunner(new StoreResultsService());
            myRunner.InitializeCompareRunner();
            var import = new PostmanImport(myRunner);
            import.LoadFromPostman("collection.json");

            var ExecuteRunner = new ExecuteRunnerService(myRunner);
            var results = await ExecuteRunner.ExecuteRunnerAsync();

            var myResults = JsonConvert.SerializeObject(results);

            foreach (var result in results.ToList().OrderBy(o => o.Instance).ThenBy(o => o.Request))
            {
                Console.WriteLine(result.ToString());
            }

            WriteResultsToCSV(results);

        }

        private static void WriteResultsToCSV(IEnumerable<CompareResults> results)
        {
            var engine = new FileHelperEngine<CompareResults>();
            engine.HeaderText = engine.GetFileHeader();
            string dirPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            try
            {
                //File location, where the .csv goes and gets stored.
                string filePath = Path.Combine(dirPath, "RESTRunner" + ".csv");
                engine.WriteFile(filePath, results);
            }
            catch
            {
            }
        }
    }
}
