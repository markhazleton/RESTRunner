# GitHub Issue Templates

## Issue 1: Implement MSTest v4 Analyzer Suggestions

### Title
Implement MSTest v4 code quality analyzer suggestions

### Labels
- `enhancement`
- `testing`
- `code-quality`
- `good first issue`

### Description

## Overview
MSTest v4 introduces enhanced code analyzers that provide 7 actionable suggestions to improve test code quality. These are currently appearing as warnings but represent opportunities to enhance test maintainability and diagnostic clarity.

## Current Status
- MSTest packages successfully upgraded to v4.0.2
- All 21 tests passing (100% success rate)
- 7 analyzer warnings present (not errors)

## Analyzer Warnings to Address

### 1. MSTEST0001: Configure Test Parallelization (1 instance)
**Location**: Assembly level  
**Current**: Tests run sequentially by default (no explicit configuration)  
**Recommendation**: Add explicit parallelization configuration

**Suggested Fix**:
```csharp
// Add to RESTRunner.Domain.Tests/GlobalUsings.cs or AssemblyInfo.cs
[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]
```

**Benefit**: Explicitly documents parallelization strategy, enables future optimization

---

### 2. MSTEST0017: Fix Assertion Argument Order (3 instances)

**Locations**:
- `StrongDictionaryTests.cs` line 59
- `StrongDictionaryTests.cs` line 72
- `String_ExtensionsTests.cs` line 55

**Issue**: Expected and actual arguments are swapped, leading to confusing error messages

**Example of Current (Incorrect) Pattern**:
```csharp
// WRONG - arguments swapped
Assert.AreEqual(actualValue, expectedValue);
```

**Correct Pattern**:
```csharp
// CORRECT - expected first, actual second
Assert.AreEqual(expectedValue, actualValue);
```

**Benefit**: Better error messages when tests fail - clearly shows expected vs actual values

---

### 3. MSTEST0037: Use Specific Assertion Methods (3 instances)

**Locations**:
- `StrongDictionaryTests.cs` line 18: Use `Assert.HasCount` instead of `Assert.AreEqual`
- `StrongDictionaryTests.cs` line 100: Use `Assert.IsNotNull` instead of `Assert.AreNotEqual`
- `StrongDictionaryTests.cs` line 110: Use `Assert.IsNotNull` instead of `Assert.AreNotEqual`

**Current Pattern**:
```csharp
// Less specific
Assert.AreEqual(5, collection.Count);
Assert.AreNotEqual(null, obj);
```

**Improved Pattern**:
```csharp
// More specific and descriptive
Assert.HasCount(5, collection);
Assert.IsNotNull(obj);
```

**Benefit**: More descriptive test failures, clearer intent, better IDE integration

---

## Implementation Plan

### Phase 1: Assembly-Level Configuration (5 minutes)
1. Create or update `AssemblyInfo.cs` in RESTRunner.Domain.Tests
2. Add `[assembly: Parallelize(...)]` attribute
3. Document parallelization strategy in comments
4. Build and verify no test failures

### Phase 2: Fix Assertion Argument Order (10-15 minutes)
1. Open `StrongDictionaryTests.cs`
   - Line 59: Swap arguments in `Assert.AreEqual`
   - Line 72: Swap arguments in `Assert.AreEqual`
2. Open `String_ExtensionsTests.cs`
   - Line 55: Swap arguments in `Assert.AreEqual`
3. Run tests to verify all still pass
4. Intentionally break a test to verify error message improvement

### Phase 3: Update to Specific Assertions (10-15 minutes)
1. Open `StrongDictionaryTests.cs`
   - Line 18: Replace `Assert.AreEqual(count, dict.Count)` with `Assert.HasCount(count, dict)`
   - Line 100: Replace `Assert.AreNotEqual(null, obj)` with `Assert.IsNotNull(obj)`
   - Line 110: Replace `Assert.AreNotEqual(null, obj)` with `Assert.IsNotNull(obj)`
2. Run tests to verify all still pass
3. Review improved test readability

### Phase 4: Validation (5 minutes)
1. Build solution: `dotnet build --configuration Release`
2. Run all tests: `dotnet test`
3. Verify 0 MSTEST warnings remain
4. Commit changes with descriptive message

---

## Acceptance Criteria
- [ ] All 7 MSTest v4 analyzer warnings resolved
- [ ] All 21 tests still passing (100%)
- [ ] Build produces 0 MSTEST warnings
- [ ] Test failure messages are clearer (verify with intentional break)
- [ ] Code committed with clear commit message
- [ ] No regressions in test functionality

---

## Benefits
1. **Better Diagnostics**: Clearer test failure messages
2. **Code Clarity**: More explicit and descriptive assertions
3. **Future Performance**: Foundation for test parallelization optimization
4. **Best Practices**: Aligns with MSTest v4 recommendations
5. **Maintainability**: Easier for future developers to understand test intent

