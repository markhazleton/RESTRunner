using Microsoft.OpenApi.Models; // Add for Swagger
using Microsoft.AspNetCore.Http.Features; // Add for FormOptions
using Microsoft.AspNetCore.Server.IIS; // Add for IISServerOptions
using Microsoft.AspNetCore.Server.Kestrel.Core; // Add for KestrelServerOptions
using System.Text.Json; // Add for JSON serialization
using RESTRunner.Web.SampleCRUD;
using WebSpark.Bootswatch; // Add for Bootswatch
using WebSpark.HttpClientUtility.ClientService;
using WebSpark.HttpClientUtility.RequestResult; // Add for HttpRequestResultService
using WebSpark.HttpClientUtility.StringConverter;
using RESTRunner.Web.Services; // Add for our new services
using RESTRunner.Domain.Interfaces;
using RESTRunner.Services.HttpClientRunner;
using RESTRunner.Domain.Models;

var builder = WebApplication.CreateBuilder(args);

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RESTRunner API", Version = "v1" });
    // Group endpoints by tag (controller-like grouping for minimal APIs)
    c.TagActionsBy(api =>
    {
        var path = api.RelativePath?.ToLower();
        if (path != null)
        {
            if (path.StartsWith("api/employees"))
                return new[] { "Employee" };
            if (path.StartsWith("api/departments"))
                return new[] { "Department" };
        }
        return new[] { "Other" };
    });
    c.DocInclusionPredicate((name, api) => true);
});

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// Configure form options for file uploads
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

// Configure request size limits
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
});

// Configure Kestrel server options for file uploads
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
});

builder.Services.AddBootswatchThemeSwitcher();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IStringConverter, SystemJsonStringConverter>();
builder.Services.AddScoped<IHttpClientService, HttpClientService>();
builder.Services.AddScoped<HttpRequestResultService>();
builder.Services.AddScoped<IHttpRequestResultService>(provider =>
{
    IHttpRequestResultService service = provider.GetRequiredService<HttpRequestResultService>();
    // Add Telemetry decorator (outermost layer)
    service = new HttpRequestResultServiceTelemetry(
        provider.GetRequiredService<ILogger<HttpRequestResultServiceTelemetry>>(),
        service
    );
    return service;
});

// Add RESTRunner services
builder.Services.AddSingleton<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IConfigurationService, FileConfigurationService>();
builder.Services.AddScoped<ICollectionService, FileCollectionService>();
builder.Services.AddScoped<IExecutionService, SimpleExecutionService>();

// Register CompareRunner as a factory that creates a basic initialized instance
// For web usage, we'll create configurations dynamically rather than using static initialization
builder.Services.AddScoped<CompareRunner>(serviceProvider =>
{
    var runner = new CompareRunner();
    
    // Initialize with basic default configuration
    runner.Instances = new List<CompareInstance>
    {
        new() { Name = "Local", BaseUrl = "https://localhost:7001/" },
        new() { Name = "Demo", BaseUrl = "https://samplecrud.markhazleton.com/" }
    };
    
    runner.Requests = new List<CompareRequest>
    {
        new() { Path = "api/status", RequestMethod = HttpVerb.GET, RequiresClientToken = false }
    };
    
    runner.Users = new List<CompareUser>
    {
        new() 
        { 
            UserName = "default",
            Password = "password"
        }
    };
    
    return runner;
});

// Register ExecuteRunnerService with proper dependencies
builder.Services.AddScoped<IExecuteRunner, ExecuteRunnerService>();

var app = builder.Build();

// Initialize file storage and create sample data on startup
using (var scope = app.Services.CreateScope())
{
    var fileStorage = scope.ServiceProvider.GetRequiredService<IFileStorageService>();
    await fileStorage.InitializeAsync();
    
    // Initialize sample data
    await InitializeSampleDataAsync(scope.ServiceProvider);
}

// Enable Swagger middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RESTRunner API v1");
        c.RoutePrefix = "docs"; // Swagger UI at /docs
    });
}

