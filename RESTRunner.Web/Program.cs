using Microsoft.OpenApi.Models; // Add for Swagger
using RESTRunner.Web.SampleCRUD;
using WebSpark.Bootswatch; // Add for Bootswatch
using WebSpark.HttpClientUtility.ClientService;
using WebSpark.HttpClientUtility.RequestResult; // Add for HttpRequestResultService
using WebSpark.HttpClientUtility.StringConverter;

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

var app = builder.Build();

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

app.Run();


