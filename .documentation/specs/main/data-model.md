# Data Model: Audit Remediation

**Feature**: Audit Finding Remediation (2026-03-16)
**Phase**: 1 Design Output

No new domain entities are introduced. Changes are structural (new files, new projects, new interface extraction) and operational (CI workflow, test coverage). The models below document the new types and interfaces created as part of the decomposition work.

---

## New Type: DomainConstants

**Location**: `RESTRunner.Domain/Constants/DomainConstants.cs`

```csharp
namespace RESTRunner.Domain.Constants;

public static class DomainConstants
{
    /// <summary>
    /// Placeholder password used in sample/seed data only.
    /// Never use in production credential management.
    /// </summary>
    public const string SamplePassword = "[sample]";
}
```

**Consumers**: 5 production-code files (see research.md §3).

---

## New Interface: IExecutionHistoryService

**Location**: `RESTRunner.Web/Services/IExecutionHistoryService.cs`

```csharp
public interface IExecutionHistoryService
{
    Task SaveHistoryAsync(ExecutionHistory history);
    Task<ExecutionHistory?> GetHistoryAsync(string executionId);
    Task<IEnumerable<ExecutionHistory>> GetAllHistoryAsync(int pageSize, int pageNumber, string? configurationId);
    Task<bool> DeleteHistoryAsync(string executionId);
}
```

**Implemented by**: `ExecutionHistoryService` (new) and transitionally by `RealExecutionService` until migration completes.

---

## Extracted Class: OpenApiSpecParser

**Location**: `RESTRunner.Web/Services/OpenApiSpecParser.cs`

Responsibility: stateless parsing of OpenAPI 3.x / Swagger 2.0 JSON. All methods are `internal static` or `internal` — no DI registration needed.

Key members extracted from `FileOpenApiService`:
- `ParseSpecContent(string json) → OpenApiSpec`
- `ExtractEndpoints(JsonElement root) → List<OpenApiEndpoint>`
- `ExtractSchemaType(JsonElement schema) → string`

---

## New Test Projects

### RESTRunner.Web.Tests
- **Type**: MSTest project, `net10.0`
- **References**: `RESTRunner.Web`, `Microsoft.AspNetCore.Mvc.Testing` (for integration-style tests)
- **Initial test classes**:
  - `Controllers/ExecutionControllerTests.cs`
  - `Controllers/ConfigurationControllerTests.cs`
  - `Services/RealExecutionServiceTests.cs`

### RESTRunner.Services.Tests
- **Type**: MSTest project, `net10.0`
- **References**: `RESTRunner.Services.HttpClient`, `RESTRunner.Domain`
- **Initial test classes**:
  - `ExecuteRunnerServiceTests.cs`

---

## New Controller: ExecutionApiController

**Location**: `RESTRunner.Web/Controllers/ExecutionApiController.cs`
- Derives from `ControllerBase`
- Route: `/api/execution`
- Receives same constructor injections as `ExecutionController`
- Contains all actions currently tagged `[HttpGet("/api/...")]` / `[HttpPost("/api/...")]` in `ExecutionController.cs`

---

## CI Workflow

**Location**: `.github/workflows/build.yml`
- Triggers: `push` and `pull_request` to `main`
- Runner: `ubuntu-latest`
- Steps: checkout → setup-dotnet 10.x → restore → build → test
