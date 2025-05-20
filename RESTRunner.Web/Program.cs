using RESTRunner.Web.SampleCRUD;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", async () =>
{
    var sampleCRUDService = new SampleCRUDService();
    return await sampleCRUDService.GetEmployeeCount().ConfigureAwait(true);
});

app.Run();


