var myRunner = new CompareRunner();
myRunner.InitializeCompareRunner();
var import = new PostmanImport(myRunner);
import.LoadFromPostman("collection.json");
myRunner.SaveToFile();
var ExecuteRunner = new ExecuteRunnerService(myRunner);
await ExecuteRunner.ExecuteRunnerAsync(new CsvOutput()).ConfigureAwait(false);

