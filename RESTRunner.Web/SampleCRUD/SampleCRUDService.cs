namespace RESTRunner.Web.SampleCRUD;

public class SampleCRUDService
{
    private readonly HttpClient _client;
    private readonly SampleCRUDClient _crudClient;
    private const string ApiVersion = "1";

    public SampleCRUDService()
    {
        _client = new HttpClient
        {
            BaseAddress = new Uri("https://samplecrud.markhazleton.com/")
        };
        _crudClient = new SampleCRUDClient(_client);
    }

    public async Task<string> GetEmployeeCount()
    {
        var employees = await _crudClient.EmployeeAllAsync(1, 15, ApiVersion);
        return $"Got {employees.Count} Employees";
    }

    public Task<ICollection<EmployeeDto>> GetAllEmployees(int? pageNumber = 1, int? pageSize = 15)
        => _crudClient.EmployeeAllAsync(pageNumber, pageSize, ApiVersion);

    public Task<EmployeeDto> GetEmployeeById(int id)
        => _crudClient.EmployeeGETAsync(id, ApiVersion);

    public Task<EmployeeDto> CreateEmployee(EmployeeDto employee)
        => _crudClient.EmployeePOSTAsync(ApiVersion, employee);

    public Task<EmployeeDto> UpdateEmployee(int id, EmployeeDto employee)
        => _crudClient.EmployeePUTAsync(id, ApiVersion, employee);

    public Task<EmployeeDto> DeleteEmployee(int id)
        => _crudClient.DeleteEmployeeAsync(id, ApiVersion);

    public Task<ICollection<DepartmentDto>> GetDepartments(bool? includeEmployees = null)
        => _crudClient.DepartmentAsync(includeEmployees, ApiVersion);

    public Task<ICollection<DepartmentDto>> GetDepartmentById(int id)
        => _crudClient.Department2Async(id, ApiVersion);

    public Task<ICollection<ApiExplorerModel>> GetApiExplorer()
        => _crudClient.ExplorerAsync(ApiVersion);

    public Task<ApplicationStatus> GetStatus()
        => _crudClient.StatusAsync(ApiVersion);
}