---

## Estimated Effort
**Total Time**: 30-60 minutes  
**Complexity**: Low  
**Risk**: Very Low (all tests currently passing)

---

## Related
- PR: #X (Upgrade to .NET 10 with package optimization)
- MSTest v4 Documentation: https://learn.microsoft.com/dotnet/core/testing/mstest-analyzers

---

## Notes
- All warnings are suggestions, not errors
- Tests function correctly without these changes
- Changes improve code quality and maintainability
- Good first issue for contributors

---

================================================================================

## Issue 2: Upgrade Swashbuckle.AspNetCore to v10

### Title
Upgrade Swashbuckle.AspNetCore to v10 with Microsoft.OpenApi v2.x compatibility

### Labels
- `enhancement`
- `dependencies`
- `breaking-change`
- `web`

### Description

## Overview
Upgrade Swashbuckle.AspNetCore from v9.0.4 to v10.1.0 to leverage latest Swagger/OpenAPI features, security updates, and improved .NET 10 integration. This upgrade requires code refactoring due to breaking changes in the Microsoft.OpenApi v2.x dependency.

## Current Status
- **Current Version**: Swashbuckle.AspNetCore v9.0.4
- **Target Version**: Swashbuckle.AspNetCore v10.1.0
- **Status**: Deferred from .NET 10 upgrade due to breaking changes
- **Reason**: Microsoft.OpenApi v2.x has API incompatibilities

## Breaking Changes

### Microsoft.OpenApi v2.x Changes
Swashbuckle v10 requires Microsoft.OpenApi >= 2.3.0, which has several breaking changes:

1. **Namespace Structure**: Potential changes in type locations
2. **API Modifications**: `OpenApiInfo` and related types may have different initialization patterns
3. **Configuration API**: Swagger configuration methods may have changed

### Known Issues from .NET 10 Upgrade Attempt

**Error Encountered**:
```
CS0234: The type or namespace name 'Models' does not exist in the namespace 'Microsoft.OpenApi'
```

**Affected File**: `RESTRunner.Web\Program.cs`

**Current Code** (Swashbuckle v9):
```csharp
using Microsoft.OpenApi.Models;

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RESTRunner API", Version = "v1" });
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
```

---

## Research Phase (Before Implementation)

### Tasks
1. **Review Breaking Changes**
   - [ ] Read Swashbuckle v10 release notes
   - [ ] Review Microsoft.OpenApi v2.x migration guide
   - [ ] Identify all affected code locations
   - [ ] Document required API changes

2. **Analyze Current Usage**
   - [ ] Audit all `Microsoft.OpenApi` usages in codebase
   - [ ] Document current swagger configuration
   - [ ] Identify custom swagger configurations
   - [ ] Check for any swagger middleware customizations

3. **Create Migration Plan**
   - [ ] Document step-by-step upgrade approach
   - [ ] Identify testing requirements
   - [ ] Plan rollback strategy
   - [ ] Estimate effort and timeline

---

## Implementation Plan (After Research)

### Phase 1: Update Packages (15 minutes)
1. Update `Swashbuckle.AspNetCore` to v10.1.0
2. Update `Microsoft.OpenApi` to v2.3.0 (or latest)
3. Resolve any package dependency conflicts
4. Document any additional required packages

### Phase 2: Code Refactoring (60-120 minutes)
1. **Update using statements** in `Program.cs`
   - Fix namespace references for Microsoft.OpenApi v2.x
   - Update any deprecated imports

2. **Refactor OpenApiInfo initialization**
   - Update to Microsoft.OpenApi v2.x syntax
   - Verify all properties are correctly set

3. **Update Swagger configuration**
   - Modernize `AddSwaggerGen` configuration
   - Update `TagActionsBy` if API changed
   - Update `DocInclusionPredicate` if API changed
   - Verify custom configurations still work

4. **Update middleware configuration** (if needed)
   - Review `UseSwagger()` and `UseSwaggerUI()` calls
   - Update any custom middleware options

### Phase 3: Testing & Validation (30-60 minutes)

**Build Validation**:
- [ ] Solution builds with 0 errors
- [ ] No new warnings introduced
- [ ] All existing tests pass (21/21)

**Swagger UI Validation**:
- [ ] Start web application: `dotnet run` in RESTRunner.Web
- [ ] Navigate to https://localhost:7001/swagger
- [ ] Verify Swagger UI loads correctly
- [ ] Test all API endpoints in Swagger UI
- [ ] Verify endpoint grouping (Employee, Department, Other tags)
- [ ] Test "Try it out" functionality for sample endpoints

**API Documentation Validation**:
- [ ] Verify OpenAPI spec generates correctly
- [ ] Check endpoint descriptions
- [ ] Verify request/response schemas
- [ ] Test parameter documentation
- [ ] Verify authentication documentation (if any)

