# RESTRunner AI Coding Instructions

## Project Overview
RESTRunner is a .NET 9.0 API testing, performance benchmarking, and regression testing solution that imports Postman collections and executes them against multiple instances with detailed statistics. The solution includes both a console application for batch testing and a web application (Razor Pages + minimal APIs) for interactive testing.

## Solution Architecture

### Project Structure & Dependencies
```
RESTRunner.Domain (core models, no external deps)
  ↓
RESTRunner.Services.HttpClientRunner (execution engine)
  ↓
RESTRunner (console app) + RESTRunner.Web (web app)
  ↓
RESTRunner.PostmanImport (Postman collection parser)
```

**Key architectural decisions:**
- **Domain-first design**: `RESTRunner.Domain` contains all domain models and is referenced by all other projects
- **Service layer pattern**: `IExecuteRunner` in Domain, implemented in `ExecuteRunnerService` (Services.HttpClientRunner)
- **Dual UI**: Console app for CI/CD, web app for interactive use
- **File-based storage**: Web app uses JSON files in `~/Data` for configurations/collections (no database)

### Core Domain Models
- `CompareRunner`: Root configuration object containing `Instances`, `Users`, and `Requests`
- `CompareInstance`: Target API endpoint (e.g., Local, Demo, Production)
- `CompareRequest`: API request definition with path, method, body template
- `CompareUser`: User context with properties for request template substitution
- `CompareResult`: Individual execution result with timing, status, hash
- `ExecutionStatistics`: Thread-safe statistics collector with percentile calculations

### Request Execution Flow
1. Console: `Program.cs` → `ExecuteRunnerService.ExecuteRunnerAsync()`
2. Web: `RunnerController` → `IExecutionService` → `ExecuteRunnerService`
3. Parallel execution: 100 iterations × instances × users × requests (default 10 concurrent)
4. Template substitution: `{{encoded_user_name}}` and `{{property_key}}` replaced from `CompareUser.Properties`
5. Results output: CSV via `IOutput` interface (`CsvOutput`, `ConsoleOutput`)

## Critical Conventions

### Global Usings Pattern
All projects use `GlobalUsings.cs` for project-wide imports:
```csharp
// RESTRunner.Domain/GlobalUsings.cs
global using RESTRunner.Domain.Constants;
global using RESTRunner.Domain.Models;
global using System.Runtime.Serialization;
```
**Rule**: Add new domain types to GlobalUsings instead of per-file using statements.

### Primary Constructor Pattern (.NET 9)
Service classes use primary constructors with dependency injection:
```csharp
public class ExecuteRunnerService(
    CompareRunner compareRunner,
    IHttpClientFactory HttpClientFactory,
    ILogger<ExecuteRunnerService> logger) : IExecuteRunner
{
    private readonly HttpClient client = HttpClientFactory.CreateClient();
    // ...
}
```

### Thread-Safe Statistics
`ExecutionStatistics` uses `Interlocked` operations and `ConcurrentBag/ConcurrentDictionary`:
```csharp
public void IncrementTotalRequests() => Interlocked.Increment(ref _totalRequests);
stats.RequestsByMethod.AddOrUpdate(methodName, 1, (_, count) => count + 1);
```

### Template Substitution
Requests support runtime property replacement:
```csharp
req.Path = req.Path.Replace("{{encoded_user_name}}", user.UserName);
foreach (var prop in user.Properties)
{
    req.BodyTemplate = req.BodyTemplate.Replace($"{{{prop.Key}}}", prop.Value);
}
```
**Example**: `"path": "api/users/{{user_id}}"` + `user.Properties["user_id"] = "123"` → `"api/users/123"`

## Development Workflows

### Building & Running
```powershell
# Build entire solution
dotnet build

# Run console app (requires collection.json in RESTRunner/)
cd RESTRunner
dotnet run

# Run web app (https://localhost:7001)
cd RESTRunner.Web
dotnet run

# Run tests
dotnet test
```

### Testing Patterns
- MSTest framework (`[TestClass]`, `[TestMethod]`)
- Test files named `*Tests.cs` in `RESTRunner.Domain.Tests`
- Basic structure: Arrange → Act → Assert
```csharp
[TestMethod]
public void CompareRunner_GetTotalTestCount_ReturnsCorrectValue()
{
    var runner = new CompareRunner
    {
        Instances = new List<CompareInstance> { new() { Name = "Test" } },
        Requests = new List<CompareRequest> { new() { Path = "/api/test" } },
        Users = new List<CompareUser> { new() { UserName = "user1" } }
    };
    
    Assert.AreEqual(1, runner.GetTotalTestCount()); // 1×1×1
}
```

