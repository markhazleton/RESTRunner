namespace RESTRunner.Web.SampleCRUD
{
    public class SampleCRUDService
    {
        public SampleCRUDService()
        {

        }
        public async Task<string> GetEmployeeCount()
        {
            HttpClient client = new()
            {
                BaseAddress = new Uri("https://markhazletonsamplecrud.controlorigins.com/")
            };
            SampleCRUDClient sampleCRUClients = new(client);
            var employees = await sampleCRUClients.EmployeeAllAsync(1, 15, "1");
            return $"Got {employees.Count} Employees";
        }

    }
}