**Manual Web App Testing**:
- [ ] Home page loads
- [ ] All Razor Pages render correctly
- [ ] Minimal APIs functional (/api/employees, /api/departments, /api/status)
- [ ] No console errors
- [ ] Theme switching still works

### Phase 4: Documentation (15 minutes)
1. Update code comments if API usage changed
2. Document any new Swashbuckle v10 features utilized
3. Update developer documentation if needed
4. Create migration notes for future reference

---

## Testing Checklist

### Automated Testing
- [ ] All unit tests pass: `dotnet test`
- [ ] Build successful: `dotnet build --configuration Release`
- [ ] No compilation errors
- [ ] No new warnings (except intentional ones)

### Manual Testing - Swagger UI
- [ ] Swagger UI accessible at `/swagger`
- [ ] API documentation displays correctly
- [ ] All endpoints visible in groups
- [ ] Endpoint descriptions accurate
- [ ] Request/response models display correctly
- [ ] "Try it out" functionality works
- [ ] Examples populate correctly

### Manual Testing - Web Application
- [ ] Web app starts without errors
- [ ] All pages load correctly
- [ ] Minimal APIs respond correctly
- [ ] No JavaScript console errors
- [ ] Performance unchanged

### Regression Testing
- [ ] Configuration CRUD operations work
- [ ] Test execution functionality intact
- [ ] Results display correctly
- [ ] Theme switching functional

---

## Acceptance Criteria
- [ ] Swashbuckle.AspNetCore upgraded to v10.1.0
- [ ] Microsoft.OpenApi upgraded to v2.3.0+
- [ ] All code compiles with 0 errors
- [ ] All tests passing (21/21, 100%)
- [ ] Swagger UI renders correctly
- [ ] API documentation accurate and complete
- [ ] No regressions in web app functionality
- [ ] Documentation updated
- [ ] Changes committed with clear message

---

## Benefits

### Security
- Latest security patches in Swashbuckle v10
- Security improvements in Microsoft.OpenApi v2.x
- Reduced vulnerability exposure

### Features
- Latest OpenAPI 3.0 spec support
- Improved .NET 10 integration
- Enhanced Swagger UI features
- Better performance optimizations

### Maintainability
- Keeps dependencies current
- Aligns with latest best practices
- Easier future upgrades
- Better tooling support

---

## Estimated Effort
**Total Time**: 2-4 hours  
**Complexity**: Medium  
**Risk**: Medium (breaking changes require code refactoring)

### Breakdown
- Research & Planning: 30-60 minutes
- Package Updates: 15 minutes
- Code Refactoring: 60-120 minutes
- Testing & Validation: 30-60 minutes
- Documentation: 15 minutes

---

## Rollback Plan

If issues arise:

1. **Immediate Rollback**:
```bash
cd RESTRunner.Web
dotnet add package Swashbuckle.AspNetCore --version 9.0.4
dotnet remove package Microsoft.OpenApi
git checkout Program.cs  # Restore original
```

2. **Verification**:
```bash
dotnet build --configuration Release
dotnet run
# Verify swagger at https://localhost:7001/swagger
```

---

## Resources

### Documentation
- Swashbuckle v10 Release Notes: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/releases/tag/v10.0.0
- Microsoft.OpenApi Documentation: https://github.com/microsoft/OpenAPI.NET
- OpenAPI Specification: https://swagger.io/specification/

### Related Issues
- PR: #X (Upgrade to .NET 10 with package optimization)
- Issue: #Y (Implement MSTest v4 analyzer suggestions)

---

## Notes
- Deferred during .NET 10 upgrade to maintain focus
- Current version (v9.0.4) is stable and functional
- No urgent need, but recommended for long-term maintenance
- Consider coordinating with other dependency updates

---

## Developer Tips

### Before Starting
1. Create feature branch: `git checkout -b upgrade-swashbuckle-v10`
2. Ensure clean working directory
3. Run tests to establish baseline
4. Take note of current swagger UI behavior

### During Development
1. Make small, incremental changes
2. Build frequently to catch errors early
3. Test swagger UI after each change
4. Document any unexpected API changes

### After Completion
1. Full regression test suite
2. Review all swagger-related code
3. Update this issue with lessons learned
4. Create PR with detailed description

---

## Success Metrics
- ? Zero compilation errors
- ? Zero test failures
- ? Swagger UI fully functional
- ? All API documentation accurate
- ? No performance regression
- ? Code quality maintained or improved

---

## Questions to Resolve During Research
1. What are all the breaking changes in Microsoft.OpenApi v2.x?
2. Are there any new Swashbuckle v10 features we should leverage?
3. Does the TagActionsBy API still work the same way?
4. Are there any performance improvements we can utilize?
5. Should we update our swagger UI theme/styling?

---
