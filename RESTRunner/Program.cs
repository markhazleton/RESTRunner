using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RESTRunner.Domain.Interfaces;
using RESTRunner.Services.HttpClientRunner;

var builder = new HostBuilder()
.ConfigureServices((hostContext, services) =>
{
    services.AddLogging(configure => 
    {
        configure.AddConsole();
        configure.SetMinimumLevel(LogLevel.Warning);
    });
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
        Console.WriteLine("Starting REST Runner execution...");
        Console.WriteLine($"Start Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        
        var myService = services.GetRequiredService<IExecuteRunner>();
        var statistics = await myService.ExecuteRunnerAsync(new CsvOutput($"c:\\test\\RESTRunner.csv")).ConfigureAwait(false);
        
        // Display comprehensive statistics
        DisplayExecutionStatistics(statistics);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n❌ Error Occurred: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
        }
        return 1;
    }
}

Console.WriteLine("\n✅ REST Runner execution completed successfully!");
return 0;

static void DisplayExecutionStatistics(ExecutionStatistics statistics)
{
    Console.WriteLine("\n" + new string('=', 80));
    Console.WriteLine("🚀 REST RUNNER EXECUTION STATISTICS");
    Console.WriteLine(new string('=', 80));
    
    // Overall Summary
    Console.WriteLine("\n📊 OVERALL SUMMARY");
    Console.WriteLine(new string('-', 40));
    Console.WriteLine($"{"Total Requests:",-25} {statistics.TotalRequests:N0}");
    Console.WriteLine($"{"Successful Requests:",-25} {statistics.SuccessfulRequests:N0} ({statistics.SuccessRate:F2}%)");
    Console.WriteLine($"{"Failed Requests:",-25} {statistics.FailedRequests:N0} ({(100 - statistics.SuccessRate):F2}%)");
    Console.WriteLine($"{"Start Time:",-25} {statistics.StartTime:yyyy-MM-dd HH:mm:ss} UTC");
    Console.WriteLine($"{"End Time:",-25} {statistics.EndTime:yyyy-MM-dd HH:mm:ss} UTC");
    Console.WriteLine($"{"Total Duration:",-25} {statistics.TotalDuration:hh\\:mm\\:ss}");
    Console.WriteLine($"{"Requests per Second:",-25} {statistics.RequestsPerSecond:F2}");
    
    // Performance Metrics
    Console.WriteLine("\n⚡ PERFORMANCE METRICS");
    Console.WriteLine(new string('-', 40));
    Console.WriteLine($"{"Average Response Time:",-25} {statistics.AverageResponseTime:F2} ms");
    Console.WriteLine($"{"Minimum Response Time:",-25} {statistics.MinResponseTime:N0} ms");
    Console.WriteLine($"{"Maximum Response Time:",-25} {statistics.MaxResponseTime:N0} ms");
    
    // Response Time Percentiles
    Console.WriteLine("\n📈 RESPONSE TIME PERCENTILES");
    Console.WriteLine(new string('-', 40));
    try
    {
        Console.WriteLine($"{"50th Percentile (P50):",-25} {statistics.GetResponseTimePercentile(50):N0} ms");
        Console.WriteLine($"{"75th Percentile (P75):",-25} {statistics.GetResponseTimePercentile(75):N0} ms");
        Console.WriteLine($"{"90th Percentile (P90):",-25} {statistics.GetResponseTimePercentile(90):N0} ms");
        Console.WriteLine($"{"95th Percentile (P95):",-25} {statistics.GetResponseTimePercentile(95):N0} ms");
        Console.WriteLine($"{"99th Percentile (P99):",-25} {statistics.GetResponseTimePercentile(99):N0} ms");
        Console.WriteLine($"{"99.9th Percentile:",-25} {statistics.GetResponseTimePercentile(99.9):N0} ms");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error calculating percentiles: {ex.Message}");
    }
    
    // Requests by HTTP Method
    if (statistics.RequestsByMethod.Any())
    {
        Console.WriteLine("\n🔧 REQUESTS BY HTTP METHOD");
        Console.WriteLine(new string('-', 40));
        var totalRequests = statistics.RequestsByMethod.Values.Sum();
        foreach (var method in statistics.RequestsByMethod.OrderByDescending(x => x.Value))
        {
            var percentage = totalRequests > 0 ? (double)method.Value / totalRequests * 100 : 0;
            Console.WriteLine($"{method.Key,-10} {method.Value,8:N0} ({percentage,5:F1}%)");
        }
    }
    
    // Requests by Status Code
    if (statistics.RequestsByStatusCode.Any())
    {
        Console.WriteLine("\n📋 REQUESTS BY STATUS CODE");
        Console.WriteLine(new string('-', 40));
        var totalRequests = statistics.RequestsByStatusCode.Values.Sum();
        foreach (var status in statistics.RequestsByStatusCode.OrderBy(x => x.Key))
        {
            var percentage = totalRequests > 0 ? (double)status.Value / totalRequests * 100 : 0;
            var statusIcon = GetStatusIcon(status.Key);
            Console.WriteLine($"{statusIcon} {status.Key,-15} {status.Value,8:N0} ({percentage,5:F1}%)");
        }
    }
    
    // Requests by Instance
    if (statistics.RequestsByInstance.Any())
    {
        Console.WriteLine("\n🏢 REQUESTS BY INSTANCE");
        Console.WriteLine(new string('-', 40));
        var totalRequests = statistics.RequestsByInstance.Values.Sum();
        foreach (var instance in statistics.RequestsByInstance.OrderByDescending(x => x.Value))
        {
            var percentage = totalRequests > 0 ? (double)instance.Value / totalRequests * 100 : 0;
            Console.WriteLine($"{instance.Key,-20} {instance.Value,8:N0} ({percentage,5:F1}%)");
        }
    }
    
    // Requests by User
    if (statistics.RequestsByUser.Any())
    {
        Console.WriteLine("\n👥 REQUESTS BY USER");
        Console.WriteLine(new string('-', 40));
        var totalRequests = statistics.RequestsByUser.Values.Sum();
        foreach (var user in statistics.RequestsByUser.OrderByDescending(x => x.Value))
        {
            var percentage = totalRequests > 0 ? (double)user.Value / totalRequests * 100 : 0;
            Console.WriteLine($"{user.Key,-20} {user.Value,8:N0} ({percentage,5:F1}%)");
        }
    }
    
    // Performance Summary
    Console.WriteLine("\n🎯 PERFORMANCE SUMMARY");
    Console.WriteLine(new string('-', 80));
    
    if (statistics.SuccessRate >= 99.0)
        Console.WriteLine("✅ Excellent: Success rate is above 99%");
    else if (statistics.SuccessRate >= 95.0)
        Console.WriteLine("🟡 Good: Success rate is above 95%");
    else if (statistics.SuccessRate >= 90.0)
        Console.WriteLine("🟠 Warning: Success rate is below 95%");
    else
        Console.WriteLine("🔴 Critical: Success rate is below 90%");
    
    if (statistics.AverageResponseTime <= 100)
        Console.WriteLine("✅ Excellent: Average response time is under 100ms");
    else if (statistics.AverageResponseTime <= 500)
        Console.WriteLine("🟡 Good: Average response time is under 500ms");
    else if (statistics.AverageResponseTime <= 1000)
        Console.WriteLine("🟠 Warning: Average response time is above 500ms");
    else
        Console.WriteLine("🔴 Critical: Average response time is above 1000ms");
    
    if (statistics.RequestsPerSecond >= 100)
        Console.WriteLine("✅ Excellent: Processing over 100 requests per second");
    else if (statistics.RequestsPerSecond >= 50)
        Console.WriteLine("🟡 Good: Processing over 50 requests per second");
    else if (statistics.RequestsPerSecond >= 10)
        Console.WriteLine("🟠 Moderate: Processing over 10 requests per second");
    else
        Console.WriteLine("🔴 Low: Processing fewer than 10 requests per second");
    
    Console.WriteLine(new string('=', 80));
    Console.WriteLine($"📄 Results exported to: c:\\test\\RESTRunner.csv");
    Console.WriteLine($"⏰ Execution completed at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    Console.WriteLine(new string('=', 80));
}

static string GetStatusIcon(string statusCode)
{
    return statusCode[0] switch
    {
        '2' => "✅", // 2xx Success
        '3' => "🔄", // 3xx Redirection
        '4' => "⚠️",  // 4xx Client Error
        '5' => "❌", // 5xx Server Error
        _ => "❓"    // Unknown/Error
    };
}