app.UseStaticFiles();
app.UseRouting();
app.UseBootswatchAll();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Employee Endpoints
app.MapGet("/api/employees/count", async () =>
{
    var service = new SampleCRUDService();
    return await service.GetEmployeeCount();
}).WithTags("Employee");

app.MapGet("/api/employees", async (int? pageNumber, int? pageSize) =>
{
    var service = new SampleCRUDService();
    return await service.GetAllEmployees(pageNumber, pageSize);
}).WithTags("Employee");

app.MapGet("/api/employees/{id}", async (int id) =>
{
    var service = new SampleCRUDService();
    return await service.GetEmployeeById(id);
}).WithTags("Employee");

app.MapPost("/api/employees", async (EmployeeDto employee) =>
{
    var service = new SampleCRUDService();
    return await service.CreateEmployee(employee);
}).WithTags("Employee");

app.MapPut("/api/employees/{id}", async (int id, EmployeeDto employee) =>
{
    var service = new SampleCRUDService();
    return await service.UpdateEmployee(id, employee);
}).WithTags("Employee");

app.MapDelete("/api/employees/{id}", async (int id) =>
{
    var service = new SampleCRUDService();
    return await service.DeleteEmployee(id);
}).WithTags("Employee");

// Department Endpoints
app.MapGet("/api/departments", async (bool? includeEmployees) =>
{
    var service = new SampleCRUDService();
    return await service.GetDepartments(includeEmployees);
}).WithTags("Department");

app.MapGet("/api/departments/{id}", async (int id) =>
{
    var service = new SampleCRUDService();
    return await service.GetDepartmentById(id);
}).WithTags("Department");

app.MapGet("/api/api-explorer", async () =>
{
    var service = new SampleCRUDService();
    return await service.GetApiExplorer();
});

app.MapGet("/api/status", async () =>
{
    var service = new SampleCRUDService();
    return await service.GetStatus();
});

// Add initialization status endpoint
app.MapGet("/api/initialization-status", async (HttpContext context) =>
{
    var serviceProvider = context.RequestServices;
    try
    {
        var configurationService = serviceProvider.GetRequiredService<IConfigurationService>();
        var collectionService = serviceProvider.GetRequiredService<ICollectionService>();
        
        var configurations = await configurationService.GetAllAsync();
        var collections = await collectionService.GetAllAsync();
        
        var initialConfig = configurations.FirstOrDefault(c => c.Name == "Initial RESTRunner Configuration");
        var sampleCollection = collections.FirstOrDefault(c => c.Name == "Sample RESTRunner Test Collection");
        
        return Results.Ok(new
        {
            timestamp = DateTime.UtcNow,
            initialized = true,
            sampleCollection = new
            {
                exists = sampleCollection != null,
                id = sampleCollection?.Id,
                name = sampleCollection?.Name,
                requestCount = sampleCollection?.RequestCount ?? 0
            },
            initialConfiguration = new
            {
                exists = initialConfig != null,
                id = initialConfig?.Id,
                name = initialConfig?.Name,
                totalTests = initialConfig?.GetTotalTestCount() ?? 0,
                iterations = initialConfig?.Iterations ?? 0,
                concurrency = initialConfig?.MaxConcurrency ?? 0
            },
            totals = new
            {
                configurations = configurations.Count,
                collections = collections.Count,
                activeConfigurations = configurations.Count(c => c.IsActive)
            }
        });
    }
    catch (Exception ex)
    {
        return Results.Ok(new
        {
            timestamp = DateTime.UtcNow,
            initialized = false,
            error = ex.Message
        });
    }
}).WithTags("System");

// Add debug endpoint to list all configurations
app.MapGet("/api/debug-configurations", async (HttpContext context) =>
{
    var serviceProvider = context.RequestServices;
    try
    {
        var configurationService = serviceProvider.GetRequiredService<IConfigurationService>();
        var configurations = await configurationService.GetAllAsync();
        
        return Results.Ok(new
        {
            timestamp = DateTime.UtcNow,
            totalConfigurations = configurations.Count,
            configurations = configurations.Select(c => new
            {
                id = c.Id,
                name = c.Name,
                description = c.Description,
                isActive = c.IsActive,
                createdAt = c.CreatedAt,
                iterations = c.Iterations,
                maxConcurrency = c.MaxConcurrency,
                instancesCount = c.Runner?.Instances?.Count ?? 0,
                usersCount = c.Runner?.Users?.Count ?? 0,
                requestsCount = c.Runner?.Requests?.Count ?? 0,
                editUrl = $"/Configuration/Edit/{c.Id}"
            })
        });
    }
    catch (Exception ex)
    {
        return Results.Ok(new
        {
            timestamp = DateTime.UtcNow,
            error = ex.Message
        });
    }
}).WithTags("Debug");