### Postman Import
- Place `collection.json` in `RESTRunner/` directory (auto-copied to output)
- Uses Postman v2.1.0 format
- URL template: `{{url}}/path` or `{{api-url}}/path` → converted to relative path
- Import process: `PostmanImport.LoadFromPostman()` → populates `CompareRunner.Requests`

## Web Application Specifics

### Service Registration (Program.cs)
```csharp
// RESTRunner services
builder.Services.AddSingleton<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IConfigurationService, FileConfigurationService>();
builder.Services.AddScoped<ICollectionService, FileCollectionService>();
builder.Services.AddScoped<IExecutionService, SimpleExecutionService>();
```

### Minimal API Endpoints
Defined in `RESTRunner.Web/Program.cs`:
- `/api/employees/*` - Sample CRUD operations
- `/api/departments/*` - Department management
- `/api/status` - Health check
- `/docs` - Swagger UI (dev only)
- `.WithTags("TagName")` groups endpoints in Swagger

### File Storage Conventions
- Configurations: `~/Data/configurations/{guid}.json`
- Collections: `~/Data/collections/{guid}.json`
- Results: `~/Data/results/{guid}.csv`
- Use `IFileStorageService` for all file operations

### Configuration Model
`TestConfiguration` wraps `CompareRunner` with metadata:
```csharp
public class TestConfiguration
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Iterations { get; set; } = 100;
    public int MaxConcurrency { get; set; } = 10;
    public CompareRunner Runner { get; set; }
    public List<string> Tags { get; set; }
}
```

## HTTP Verbs & Request Methods
Supports all standard HTTP methods via `HttpVerb` enum:
- GET, POST, PUT, DELETE, PATCH (common)
- HEAD, OPTIONS, MERGE, COPY (less common)
- Switch statement in `ExecuteRunnerService.GetResponseAsync()` handles each verb

## Execution Statistics Display
Console app displays comprehensive execution report with emoji indicators:
- ✅ 2xx Success, ⚠️ 4xx Client Error, ❌ 5xx Server Error
- Performance metrics: P50/P75/P90/P95/P99/P99.9 percentiles
- Breakdown by: HTTP method, status code, instance, user
- Results exported to `c:\test\RESTRunner.csv`

## Version & Build Configuration
All projects use deterministic versioning in `.csproj`:
```xml
<AssemblyVersion>6.$([System.DateTime]::UtcNow.ToString(yyMM)).$([System.DateTime]::UtcNow.ToString(ddHH)).$([System.DateTime]::UtcNow.ToString(mmss))</AssemblyVersion>
```
- Format: `6.YYMM.DDHH.MMSS`
- .NET SDK version locked in `global.json`: `"version": "9.0.305"`

## Common Tasks

### Adding a New Request Property
1. Add property to `CompareRequest` (RESTRunner.Domain/Models)
2. Update JSON serialization if needed
3. Handle in `ExecuteRunnerService.GetResponseAsync()` template substitution
4. Update Postman import if mapping from collection

### Adding a New Output Format
1. Create class implementing `IOutput` (RESTRunner.Domain/Interfaces)
2. Implement `WriteInfo()`, `WriteError()`, `WriteHeader()` methods
3. Register in DI container (Program.cs)
4. Pass to `ExecuteRunnerAsync(IOutput output)`

### Creating a New Test Configuration
1. Web UI: `/Configuration/Create`
2. Define instances (base URLs), users (with properties), requests (paths/methods)
3. Set iterations (1-1000) and concurrency (1-100)
4. Total tests = instances × users × requests × iterations
5. Saved as JSON in `~/Data/configurations/`

## External Dependencies
- **CsvHelper**: CSV result export
- **Newtonsoft.Json**: Postman collection parsing
- **WebSpark.Bootswatch**: Web UI theme switching
- **WebSpark.HttpClientUtility**: HTTP request decorators
- **Swashbuckle.AspNetCore**: Swagger/OpenAPI documentation

## Troubleshooting Tips
- **Console app fails**: Check `collection.json` exists and is valid Postman v2.1.0 format
- **Web app 404**: Ensure file storage initialized (`InitializeSampleDataAsync()` in Program.cs)
- **Thread safety issues**: Use `Interlocked` or `Concurrent*` collections for shared state
- **Template substitution not working**: Verify property keys match exactly (case-sensitive)
- **High memory usage**: Reduce iterations or concurrency in configuration
