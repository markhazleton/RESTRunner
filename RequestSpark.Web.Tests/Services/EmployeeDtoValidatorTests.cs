namespace RequestSpark.Web.Tests.Services;

[TestClass]
public class EmployeeDtoValidatorTests
{
    [TestMethod]
    public void Validate_WithValidEmployee_ReturnsNoErrors()
    {
        var employee = new EmployeeDto
        {
            Age = 30,
            Country = "USA",
            Department = EmployeeDepartmentEnum._1,
            Gender = GenderEnum._1,
            Id = 1,
            Name = "Jane Doe",
            State = "TX",
            Profile_picture = "profile.png"
        };

        var errors = EmployeeDtoValidator.Validate(employee);

        Assert.AreEqual(0, errors.Count);
    }

    [TestMethod]
    public void Validate_WithMissingRequiredFields_ReturnsErrors()
    {
        var employee = new EmployeeDto
        {
            Age = 0,
            Country = string.Empty,
            Name = string.Empty,
            State = string.Empty,
            Profile_picture = string.Empty
        };

        var errors = EmployeeDtoValidator.Validate(employee);

        Assert.IsTrue(errors.ContainsKey(nameof(EmployeeDto.Age)));
        Assert.IsTrue(errors.ContainsKey(nameof(EmployeeDto.Country)));
        Assert.IsTrue(errors.ContainsKey(nameof(EmployeeDto.Name)));
        Assert.IsTrue(errors.ContainsKey(nameof(EmployeeDto.State)));
        Assert.IsTrue(errors.ContainsKey(nameof(EmployeeDto.Profile_picture)));
    }
}
