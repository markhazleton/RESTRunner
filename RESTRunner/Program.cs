using RESTRunner.Domain.Extensions;
using RESTRunner.Domain.Models;
using RESTRunner.Extensions;
using RESTRunner.Infrastructure;
using RESTRunner.Postman;
using RESTRunner.Services;
using RESTRunner.Services.StoreResults.Memory;
using System.Collections.Generic;
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
            myRunner.SaveToFile();
            var ExecuteRunner = new ExecuteRunnerService(myRunner);
            var compareResults = new List<CompareResult>();
            compareResults.AddRange(await ExecuteRunner.ExecuteRunnerAsync().ConfigureAwait(false));
            compareResults.ExportToCsv();
        }
    }
}