app.Run();

/// <summary>
/// Initialize sample data including default configuration and collection
/// </summary>
/// <param name="serviceProvider">Service provider for dependency injection</param>
static async Task InitializeSampleDataAsync(IServiceProvider serviceProvider)
{
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var configurationService = serviceProvider.GetRequiredService<IConfigurationService>();
        
        // Check if initial configuration exists
        var existingConfigurations = await configurationService.GetAllAsync();
        var initialConfig = existingConfigurations.FirstOrDefault(c => c.Name == "Initial RESTRunner Configuration");
        
        if (initialConfig == null)
        {
            logger.LogInformation("Creating initial configuration based on console RESTRunner defaults...");
            
            // Create initial configuration based on console RESTRunner defaults
            var configuration = new RESTRunner.Web.Models.TestConfiguration
            {
                Name = "Initial RESTRunner Configuration",
                Description = "Default configuration created automatically using console RESTRunner values (100 iterations, 10 concurrency)",
                Iterations = 100, // Default from console app
                MaxConcurrency = 10, // Default from console app
                Tags = new List<string> { "initial", "demo", "auto-generated", "console-defaults" },
                CreatedBy = "System",
                IsActive = true,
                Runner = new CompareRunner
                {
                    // Set up instances (matching console app defaults)
                    Instances = new List<CompareInstance>
                    {
                        new() { Name = "Local", BaseUrl = "https://localhost:44315/" },
                        new() { Name = "Demo", BaseUrl = "https://samplecrud.markhazleton.com/" }
                    },
                    
                    // Set up users (matching console app pattern)
                    Users = new List<CompareUser>
                    {
                        new() 
                        { 
                            UserName = "testuser",
                            Password = "password",
                            Properties = new Dictionary<string, string>
                            {
                                { "email", "test@example.com" },
                                { "role", "tester" },
                                { "department", "QA" }
                            }
                        },
                        new() 
                        { 
                            UserName = "admin",
                            Password = "admin123",
                            Properties = new Dictionary<string, string>
                            {
                                { "email", "admin@example.com" },
                                { "role", "administrator" },
                                { "department", "IT" }
                            }
                        }
                    },
                    
                    // Set up requests based on available API endpoints
                    Requests = new List<CompareRequest>
                    {
                        new() 
                        { 
                            Path = "api/status", 
                            RequestMethod = HttpVerb.GET, 
                            RequiresClientToken = false
                        },
                        new() 
                        { 
                            Path = "api/employees", 
                            RequestMethod = HttpVerb.GET, 
                            RequiresClientToken = false
                        },
                        new() 
                        { 
                            Path = "api/employees/1", 
                            RequestMethod = HttpVerb.GET, 
                            RequiresClientToken = false
                        },
                        new() 
                        { 
                            Path = "api/employees/count", 
                            RequestMethod = HttpVerb.GET, 
                            RequiresClientToken = false
                        },
                        new() 
                        { 
                            Path = "api/departments", 
                            RequestMethod = HttpVerb.GET, 
                            RequiresClientToken = false
                        }
                    }
                }
            };
            
            await configurationService.CreateAsync(configuration);
            logger.LogInformation("Initial configuration created with ID: {ConfigurationId}, Total Tests: {TotalTests}", 
                configuration.Id, configuration.GetTotalTestCount());
        }
        else
        {
            logger.LogInformation("Initial configuration already exists with ID: {ConfigurationId}", initialConfig.Id);
        }
        
        logger.LogInformation("Sample data initialization completed successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to initialize sample data");
    }
}

















