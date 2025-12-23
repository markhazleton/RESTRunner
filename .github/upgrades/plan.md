# .NET 10 Upgrade Plan for RESTRunner Solution

## Table of Contents

- [Executive Summary](#executive-summary)
- [Migration Strategy](#migration-strategy)
- [Detailed Dependency Analysis](#detailed-dependency-analysis)
- [Project-by-Project Migration Plans](#project-by-project-migration-plans)
- [Package Update Reference](#package-update-reference)
- [Breaking Changes Catalog](#breaking-changes-catalog)
- [Testing & Validation Strategy](#testing--validation-strategy)
- [Risk Management](#risk-management)
- [Complexity & Effort Assessment](#complexity--effort-assessment)
- [Source Control Strategy](#source-control-strategy)
- [Success Criteria](#success-criteria)

---

## Executive Summary

### Scenario Description
Upgrade RESTRunner solution from .NET 9.0 to .NET 10.0 (Long Term Support). This solution includes a console application for batch API testing, a Razor Pages web application for interactive testing, and supporting class libraries for domain models, Postman import, and HTTP client execution.

### Scope
- **Projects Affected**: 6 projects (all require upgrade from net9.0 to net10.0)
- **Current State**: All projects targeting .NET 9.0
- **Target State**: All projects targeting .NET 10.0 (LTS)
- **Total Codebase**: 11,634 lines of code across 78 files
- **Estimated Impact**: 137+ lines requiring attention (1.2% of codebase)

### Key Metrics
- **Total Projects**: 6
- **Total NuGet Packages**: 20 (7 need upgrade, 1 incompatible, 2 redundant)
- **Package Compatibility**: 65% compatible, 30% upgrade recommended, 5% incompatible
- **API Issues**: 137 behavioral changes (primarily System.Uri and HttpContent), 0 binary/source incompatibilities
- **Dependency Depth**: 2 levels (simple structure)
- **Circular Dependencies**: None

### Complexity Assessment
**Classification**: ?? **Simple Solution**

**Rationale**:
- Small solution (6 projects, <15K LOC)
- Shallow dependency tree (max depth 2)
- All projects marked "Low" difficulty by assessment
- Homogeneous baseline (all projects on net9.0)
- No security vulnerabilities detected
- Clear dependency structure without cycles
- All projects SDK-style (no conversion needed)

### Selected Strategy
**All-At-Once Strategy** — All projects upgraded simultaneously in a single coordinated operation.

**Justification**:
- Solution size ideal for atomic upgrade (6 projects)
- All projects currently on modern .NET (9.0), consistent starting point
- Simple dependency structure supports simultaneous update
- All required packages have known target versions
- Low complexity across all projects reduces coordination risk
- Single comprehensive testing phase more efficient than staged approach

### Critical Issues
? **No Security Vulnerabilities** - All packages are secure

?? **Incompatible Package**:
- `Microsoft.VisualStudio.Azure.Containers.Tools.Targets` v1.22.1 (RESTRunner.Web) - Development-time tool, may need removal or updated version

?? **Redundant Packages** (included in .NET 10 framework):
- `System.Net.Http` v4.3.4 (RESTRunner.Web)
- `System.Text.RegularExpressions` v4.3.1 (RESTRunner.Web)

?? **Behavioral Changes**:
- 137 instances across 4 projects (primarily System.Uri and HttpContent usage)
- Requires runtime testing validation, not code changes

### Iteration Strategy Used
**Fast batch approach** — Simple solution enables consolidated detail generation:
1. **Phase 1**: Discovery & classification (3 iterations) ?
2. **Phase 2**: Foundation sections (3 iterations)
3. **Phase 3**: Consolidated project details (2 iterations)
4. **Total Expected**: 8 iterations

### Recommended Approach
Proceed with All-At-Once strategy for fastest completion with manageable risk.

---

## Migration Strategy

### Approach Selection: All-At-Once Strategy

**Selected Strategy**: All projects upgraded simultaneously in a single coordinated operation.

### Rationale

**Solution Characteristics Supporting All-At-Once**:
1. **Small Solution** - 6 projects (well below 30-project threshold)
2. **Homogeneous Baseline** - All projects currently on .NET 9.0 (modern .NET, not Framework)
3. **Simple Dependencies** - Max depth 2, no circular dependencies
4. **Low Complexity** - All projects assessed as "Low" difficulty
5. **Package Compatibility** - All required packages have .NET 10 versions (except 1 dev tool)
6. **No Security Issues** - No vulnerabilities requiring immediate attention

**All-At-Once Strategy Advantages**:
- ? **Fastest Completion** - Single upgrade phase, no multi-targeting overhead
- ? **Clean Testing** - One comprehensive test pass vs. multiple phase validations
- ? **No Intermediate States** - Avoid complexity of supporting multiple frameworks simultaneously
- ? **Simplified Coordination** - All developers migrate together, no "which version" confusion
- ? **Unified Dependencies** - All projects reference same package versions immediately

**Risks Mitigated**:
- ?? **Rollback Plan** - Git branch isolation (`upgrade-to-NET10`) enables instant revert
- ?? **Build Validation** - Atomic operation ensures all projects build together before commit
- ?? **Test Safety** - MSTest suite validates behavioral changes don't break functionality
- ?? **Package Conflicts** - Single package resolution pass eliminates version mismatches

### Migration Principles

**1. Simultaneity**
All project files and package references updated together in one pass. No project left in intermediate state.

**2. Atomic Commit**
Entire upgrade committed as single unit. Solution always in working state at commit boundaries.

**3. Validation Before Commit**
Solution must build with 0 errors and all tests pass before committing upgrade.

**4. Breaking Change Review**
Behavioral changes tested at runtime through existing MSTest suite and manual smoke tests.

### Execution Phases

#### Phase 0: Prerequisites (if needed)
- Verify .NET 10 SDK installed
- Check global.json compatibility (if present)
- Confirm upgrade branch created (`upgrade-to-NET10`)

#### Phase 1: Atomic Upgrade
**Operations** (performed as single coordinated batch):
1. Update all 6 project files: `<TargetFramework>net9.0</TargetFramework>` ? `<TargetFramework>net10.0</TargetFramework>`
2. Update all package references across projects (7 packages to update version, 2 to remove)
3. Restore dependencies: `dotnet restore RESTRunner.sln`
4. Build entire solution: `dotnet build RESTRunner.sln`
5. Fix compilation errors (if any) based on breaking changes catalog
6. Rebuild to verify fixes

**Deliverables**: 
- Solution builds with 0 errors
- Solution builds with 0 warnings (goal)
- All projects targeting net10.0

#### Phase 2: Test Validation
**Operations**:
1. Execute MSTest suite: `dotnet test RESTRunner.Domain.Tests.csproj`
2. Address test failures related to behavioral changes
3. Manual smoke test: Console app with sample Postman collection
4. Manual smoke test: Web app basic navigation and test execution

**Deliverables**:
- All unit tests pass
- Console app successfully executes test collection
- Web app loads and executes basic scenarios

#### Phase 3: Final Validation
**Operations**:
1. Code review: Verify no unexpected changes
2. Documentation update: README/release notes reflect .NET 10
3. Commit upgrade with descriptive message
4. (Optional) Push branch and create pull request

**Deliverables**:
- Clean commit in `upgrade-to-NET10` branch
- Ready for merge to main branch

### Dependency-Based Ordering (Within Atomic Operation)

While all projects upgrade simultaneously, internal operations follow dependency order:

**Project File Updates** (can be parallel):
1. RESTRunner.Domain.csproj
2. RESTRunner.PostmanImport.csproj, RESTRunner.Services.HttpClientRunner.csproj
3. RESTRunner.csproj, RESTRunner.Web.csproj, RESTRunner.Domain.Tests.csproj

**Package Updates** (coordinated across projects):
- Restore runs once after all project files updated
- Package resolution determines final versions for entire solution

**Build Validation** (follows dependencies):
- RESTRunner.Domain builds first (implicit)
- Dependent projects build next (automatic)
- MSBuild handles ordering

### Parallel vs. Sequential Execution

**Sequential Operations** (must be ordered):
1. Project file updates ? Restore ? Build ? Fix errors ? Test
2. Cannot test before building, cannot build before restoring

**Parallel Opportunities** (not applicable for small solution):
- All 6 project files can be edited simultaneously
- Not worth parallelizing for this solution size

### Success Criteria Summary

? All 6 projects target `net10.0`  
? All 7 package updates applied  
? 2 redundant packages removed  
? Solution builds with 0 errors  
? All MSTest tests pass  
? Console app smoke test passes  
? Web app smoke test passes  
? No new warnings introduced  
? Committed to `upgrade-to-NET10` branch

---

## Detailed Dependency Analysis

### Dependency Graph Summary

The RESTRunner solution has a clean, layered architecture with no circular dependencies:

**Layer 0 (Foundation)**: 
- `RESTRunner.Domain` - Core models and interfaces (no dependencies)

**Layer 1 (Services)**:
- `RESTRunner.PostmanImport` - Postman collection parser (depends on Domain)
- `RESTRunner.Services.HttpClientRunner` - HTTP execution engine (depends on Domain)

**Layer 2 (Applications)**:
- `RESTRunner` - Console application (depends on Domain, PostmanImport, Services.HttpClientRunner)
- `RESTRunner.Web` - Razor Pages web app (depends on Domain, PostmanImport, Services.HttpClientRunner)
- `RESTRunner.Domain.Tests` - MSTest unit tests (depends on Domain, PostmanImport)

### Project Groupings for All-At-Once Migration

Since this is an All-At-Once strategy, all projects will be upgraded simultaneously. However, understanding the dependency layers helps identify validation checkpoints:

**Atomic Upgrade Group (All Projects)**:
1. **RESTRunner.Domain** (0 dependencies, 5 dependants)
   - Foundation library, highest reuse
   - 1,428 LOC, 2 behavioral change instances
   
2. **RESTRunner.PostmanImport** (1 dependency, 3 dependants)
   - Postman v2.1.0 collection parser
   - 303 LOC, 0 API issues
   
3. **RESTRunner.Services.HttpClientRunner** (1 dependency, 2 dependants)
   - Core execution engine with HttpClient
   - 373 LOC, 19 behavioral change instances
   - 2 package updates needed
   
4. **RESTRunner** (3 dependencies, 0 dependants)
   - Console application (main entry point)
   - 310 LOC, 3 behavioral change instances
   - 4 package updates needed
   
5. **RESTRunner.Web** (3 dependencies, 0 dependants)
   - Razor Pages web application
   - 8,744 LOC (largest project), 113 behavioral change instances
   - 4 package updates needed, 1 incompatible package
   
6. **RESTRunner.Domain.Tests** (2 dependencies, 0 dependants)
   - MSTest unit tests
   - 476 LOC, 0 API issues

### Critical Path

**All projects upgraded together**, but validation should prioritize:
1. **RESTRunner.Domain** - Foundation used by all projects
2. **Services.HttpClientRunner** - Highest API issue density (19 behavioral changes in 373 LOC = 5.1%)
3. **RESTRunner.Web** - Largest project (8,744 LOC), most total issues (113), has incompatible package

### Circular Dependencies
? **None detected** - Clean dependency structure supports straightforward migration

### Dependency Visualization

```
RESTRunner.Domain (Layer 0)
    ?
    ??? RESTRunner.PostmanImport (Layer 1)
    ?       ?
    ?       ??? RESTRunner (Layer 2)
    ?       ??? RESTRunner.Web (Layer 2)
    ?       ??? RESTRunner.Domain.Tests (Layer 2)
    ?
    ??? RESTRunner.Services.HttpClientRunner (Layer 1)
            ?
            ??? RESTRunner (Layer 2)
            ??? RESTRunner.Web (Layer 2)
```

**Migration Advantage**: Single atomic operation eliminates need for multi-targeting or phased compatibility testing.

---

## Project-by-Project Migration Plans

### Overview
All projects upgraded simultaneously (All-At-Once strategy). Details below provide context for validation and troubleshooting.

---

### 1. RESTRunner.Domain

**Current State**: net9.0, 0 dependencies, 1,428 LOC, 1 NuGet package (FileHelpers 3.5.2)

**Target State**: net10.0

**Risk Level**: ?? Low

**Package Dependencies**:
| Package | Current Version | Target Version | Action |
|---------|----------------|----------------|--------|
| FileHelpers | 3.5.2 | 3.5.2 | ? Compatible (no update needed) |

**Migration Steps**:

1. **Update Project File** (`RESTRunner.Domain\RESTRunner.Domain.csproj`)
   ```xml
   <!-- Change: -->
   <TargetFramework>net9.0</TargetFramework>
   <!-- To: -->
   <TargetFramework>net10.0</TargetFramework>
   ```

2. **Package Updates**
   - No updates required (FileHelpers 3.5.2 compatible with .NET 10)

3. **Expected Breaking Changes**
   - **Behavioral Changes**: 2 instances detected in assessment
   - **APIs Affected**: Likely System.Uri usage in domain models
   - **Expected Impact**: Low - behavioral changes require runtime validation, not code modifications

4. **Code Modifications**
   - No immediate code changes expected
   - Monitor for behavioral differences in URI handling if domain models use System.Uri

5. **Testing Strategy**
   - **Unit Tests**: RESTRunner.Domain.Tests exercises domain models
   - **Validation**: All existing tests should pass
   - **Focus Areas**: Any models using System.Uri or HTTP-related types

6. **Validation Checklist**
   - [ ] Project targets `net10.0`
   - [ ] Builds without errors
   - [ ] Builds without warnings
   - [ ] RESTRunner.Domain.Tests suite passes
   - [ ] No runtime exceptions in dependent projects

**Dependencies Impact**: This is the foundation library - 5 projects depend on it (PostmanImport, Services.HttpClientRunner, Console, Web, Tests). Must build successfully before dependents.

---

### 2. RESTRunner.PostmanImport

**Current State**: net9.0, 1 dependency (Domain), 303 LOC, 1 NuGet package (Newtonsoft.Json 13.0.4)

**Target State**: net10.0

**Risk Level**: ?? Low

**Package Dependencies**:
| Package | Current Version | Target Version | Action |
|---------|----------------|----------------|--------|
| Newtonsoft.Json | 13.0.4 | 13.0.4 | ? Compatible (no update needed) |

**Migration Steps**:

1. **Update Project File** (`RESTRunner.PostmanImport\RESTRunner.PostmanImport.csproj`)
   ```xml
   <!-- Change: -->
   <TargetFramework>net9.0</TargetFramework>
   <!-- To: -->
   <TargetFramework>net10.0</TargetFramework>
   ```

2. **Package Updates**
   - No updates required (Newtonsoft.Json 13.0.4 compatible with .NET 10)

3. **Expected Breaking Changes**
   - **API Issues**: 0 (no behavioral changes detected)
   - **Expected Impact**: None

4. **Code Modifications**
   - No code changes expected
   - Postman collection parsing logic should work unchanged

5. **Testing Strategy**
   - **Manual Test**: Import sample `collection.json` in console or web app
   - **Validation**: Verify CompareRunner populated correctly from Postman v2.1.0 format
   - **Edge Cases**: Test URL template substitution (`{{url}}/path` ? relative path)

6. **Validation Checklist**
   - [ ] Project targets `net10.0`
   - [ ] Builds without errors
   - [ ] Builds without warnings
   - [ ] Sample Postman collection imports successfully
   - [ ] Request properties mapped correctly (path, method, body)

**Dependencies Impact**: Depends on Domain (must build after Domain). Used by Console, Web, and Tests projects.

---

### 3. RESTRunner.Services.HttpClientRunner

**Current State**: net9.0, 1 dependency (Domain), 373 LOC, 3 NuGet packages

**Target State**: net10.0

**Risk Level**: ?? Medium (Highest issue density: 5.1%)

**Package Dependencies**:
| Package | Current Version | Target Version | Action |
|---------|----------------|----------------|--------|
| Microsoft.AspNet.WebApi.Client | 6.0.0 | 6.0.0 | ? Compatible (no update needed) |
| Microsoft.Extensions.Http | 9.0.9 | 10.0.1 | ?? Update recommended |
| Microsoft.Extensions.Logging.Abstractions | 9.0.9 | 10.0.1 | ?? Update recommended |

**Migration Steps**:

1. **Update Project File** (`RESTRunner.Services.HttpClient\RESTRunner.Services.HttpClientRunner.csproj`)
   ```xml
   <!-- Change: -->
   <TargetFramework>net9.0</TargetFramework>
   <!-- To: -->
   <TargetFramework>net10.0</TargetFramework>
   ```

2. **Package Updates**
   ```xml
   <!-- Update: -->
   <PackageReference Include="Microsoft.Extensions.Http" Version="9.0.9" />
   <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="9.0.9" />
   
   <!-- To: -->
   <PackageReference Include="Microsoft.Extensions.Http" Version="10.0.1" />
   <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="10.0.1" />
   ```

3. **Expected Breaking Changes**
   - **Behavioral Changes**: 19 instances (highest density in solution)
   - **APIs Affected**:
     - `System.Uri` constructors and parsing (likely in request URL handling)
     - `System.Net.Http.HttpContent` reading (response body parsing)
     - `HttpClientFactory.AddHttpClient` registration behavior
   - **Expected Impact**: Medium - core execution engine, but changes are behavioral (not binary incompatible)

4. **Code Modifications**
   - **Likely Areas** (based on project purpose):
     - `ExecuteRunnerService.GetResponseAsync()` - HTTP request execution
     - URI template substitution (`{{encoded_user_name}}`, `{{property_key}}`)
     - Response content stream handling
   - **No Expected Changes**: Behavioral changes typically don't require code modifications
   - **Monitoring**: Watch for differences in URL parsing or response reading

5. **Testing Strategy**
   - **Unit Tests**: RESTRunner.Domain.Tests may cover some scenarios
   - **Integration Tests**: Execute sample requests against test API
   - **Focus Areas**:
     - Absolute vs. relative URI handling
     - Template substitution in request paths/bodies
     - Response body parsing and hashing
     - Error handling for malformed URIs
   - **Validation**: Compare .NET 9 vs .NET 10 results for same requests

6. **Validation Checklist**
   - [ ] Project targets `net10.0`
   - [ ] Packages updated to 10.0.1 (Microsoft.Extensions.*)
   - [ ] Builds without errors
   - [ ] Builds without warnings
   - [ ] HttpClient instances created correctly
   - [ ] Request execution works (GET, POST, PUT, DELETE, PATCH, etc.)
   - [ ] Response body parsed correctly
   - [ ] Template substitution works
   - [ ] Statistics collection accurate (percentiles, status codes)

**Dependencies Impact**: Depends on Domain. Used by Console and Web apps - critical component for both.

**Special Attention**: This project has highest concentration of behavioral changes. Thorough testing essential before declaring success.

---

### 4. RESTRunner (Console App)

**Current State**: net9.0, 3 dependencies, 310 LOC, 5 NuGet packages

**Target State**: net10.0

**Risk Level**: ?? Low

**Package Dependencies**:
| Package | Current Version | Target Version | Action |
|---------|----------------|----------------|--------|
| CsvHelper | 33.1.0 | 33.1.0 | ? Compatible (no update needed) |
| Microsoft.Extensions.Hosting | 9.0.9 | 10.0.1 | ?? Update recommended |
| Microsoft.Extensions.Http | 9.0.9 | 10.0.1 | ?? Update recommended |
| System.Configuration.ConfigurationManager | 9.0.9 | 10.0.1 | ?? Update recommended |
| System.Text.Json | 9.0.9 | 10.0.1 | ?? Update recommended |

**Migration Steps**:

1. **Update Project File** (`RESTRunner\RESTRunner.csproj`)
   ```xml
   <!-- Change: -->
   <TargetFramework>net9.0</TargetFramework>
   <!-- To: -->
   <TargetFramework>net10.0</TargetFramework>
   ```

2. **Package Updates**
   ```xml
   <!-- Update: -->
   <PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.9" />
   <PackageReference Include="Microsoft.Extensions.Http" Version="9.0.9" />
   <PackageReference Include="System.Configuration.ConfigurationManager" Version="9.0.9" />
   <PackageReference Include="System.Text.Json" Version="9.0.9" />
   
   <!-- To: -->
   <PackageReference Include="Microsoft.Extensions.Hosting" Version="10.0.1" />
   <PackageReference Include="Microsoft.Extensions.Http" Version="10.0.1" />
   <PackageReference Include="System.Configuration.ConfigurationManager" Version="10.0.1" />
   <PackageReference Include="System.Text.Json" Version="10.0.1" />
   ```

3. **Expected Breaking Changes**
   - **Behavioral Changes**: 3 instances detected
   - **APIs Affected**:
     - `Microsoft.Extensions.Hosting.HostBuilder` (1 instance)
     - `HttpClientFactory.AddHttpClient` (likely)
     - `ConsoleLoggerExtensions.AddConsole` (1 instance)
   - **Expected Impact**: Low - console app with straightforward DI setup

4. **Code Modifications**
   - **Likely Areas** (based on assessment):
     - `Program.cs` - Host builder and DI registration
     - Console logging configuration
   - **No Expected Changes**: Behavioral changes typically don't break compilation
   - **Monitoring**: Watch for logging output changes or DI resolution differences

5. **Testing Strategy**
   - **Smoke Test**: Run console app with sample Postman collection
   - **Validation Steps**:
     1. Place `collection.json` in RESTRunner output directory
     2. Run: `dotnet run --project RESTRunner\RESTRunner.csproj`
     3. Verify execution completes successfully
     4. Check CSV output generated (`c:\test\RESTRunner.csv`)
     5. Review statistics display (P50/P75/P90/P95/P99 percentiles)
   - **Expected Output**: 
     - Execution report with emoji indicators (? 2xx, ?? 4xx, ? 5xx)
     - Breakdown by HTTP method, status code, instance, user
     - CSV file with individual results

6. **Validation Checklist**
   - [ ] Project targets `net10.0`
   - [ ] Packages updated to 10.0.1 (Microsoft.Extensions.*)
   - [ ] Builds without errors
   - [ ] Builds without warnings
   - [ ] Console app starts successfully
   - [ ] Postman collection imported correctly
   - [ ] Requests execute against configured instances
   - [ ] Statistics calculated correctly
   - [ ] CSV output generated
   - [ ] Console logging displays properly

**Dependencies Impact**: Depends on Domain, PostmanImport, Services.HttpClientRunner. No dependents (application entry point).

---

### 5. RESTRunner.Web (Razor Pages)

**Current State**: net9.0, 3 dependencies, 8,744 LOC (largest project), 10 NuGet packages

**Target State**: net10.0

**Risk Level**: ?? Medium (Largest project, 113 behavioral changes, incompatible package)

**Package Dependencies**:
| Package | Current Version | Target Version | Action |
|---------|----------------|----------------|--------|
| Microsoft.VisualStudio.Azure.Containers.Tools.Targets | 1.22.1 | N/A | ?? Remove (incompatible, dev tool) |
| Newtonsoft.Json | 13.0.4 | 13.0.4 | ? Compatible (no update needed) |
| Swashbuckle.AspNetCore | 9.0.4 | 9.0.4 | ? Compatible (no update needed) |
| System.Net.Http | 4.3.4 | N/A | ?? Remove (framework-included) |
| System.Security.Cryptography.Xml | 9.0.9 | 10.0.1 | ?? Update recommended |
| System.Text.RegularExpressions | 4.3.1 | N/A | ?? Remove (framework-included) |
| WebSpark.Bootswatch | 1.30.0 | 1.30.0 | ? Compatible (no update needed) |
| WebSpark.HttpClientUtility | 1.2.0 | 1.2.0 | ? Compatible (no update needed) |

**Migration Steps**:

1. **Update Project File** (`RESTRunner.Web\RESTRunner.Web.csproj`)
   ```xml
   <!-- Change: -->
   <TargetFramework>net9.0</TargetFramework>
   <!-- To: -->
   <TargetFramework>net10.0</TargetFramework>
   ```

2. **Package Updates and Removals**
   ```xml
   <!-- UPDATE: -->
   <PackageReference Include="System.Security.Cryptography.Xml" Version="9.0.9" />
   <!-- To: -->
   <PackageReference Include="System.Security.Cryptography.Xml" Version="10.0.1" />
   
   <!-- REMOVE (incompatible dev tool): -->
   <PackageReference Include="Microsoft.VisualStudio.Azure.Containers.Tools.Targets" Version="1.22.1" />
   
   <!-- REMOVE (framework-included): -->
   <PackageReference Include="System.Net.Http" Version="4.3.4" />
   <PackageReference Include="System.Text.RegularExpressions" Version="4.3.1" />
   ```

3. **Expected Breaking Changes**
   - **Behavioral Changes**: 113 instances (82% of total solution issues)
   - **APIs Affected**:
     - `System.Uri` - 59 instances (URL parsing in Razor Pages, minimal APIs)
     - `System.Net.Http.HttpContent` - 53 instances (API responses, HTTP client usage)
     - Various System.Uri constructors - 19 instances
   - **Expected Impact**: Medium - many instances, but behavioral (not binary incompatible)

4. **Code Modifications**
   - **Areas to Review**:
     - Razor Pages (`Pages/*.cshtml.cs`) - URL generation, request handling
     - Minimal API endpoints (`Program.cs`) - `/api/employees/*`, `/api/departments/*`, `/api/status`
     - HTTP client usage for test execution
     - URI parsing for configuration management
   - **Docker Tooling**: After removing incompatible package, verify:
     - Dockerfile still present (`RESTRunner.Web/Dockerfile`
     - Docker build works: `docker build -t restrunner-web .`
     - Container runs correctly
   - **No Expected Code Changes**: Behavioral changes rarely require modifications

5. **Testing Strategy**
   - **Build Validation**:
     - Verify no errors after removing 3 packages
     - Ensure System.Net.Http and Regex work (framework-included)
   - **Smoke Test - Web UI**:
     1. Run: `dotnet run --project RESTRunner.Web\RESTRunner.Web.csproj`
     2. Navigate to `https://localhost:7001`
     3. Test pages:
        - Home page loads
        - Configuration create/list/edit
        - Collection import
        - Test execution
        - Results display
     4. Test minimal APIs:
        - Swagger UI: `https://localhost:7001/docs` (dev only)
        - `GET /api/status` (health check)
        - `GET /api/employees` (sample CRUD)
        - `GET /api/departments`
     5. File Storage Validation:
        - Configuration saves to `~/Data/configurations/{guid}.json`
        - Collection saves to `~/Data/collections/{guid}.json`
        - Results export to `~/Data/results/{guid}.csv`
   - **Docker Validation** (if Docker used):
     - Build image successfully
     - Run container exposes port 8080
     - App accessible in container

6. **Validation Checklist**
   - [ ] Project targets `net10.0`
   - [ ] System.Security.Cryptography.Xml updated to 10.0.1
   - [ ] Microsoft.VisualStudio.Azure.Containers.Tools.Targets removed
   - [ ] System.Net.Http and System.Text.RegularExpressions removed
   - [ ] Builds without errors
   - [ ] Builds without warnings (verify no implicit package warnings)
   - [ ] Web app starts on https://localhost:7001
   - [ ] Home page renders correctly (Bootswatch theme)
   - [ ] Configuration CRUD operations work
   - [ ] Postman collection import works
   - [ ] Test execution completes
   - [ ] Results display correctly
   - [ ] Minimal APIs respond correctly
   - [ ] Swagger UI works (dev environment)
   - [ ] File storage service reads/writes JSON
   - [ ] Docker build succeeds (if applicable)

**Dependencies Impact**: Depends on Domain, PostmanImport, Services.HttpClientRunner. No dependents (application entry point). Largest project requires most thorough testing.

**Special Attention**: 
- Incompatible package removal may affect Docker workflow
- 113 behavioral changes require comprehensive testing
- Razor Pages and minimal APIs both need validation

---

### 6. RESTRunner.Domain.Tests

**Current State**: net9.0, 2 dependencies, 476 LOC, 4 NuGet packages (test frameworks)

**Target State**: net10.0

**Risk Level**: ?? Low

**Package Dependencies**:
| Package | Current Version | Target Version | Action |
|---------|----------------|----------------|--------|
| coverlet.collector | 6.0.4 | 6.0.4 | ? Compatible (no update needed) |
| Microsoft.NET.Test.Sdk | 17.14.1 | 17.14.1 | ? Compatible (no update needed) |
| MSTest.TestAdapter | 3.10.4 | 3.10.4 | ? Compatible (no update needed) |
| MSTest.TestFramework | 3.10.4 | 3.10.4 | ? Compatible (no update needed) |

**Migration Steps**:

1. **Update Project File** (`RESTRunner.Domain.Tests\RESTRunner.Domain.Tests.csproj`)
   ```xml
   <!-- Change: -->
   <TargetFramework>net9.0</TargetFramework>
   <!-- To: -->
   <TargetFramework>net10.0</TargetFramework>
   ```

2. **Package Updates**
   - No updates required (all test packages compatible with .NET 10)

3. **Expected Breaking Changes**
   - **API Issues**: 0 (no behavioral changes detected in test project)
   - **Expected Impact**: None - test frameworks stable

4. **Code Modifications**
   - No code changes expected
   - Test assertions should work unchanged
   - MSTest attributes (`[TestClass]`, `[TestMethod]`) unchanged

5. **Testing Strategy**
   - **Execute Test Suite**: 
     ```
     dotnet test RESTRunner.Domain.Tests\RESTRunner.Domain.Tests.csproj
     ```
   - **Expected Outcome**: All tests pass (validates Domain and PostmanImport projects)
   - **Test Areas Covered**:
     - `CompareRunner` total test count calculation
     - Domain model integrity
     - Postman import functionality
   - **Failure Analysis**: If tests fail, distinguish:
     - Real bugs introduced by .NET 10 changes
     - Test expectations needing update for behavioral changes
     - Issues in tested code (Domain, PostmanImport)

6. **Validation Checklist**
   - [ ] Project targets `net10.0`
   - [ ] Builds without errors
   - [ ] Builds without warnings
   - [ ] All tests execute (no discovery failures)
   - [ ] All tests pass
   - [ ] Test coverage maintained
   - [ ] No new test infrastructure issues

**Dependencies Impact**: Depends on Domain and PostmanImport. No dependents (test project). Provides validation for foundation libraries.

**Role in Migration**: This project validates that Domain and PostmanImport work correctly in .NET 10. Success here confirms foundation is solid.

---

## Package Update Reference

### Common Package Updates (Affecting Multiple Projects)

| Package | Current | Target | Projects Affected | Update Reason |
|---------|---------|--------|-------------------|---------------|
| Microsoft.Extensions.Http | 9.0.9 | 10.0.1 | 2 projects (Console, Services.HttpClientRunner) | .NET 10 framework compatibility, aligned with framework version |

### Category-Specific Updates

**Microsoft Extensions Packages** (Console app - 4 packages):
- `Microsoft.Extensions.Hosting`: 9.0.9 ? 10.0.1 (host builder and DI)
- `Microsoft.Extensions.Http`: 9.0.9 ? 10.0.1 (HttpClientFactory)
- `System.Configuration.ConfigurationManager`: 9.0.9 ? 10.0.1 (configuration management)
- `System.Text.Json`: 9.0.9 ? 10.0.1 (JSON serialization)

**Services.HttpClientRunner** (2 packages):
- `Microsoft.Extensions.Http`: 9.0.9 ? 10.0.1 (HttpClientFactory)
- `Microsoft.Extensions.Logging.Abstractions`: 9.0.9 ? 10.0.1 (logging interfaces)

**RESTRunner.Web** (1 package update):
- `System.Security.Cryptography.Xml`: 9.0.9 ? 10.0.1 (XML cryptography)

### Package Removals

**Incompatible Packages** (RESTRunner.Web):
| Package | Version | Reason for Removal | Impact |
|---------|---------|-------------------|--------|
| Microsoft.VisualStudio.Azure.Containers.Tools.Targets | 1.22.1 | Incompatible with .NET 10 | Development-time Docker tooling; functionality may be in SDK or requires manual Docker config |

**Framework-Included Packages** (RESTRunner.Web):
| Package | Version | Reason for Removal | Impact |
|---------|---------|-------------------|--------|
| System.Net.Http | 4.3.4 | Functionality included in .NET 10 framework | No impact; framework provides HttpClient |
| System.Text.RegularExpressions | 4.3.1 | Functionality included in .NET 10 framework | No impact; framework provides Regex |

### No Update Required (Compatible Packages)

| Package | Version | Projects | Reason |
|---------|---------|----------|--------|
| CsvHelper | 33.1.0 | Console | Compatible with .NET 10 |
| coverlet.collector | 6.0.4 | Domain.Tests | Test coverage tool, .NET 10 compatible |
| FileHelpers | 3.5.2 | Domain | CSV/delimited file parsing, compatible |
| Microsoft.AspNet.WebApi.Client | 6.0.0 | Services.HttpClientRunner | Web API client library, compatible |
| Microsoft.NET.Test.Sdk | 17.14.1 | Domain.Tests | Test platform, .NET 10 compatible |
| MSTest.TestAdapter | 3.10.4 | Domain.Tests | MSTest runner, compatible |
| MSTest.TestFramework | 3.10.4 | Domain.Tests | MSTest framework, compatible |
| Newtonsoft.Json | 13.0.4 | PostmanImport, Web | JSON.NET, widely compatible |
| Swashbuckle.AspNetCore | 9.0.4 | Web | Swagger/OpenAPI, .NET 10 compatible |
| WebSpark.Bootswatch | 1.30.0 | Web | Bootswatch theme switcher, compatible |
| WebSpark.HttpClientUtility | 1.2.0 | Web | HTTP client decorators, compatible |

### Package Update Summary by Project

| Project | Updates | Removals | No Change | Total Packages |
|---------|---------|----------|-----------|----------------|
| RESTRunner.Domain | 0 | 0 | 1 | 1 |
| RESTRunner.PostmanImport | 0 | 0 | 1 | 1 |
| RESTRunner.Services.HttpClientRunner | 2 | 0 | 1 | 3 |
| RESTRunner (Console) | 4 | 0 | 1 | 5 |
| RESTRunner.Web | 1 | 3 | 6 | 10 |
| RESTRunner.Domain.Tests | 0 | 0 | 4 | 4 |
| **TOTALS** | **7** | **3** | **13** | **20** (unique) |

### Package Version Alignment

All Microsoft.Extensions.* packages align at version **10.0.1** across the solution:
- Console: 4 packages at 10.0.1
- Services.HttpClientRunner: 2 packages at 10.0.1

**Rationale**: Microsoft.Extensions packages typically move in lockstep with framework version. Using 10.0.1 ensures compatibility and avoids transitive dependency conflicts.

---

## Breaking Changes Catalog

### Overview

**Total API Issues**: 137 behavioral changes across 4 projects  
**Binary Incompatible**: 0  
**Source Incompatible**: 0  
**Behavioral Changes**: 137 (runtime behavior differences, no code changes required)

### Key Insight

?? **No Breaking Changes Requiring Code Modifications**

All 137 issues are **behavioral changes** — APIs that work differently at runtime but remain binary/source compatible. Code compiles unchanged; validation focuses on runtime behavior.

### Behavioral Changes by Category

#### 1. System.Uri Changes (79 instances total, 57.7%)

**Affected APIs**:
- `System.Uri` type usage - 59 instances
- `System.Uri(string)` constructor - 10 instances
- `System.Uri(string, UriKind)` constructor - 9 instances
- `System.Uri.TryCreate(string, UriKind, out Uri)` - 1 instance

**Projects Impacted**:
- **RESTRunner.Web**: Primary impact (large codebase, URL handling in Razor Pages/APIs)
- **RESTRunner.Services.HttpClientRunner**: Request URL construction
- **RESTRunner**: Console app URL configuration
- **RESTRunner.Domain**: Possible URI properties in models

**Behavioral Change Details**:
.NET 10 may have refinements to URI parsing, validation, or normalization. Common areas:
- Relative vs. absolute URI resolution
- URL encoding/decoding behavior
- Query string parsing
- Fragment handling
- Internationalized domain names (IDN)

**Validation Strategy**:
- Test URL template substitution (`{{url}}/path` ? relative path in Postman import)
- Validate instance base URL parsing (Local, Demo, Production endpoints)
- Test request path construction in ExecuteRunnerService
- Compare .NET 9 vs .NET 10 output for same URLs

**Expected Impact**: Low - URI behavior typically backward-compatible; changes are edge case refinements

---

#### 2. System.Net.Http.HttpContent Changes (54 instances total, 39.4%)

**Affected APIs**:
- `System.Net.Http.HttpContent` type usage - 53 instances
- `HttpContent.ReadAsStreamAsync()` method - 1 instance

**Projects Impacted**:
- **RESTRunner.Web**: HTTP response handling (largest project)
- **RESTRunner.Services.HttpClientRunner**: Core execution engine, response parsing

**Behavioral Change Details**:
.NET 10 may have changes to:
- Content reading (stream vs. string vs. byte array)
- Buffering behavior
- Encoding detection
- Disposal timing
- Exception handling

**Validation Strategy**:
- Test response body parsing in ExecuteRunnerService
- Validate response hashing (used for result comparison)
- Test different content types (JSON, XML, plain text, binary)
- Monitor stream disposal and memory usage
- Compare .NET 9 vs .NET 10 response parsing results

**Expected Impact**: Low - HttpContent API mature and stable; changes likely internal optimizations

---

#### 3. Dependency Injection Changes (3 instances total, 2.2%)

**Affected APIs**:
- `HttpClientFactoryServiceCollectionExtensions.AddHttpClient(IServiceCollection)` - 2 instances
- `ConsoleLoggerExtensions.AddConsole(ILoggingBuilder)` - 1 instance
- `Microsoft.Extensions.Hosting.HostBuilder` type - 1 instance

**Projects Impacted**:
- **RESTRunner**: Console app DI setup in Program.cs
- **RESTRunner.Web**: Web app DI registration in Program.cs

**Behavioral Change Details**:
Possible changes to:
- HttpClientFactory registration and configuration
- HttpClient instance lifetime and disposal
- Console logger formatting or output
- Host builder configuration

**Validation Strategy**:
- Verify HttpClient instances created correctly
- Test named HttpClient retrieval (if used)
- Confirm console logging displays properly
- Validate host builder startup and shutdown

**Expected Impact**: Minimal - DI APIs extremely stable; behavior rarely changes

---

### Breaking Changes by Project

| Project | Behavioral Changes | Primary APIs Affected | Validation Focus |
|---------|-------------------|----------------------|------------------|
| RESTRunner.Web | 113 (82.5%) | System.Uri (59), HttpContent (53) | Web UI, minimal APIs, HTTP requests |
| Services.HttpClientRunner | 19 (13.9%) | System.Uri, HttpContent | Request execution, response parsing |
| RESTRunner (Console) | 3 (2.2%) | HostBuilder, AddHttpClient, AddConsole | DI setup, logging |
| RESTRunner.Domain | 2 (1.5%) | System.Uri (likely) | Domain model URI properties |
| PostmanImport | 0 | None | No behavioral changes |
| Domain.Tests | 0 | None | No behavioral changes |

### Known Breaking Changes from .NET 9 ? .NET 10

**Note**: Specific .NET 10 breaking changes documentation available at:
https://learn.microsoft.com/en-us/dotnet/core/compatibility/10.0

**General Categories to Monitor**:
1. **Networking**: HttpClient, Uri, DNS resolution behavior
2. **Serialization**: System.Text.Json defaults and behavior
3. **Configuration**: Hosting, logging, options patterns
4. **Performance**: Optimizations may change observable behavior

**Recommended**: Review official breaking changes list before committing upgrade.

### Mitigation Strategies

**For Behavioral Changes**:
1. **Automated Testing**: Run MSTest suite (RESTRunner.Domain.Tests)
2. **Manual Smoke Tests**:
   - Console app: Execute sample Postman collection, verify results
   - Web app: Test UI navigation, API execution, result display
3. **Comparison Testing**: Run same scenarios on .NET 9 and .NET 10, compare outputs
4. **Monitoring**: Watch for unexpected behavior in production after deployment

**For Code Incompatibilities** (none expected):
- If compilation errors occur, consult .NET 10 migration guide
- Update obsolete API usage to recommended alternatives
- Add `#if` conditional compilation if needed for multi-targeting

### Testing Checklist

**Behavioral Change Validation**:
- [ ] URI parsing works correctly (absolute, relative, templates)
- [ ] HTTP request execution succeeds (all verbs: GET, POST, PUT, DELETE, PATCH)
- [ ] Response body parsing accurate (JSON, XML, text)
- [ ] Response hashing consistent (for result comparison)
- [ ] Statistics calculation correct (percentiles, counts)
- [ ] Console logging displays properly
- [ ] HttpClientFactory DI resolution works
- [ ] No unexpected exceptions or errors
- [ ] Performance acceptable (no regressions)

**If Behavioral Differences Found**:
1. Document difference (what changed)
2. Assess impact (does it break functionality?)
3. Decide: Accept new behavior, adjust code, or report issue
4. Update tests if needed to reflect .NET 10 behavior

````````markdown
## Testing & Validation Strategy

### Multi-Level Testing Approach

Testing strategy aligns with All-At-Once migration: comprehensive validation after atomic upgrade completes.

---

### Level 1: Build Validation

**Objective**: Verify all projects compile and restore successfully

**Steps**:
1. Restore NuGet packages:
   ```
   dotnet restore RESTRunner.sln
   ```
   
2. Build entire solution:
   ```
   dotnet build RESTRunner.sln --configuration Release
   ```

**Success Criteria**:
- ? All 6 projects restore without errors
- ? All 6 projects build without errors
- ? Zero compilation errors
- ?? Goal: Zero warnings (acceptable: warnings about deprecated packages)

**Failure Response**:
- Review compilation errors
- Consult Breaking Changes Catalog
- Address API signature changes or missing types
- Rebuild after fixes

---

### Level 2: Automated Testing

**Objective**: Execute MSTest suite to validate domain logic and imports

**Steps**:
1. Run test project:
   ```
   dotnet test RESTRunner.Domain.Tests\RESTRunner.Domain.Tests.csproj --configuration Release
   ```

2. Review test results:
   - Total tests executed
   - Passed count
   - Failed count (with details)
   - Skipped count

**Success Criteria**:
- ? All tests execute (no discovery failures)
- ? All tests pass
- ? No new test failures vs. .NET 9 baseline

**Test Coverage Areas**:
- `CompareRunner` model calculations (GetTotalTestCount)
- Domain model integrity (serialization, properties)
- Postman collection import (JSON parsing, URL template conversion)

**Failure Response**:
- Analyze each failing test
- Distinguish:
  - Real bugs introduced by .NET 10 changes ? Fix implementation
  - Test expectations needing update for behavioral changes ? Update test
  - Test infrastructure issues ? Fix test setup
- Rerun tests after fixes

---

### Level 3: Console Application Smoke Testing

**Objective**: Validate end-to-end execution through console app

**Prerequisites**:
- Sample `collection.json` (Postman v2.1.0 format) in RESTRunner output directory
- Target API endpoints accessible (or mock server)

**Steps**:
1. Navigate to console app:
   ```
   cd RESTRunner
   ```

2. Run application:
   ```
   dotnet run --configuration Release
   ```

3. Observe execution:
   - Postman collection import
   - Request execution against instances
   - Statistics calculation
   - CSV output generation
   - Console report display

**Success Criteria**:
- ? Application starts without exceptions
- ? Collection imports successfully
- ? Requests execute (100 iterations × instances × users × requests)
- ? Statistics display correctly:
  - Emoji indicators (? 2xx Success, ?? 4xx Client Error, ? 5xx Server Error)
  - Performance metrics (P50, P75, P90, P95, P99, P99.9 percentiles)
  - Breakdown by HTTP method, status code, instance, user
- ? CSV output generated at `c:\test\RESTRunner.csv`
- ? No unhandled exceptions
- ? No unexpected errors in output

**Test Scenarios**:
1. **Basic execution**: Single instance, single user, simple GET requests
2. **Multi-instance**: Multiple API endpoints (Local, Demo, Production)
3. **Multi-user**: Multiple user contexts with property substitution
4. **Mixed verbs**: GET, POST, PUT, DELETE, PATCH requests
5. **Template substitution**: `{{encoded_user_name}}`, `{{property_key}}` replacement

**Failure Response**:
- Examine console output for errors
- Review CSV for unexpected results
- Compare .NET 9 vs .NET 10 execution results
- Debug specific request failures
- Validate URI parsing and HTTP execution

---

### Level 4: Web Application Smoke Testing

**Objective**: Validate Razor Pages, minimal APIs, and web infrastructure

**Prerequisites**:
- .NET 10 SDK installed
- HTTPS certificate trusted (for localhost development)

**Steps**:

#### 4.1 Application Startup
1. Run web application:
   ```
   cd RESTRunner.Web
   dotnet run --configuration Release
   ```

2. Navigate to: `https://localhost:7001`

3. Verify:
   - Application starts without exceptions
   - Home page loads (Bootswatch theme renders)
   - No JavaScript errors in browser console

#### 4.2 Razor Pages Validation
Test key pages:

1. **Home Page** (`/`)
   - [ ] Page loads
   - [ ] Bootswatch theme applied (navbar, buttons, colors)
   - [ ] Navigation menu works

2. **Configuration Management** (`/Configuration/*`)
   - [ ] List configurations page loads
   - [ ] Create new configuration form works
   - [ ] Edit configuration loads existing data
   - [ ] Delete configuration removes entry
   - [ ] Configuration saved to `~/Data/configurations/{guid}.json`

3. **Collection Management** (`/Collection/*`)
   - [ ] Import Postman collection page loads
   - [ ] File upload works
   - [ ] Collection parsed and displayed
   - [ ] Collection saved to `~/Data/collections/{guid}.json`

4. **Test Execution** (`/Runner/*`)
   - [ ] Execute test page loads
   - [ ] Configuration selection works
   - [ ] Test execution starts
   - [ ] Progress indication displays
   - [ ] Results page shows statistics

5. **Results Display**
   - [ ] Results table loads
   - [ ] CSV export works (`~/Data/results/{guid}.csv`)
   - [ ] Statistics formatted correctly

#### 4.3 Minimal API Validation
Test API endpoints (use Swagger UI or curl):

1. **Health Check**:
   ```
   GET https://localhost:7001/api/status
   ```
   - [ ] Returns 200 OK
   - [ ] JSON response with status

2. **Sample CRUD Operations**:
   ```
   GET https://localhost:7001/api/employees
   GET https://localhost:7001/api/departments
   ```
   - [ ] Endpoints respond
   - [ ] JSON data returned

3. **Swagger UI** (Development only):
   - [ ] Navigate to `https://localhost:7001/docs`
   - [ ] Swagger page loads
   - [ ] API documentation displays
   - [ ] "Try it out" functionality works

#### 4.4 File Storage Validation
- [ ] `~/Data/configurations/` directory used
- [ ] `~/Data/collections/` directory used
- [ ] `~/Data/results/` directory used
- [ ] JSON serialization/deserialization works
- [ ] File locking handled correctly

**Success Criteria**:
- ? All Razor Pages render correctly
- ? All forms submit and process data
- ? Minimal APIs respond correctly
- ? File storage reads/writes work
- ? No exceptions in application logs
- ? No JavaScript errors in browser console
- ? Theme switching works (WebSpark.Bootswatch)
- ? HTTP requests execute through IExecutionService

**Failure Response**:
- Check application logs for exceptions
- Inspect browser console for JavaScript errors
- Verify file paths and permissions
- Review System.Uri and HttpContent usage in web project
- Compare .NET 9 vs .NET 10 behavior

---

### Level 5: Docker Validation (Optional)

**Objective**: Ensure Docker support still works after removing incompatible package

**Prerequisites**:
- Docker Desktop installed
- Dockerfile present in `RESTRunner.Web/`

**Steps**:
1. Build Docker image:
   ```
   cd RESTRunner.Web
   docker build -t restrunner-web:net10 .
   ```

2. Run container:
   ```
   docker run -d -p 8080:8080 --name restrunner-test restrunner-web:net10
   ```

3. Test application:
   ```
   curl http://localhost:8080/api/status
   ```

4. Cleanup:
   ```
   docker stop restrunner-test
   docker rm restrunner-test
   ```

**Success Criteria**:
- ? Docker image builds successfully
- ? Container starts without errors
- ? Application accessible on port 8080
- ? Health check endpoint responds

**Failure Response**:
- If build fails: Verify Dockerfile references correct SDK/runtime versions
- If runtime fails: Check container logs (`docker logs restrunner-test`)
- If incompatible package needed: Research updated version or manual Docker config

---

### Level 6: Performance Validation

**Objective**: Ensure no performance regressions from .NET 9 to .NET 10

**Steps**:
1. Run console app with consistent Postman collection
2. Note execution time and statistics
3. Compare to .NET 9 baseline (if available)

**Success Criteria**:
- ? Execution time comparable (within 10% variance)
- ? Memory usage acceptable
- ? No unexpected performance degradation

**Metrics to Monitor**:
- Total execution time
- Requests per second
- P95/P99 response times
- Memory usage (dotnet-counters or profiler)

---

### Testing Phase Summary

| Level | Scope | Automation | Estimated Effort |
|-------|-------|-----------|------------------|
| 1. Build Validation | Solution-wide | ? Automated | ?? Minimal (< 5 min) |
| 2. Automated Tests | Unit tests | ? Automated | ?? Low (5-10 min) |
| 3. Console Smoke Test | End-to-end | ?? Manual | ?? Medium (15-30 min) |
| 4. Web Smoke Test | UI + APIs | ?? Manual | ?? Medium (30-45 min) |
| 5. Docker Validation | Containerization | ?? Manual (optional) | ?? Low (10-15 min) |
| 6. Performance Check | Benchmarking | ?? Manual (optional) | ?? Low (10-15 min) |

**Total Testing Effort**: ~1.5-2 hours (assuming no failures)

**Critical Path**: Levels 1-4 mandatory before declaring success. Levels 5-6 optional but recommended.

---

### Regression Testing

**Behavioral Change Focus Areas**:
1. **URI Parsing**:
   - Test absolute vs. relative URLs
   - Test URL template substitution
   - Test query string handling
   - Test special characters in URLs

2. **HTTP Execution**:
   - Test all HTTP verbs (GET, POST, PUT, DELETE, PATCH, HEAD, OPTIONS)
   - Test request body serialization
   - Test response body parsing
   - Test different content types

3. **Statistics**:
   - Test percentile calculations (P50, P75, P90, P95, P99, P99.9)
   - Test request/response hash generation
   - Test status code categorization

4. **Dependency Injection**:
   - Test HttpClientFactory resolution
   - Test IExecuteRunner service injection
   - Test IFileStorageService injection

**Comparison Testing**:
If possible, run same scenarios on .NET 9 and .NET 10, compare:
- Request execution results
- Response hashes
- Statistics calculations
- Output formatting

---

### Test Failure Escalation

**Minor Failures** (acceptable):
- New warnings (package deprecation)
- Minor formatting differences
- Performance variance within 10%

**Major Failures** (must fix):
- Compilation errors
- Test suite failures
- Application crashes
- Functional regressions
- Data corruption

**Escalation Path**:
1. **Level 1-2 Failures**: Block upgrade, address immediately
2. **Level 3-4 Failures**: Investigate, attempt fixes, consider rollback if extensive
3. **Level 5-6 Failures**: Document, proceed with caution, fix in follow-up

---
