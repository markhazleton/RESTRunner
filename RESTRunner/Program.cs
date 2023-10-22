using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RESTRunner.Domain.Interfaces;

var builder = new HostBuilder()
.ConfigureServices((hostContext, services) =>
{
    services.AddHttpClient();
    services.AddSingleton(serviceProvider =>
    {
        var myRunner = new CompareRunner();
        myRunner.InitializeCompareRunner();
        var import = new PostmanImport(myRunner);
        import.LoadFromPostman("collection.json");
        myRunner.SaveToFile();
        return myRunner;
    });
    services.AddSingleton<IExecuteRunner, ExecuteRunnerService>();
}).UseConsoleLifetime();

var host = builder.Build();
using (var serviceScope = host.Services.CreateScope())
{
    var services = serviceScope.ServiceProvider;
    try
    {
        var myService = services.GetRequiredService<IExecuteRunner>();
        await myService.ExecuteRunnerAsync(new CsvOutput($"c:\\test\\RESTRunner.csv")).ConfigureAwait(false);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error Occurred:{ex.Message}");
    }
}
return 0;
