using RequestSpark.Web.SampleCRUD;
using System.ComponentModel.DataAnnotations;

namespace RequestSpark.Web.Services;

/// <summary>
/// Validates employee payloads before they are forwarded to the sample CRUD API.
/// </summary>
public static class EmployeeDtoValidator
{
    /// <summary>
    /// Validates an employee payload and returns Minimal API-compatible errors.
    /// </summary>
    /// <param name="employee">Employee payload.</param>
    /// <returns>Validation errors keyed by member name.</returns>
    public static Dictionary<string, string[]> Validate(EmployeeDto employee)
    {
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var validationContext = new ValidationContext(employee);

        Validator.TryValidateObject(employee, validationContext, validationResults, validateAllProperties: true);

        var errors = validationResults
            .GroupBy(result => result.MemberNames.FirstOrDefault() ?? string.Empty)
            .ToDictionary(
                group => string.IsNullOrWhiteSpace(group.Key) ? string.Empty : group.Key,
                group => group.Select(result => result.ErrorMessage ?? "Validation error").Distinct().ToArray(),
                StringComparer.OrdinalIgnoreCase);

        AddIfInvalid(errors, employee.Age <= 0, nameof(employee.Age), "Age must be greater than zero.");
        AddIfInvalid(errors, string.IsNullOrWhiteSpace(employee.Country), nameof(employee.Country), "Country is required.");

        return errors;
    }

    private static void AddIfInvalid(Dictionary<string, string[]> errors, bool condition, string key, string message)
    {
        if (!condition)
        {
            return;
        }

        if (errors.TryGetValue(key, out var existing))
        {
            errors[key] = existing.Concat([message]).Distinct().ToArray();
            return;
        }

        errors[key] = [message];
    }
}
