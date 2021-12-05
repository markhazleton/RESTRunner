
var myRunner = new CompareRunner(new StoreResultsService());
myRunner.InitializeCompareRunner();
var import = new PostmanImport(myRunner);
import.LoadFromPostman("collection.json");
myRunner.SaveToFile();
var ExecuteRunner = new ExecuteRunnerService(myRunner);
var compareResults = new List<CompareResult>();
compareResults.AddRange(await ExecuteRunner.ExecuteRunnerAsync().ConfigureAwait(false));
compareResults.ExportToCsv();
