using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
    services.AddTransient<ExecuteRunnerService>();
}).UseConsoleLifetime();


var host = builder.Build();

using (var serviceScope = host.Services.CreateScope())
{
    var services = serviceScope.ServiceProvider;
    try
    {
        var myService = services.GetRequiredService<ExecuteRunnerService>();
        await myService.ExecuteRunnerAsync(new CsvOutput()).ConfigureAwait(false);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error Occured:{ex.Message}");
    }
}
return 0;
