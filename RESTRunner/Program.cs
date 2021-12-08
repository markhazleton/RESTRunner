var storeResults = new MemoryStoreResultsService();

var myRunner = new CompareRunner();
myRunner.InitializeCompareRunner();
var import = new PostmanImport(myRunner);
import.LoadFromPostman("collection.json");
myRunner.SaveToFile();
var ExecuteRunner = new ExecuteRunnerService(myRunner);
var compareResults = new List<CompareResult>();
compareResults.AddRange(await ExecuteRunner.ExecuteRunnerAsync(storeResults).ConfigureAwait(false));
compareResults.ExportToCsv();
