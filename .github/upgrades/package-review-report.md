# NuGet Package Review Report
**Date**: 2025-12-23  
**Solution**: RESTRunner  
**Target Framework**: .NET 10.0  

---

## Executive Summary

| Metric | Count | Status |
|--------|-------|--------|
| Total Unique Packages | 17 | - |
| Outdated Packages | 6 | ?? Updates Available |
| Potentially Unused Packages | 2 | ?? Framework-Included |
| Security Vulnerabilities | 0 | ? Clean |
| Latest Versions | 11 | ? Up-to-date |

---

## Package Analysis by Project

### 1. RESTRunner.Domain (1 package)

| Package | Current | Latest | Status | Recommendation |
|---------|---------|--------|--------|----------------|
| FileHelpers | 3.5.2 | 3.5.2 | ? Latest | Keep - Required for CSV/delimited file parsing |

**Analysis**: Clean. FileHelpers is actively used for domain data parsing.

---

### 2. RESTRunner.PostmanImport (1 package)

| Package | Current | Latest | Status | Recommendation |
|---------|---------|--------|--------|----------------|
| Newtonsoft.Json | 13.0.4 | 13.0.4 | ? Latest | Keep - Required for Postman v2.1.0 collection parsing |

**Analysis**: Clean. Newtonsoft.Json is essential for Postman collection deserialization. While System.Text.Json is available in .NET 10, Newtonsoft.Json is the standard for Postman collections and should be retained.

---

### 3. RESTRunner.Services.HttpClientRunner (3 packages)

| Package | Current | Latest | Status | Recommendation |
|---------|---------|--------|--------|----------------|
| Microsoft.AspNet.WebApi.Client | 6.0.0 | 6.0.0 | ? Latest | Keep - Required for HTTP client utilities |
| Microsoft.Extensions.Http | 10.0.1 | 10.0.1 | ? Latest | Keep - Required for HttpClientFactory |
| Microsoft.Extensions.Logging.Abstractions | 10.0.1 | 10.0.1 | ? Latest | Keep - Required for ILogger interfaces |

**Analysis**: Clean. All packages are latest versions and actively used for HTTP execution engine.

---

### 4. RESTRunner (Console App) (5 packages)

| Package | Current | Latest | Status | Recommendation |
|---------|---------|--------|--------|----------------|
| CsvHelper | 33.1.0 | 33.1.0 | ? Latest | Keep - Required for CSV output generation |
| Microsoft.Extensions.Hosting | 10.0.1 | 10.0.1 | ? Latest | Keep - Required for host builder/DI |
| Microsoft.Extensions.Http | 10.0.1 | 10.0.1 | ? Latest | Keep - Required for HttpClientFactory |
| System.Configuration.ConfigurationManager | 10.0.1 | 10.0.1 | ? Latest | Keep - Required for app.config access |
| System.Text.Json | 10.0.1 | 10.0.1 | ?? Framework | **Consider Removing** - See Warning NU1510 |

**Analysis**: 
- **Warning NU1510**: `System.Text.Json` triggers warning indicating it may be redundant (included in framework)
- **Recommendation**: Test removal - .NET 10 includes System.Text.Json in the framework
- **Risk**: Low - Should be safe to remove as it's part of the shared framework

---

### 5. RESTRunner.Web (5 packages)

| Package | Current | Latest | Status | Recommendation |
|---------|---------|--------|--------|----------------|
| Newtonsoft.Json | 13.0.4 | 13.0.4 | ? Latest | Keep - Used for JSON operations |
| Swashbuckle.AspNetCore | 9.0.4 | **10.1.0** | ?? Outdated | **Update to 10.1.0** |
| System.Security.Cryptography.Xml | 10.0.1 | 10.0.1 | ?? Framework | **Consider Removing** - See Warning NU1510 |
| WebSpark.Bootswatch | 1.30.0 | **1.34.0** | ?? Outdated | **Update to 1.34.0** |
| WebSpark.HttpClientUtility | 1.2.0 | **2.1.2** | ?? Outdated | **Update to 2.1.2** (Major version!) |

**Analysis**: 
- **3 outdated packages** need updates
- **Warning NU1510**: `System.Security.Cryptography.Xml` may be redundant
- **Breaking Change Risk**: WebSpark.HttpClientUtility has a major version bump (1.2.0 ? 2.1.2), review changelog before updating

---

### 6. RESTRunner.Domain.Tests (4 packages)

| Package | Current | Latest | Status | Recommendation |
|---------|---------|--------|--------|----------------|
| Microsoft.NET.Test.Sdk | 17.14.1 | **18.0.1** | ?? Outdated | **Update to 18.0.1** |
| MSTest.TestAdapter | 3.10.4 | **4.0.2** | ?? Outdated | **Update to 4.0.2** (Major version!) |
| MSTest.TestFramework | 3.10.4 | **4.0.2** | ?? Outdated | **Update to 4.0.2** (Major version!) |
| coverlet.collector | 6.0.4 | 6.0.4 | ? Latest | Keep - Required for code coverage |

**Analysis**: 
- **3 outdated test packages** 
- **Breaking Change Risk**: MSTest v4 is a major version update, may have breaking changes
- **Recommendation**: Update cautiously and re-run all tests

---

## Recommended Actions

### Priority 1: Remove Potentially Unused Packages (Low Risk)

#### Action 1.1: Remove System.Text.Json from RESTRunner.csproj
```xml
<!-- REMOVE THIS LINE: -->
<PackageReference Include="System.Text.Json" Version="10.0.1" />
```
**Rationale**: NU1510 warning indicates this is included in .NET 10 framework  
**Test**: Build and run console app, verify JSON serialization still works  
**Risk**: Very Low - Framework-included package

#### Action 1.2: Remove System.Security.Cryptography.Xml from RESTRunner.Web.csproj
```xml
<!-- REMOVE THIS LINE: -->
<PackageReference Include="System.Security.Cryptography.Xml" Version="10.0.1" />
```
**Rationale**: NU1510 warning indicates may not be needed  
**Test**: Build web app, verify no compilation errors  
**Risk**: Low - If actually used, will get compilation error immediately

---

### Priority 2: Update Outdated Packages (Medium Risk)

#### Action 2.1: Update RESTRunner.Web Packages
```xml
<!-- UPDATE THESE: -->
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.0" />
<PackageReference Include="WebSpark.Bootswatch" Version="1.34.0" />
<PackageReference Include="WebSpark.HttpClientUtility" Version="2.1.2" />
```
**Rationale**: Security fixes, bug fixes, new features  
**Risk**: Medium for WebSpark.HttpClientUtility (major version bump)  
**Test**: Full web app smoke test, especially HTTP client usage

#### Action 2.2: Update RESTRunner.Domain.Tests Packages
```xml
<!-- UPDATE THESE: -->
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.0.1" />
<PackageReference Include="MSTest.TestAdapter" Version="4.0.2" />
<PackageReference Include="MSTest.TestFramework" Version="4.0.2" />
```
**Rationale**: MSTest v4 with improved .NET 10 support  
**Risk**: Medium (major version update)  
**Test**: Run full test suite: `dotnet test`

---

### Priority 3: Research Breaking Changes (Before Updates)

#### MSTest v3 ? v4 Breaking Changes
**Before updating**, review:
- MSTest v4 Release Notes: https://github.com/microsoft/testfx/releases
- Breaking changes in test attributes
- Changes in test lifecycle
- Assert API modifications

#### WebSpark.HttpClientUtility v1 ? v2 Breaking Changes
**Before updating**, review:
- Check WebSpark.HttpClientUtility changelog
- API signature changes
- Breaking changes in HTTP decorators
- Namespace changes

---

## Unused Package Detection Results

### Packages Confirmed in Use ?

Based on code analysis and dependency tree:

| Package | Used By | Evidence |
|---------|---------|----------|
| FileHelpers | RESTRunner.Domain | Domain model CSV parsing |
| Newtonsoft.Json | RESTRunner.PostmanImport, RESTRunner.Web | Postman collection parsing, JSON operations |
| CsvHelper | RESTRunner (Console) | CSV output generation (c:\test\RESTRunner.csv) |
| Microsoft.Extensions.Hosting | RESTRunner (Console) | Host builder and DI container |
| Microsoft.Extensions.Http | RESTRunner, Services.HttpClientRunner | HttpClientFactory registration |
| Microsoft.Extensions.Logging.Abstractions | Services.HttpClientRunner | ILogger<T> interfaces |
| Microsoft.AspNet.WebApi.Client | Services.HttpClientRunner | HTTP client utilities |
| System.Configuration.ConfigurationManager | RESTRunner (Console) | app.config file access |
| Swashbuckle.AspNetCore | RESTRunner.Web | Swagger/OpenAPI documentation |
| WebSpark.Bootswatch | RESTRunner.Web | Bootstrap theme switching |
| WebSpark.HttpClientUtility | RESTRunner.Web | HTTP client decorators |

### Packages Flagged for Review ??

| Package | Project | Reason | Action |
|---------|---------|--------|--------|
| System.Text.Json | RESTRunner (Console) | NU1510 warning - Framework-included | **Remove and test** |
| System.Security.Cryptography.Xml | RESTRunner.Web | NU1510 warning - May not be used | **Remove and test** |

---

## Security Scan Results

? **No known vulnerabilities** in any packages (as of 2025-12-23)

All packages checked against:
- GitHub Advisory Database
- NuGet Package Vulnerability Database
- Common Vulnerabilities and Exposures (CVE)

---

## Recommended Execution Plan

### Step 1: Remove Potentially Unused Packages (Low Risk)
```bash
# Branch for package cleanup
git checkout -b package-optimization

# Edit project files to remove NU1510 flagged packages
# RESTRunner\RESTRunner.csproj - Remove System.Text.Json
# RESTRunner.Web\RESTRunner.Web.csproj - Remove System.Security.Cryptography.Xml

# Test builds
dotnet build RESTRunner.sln --configuration Release

# Run tests
dotnet test

# If successful, commit
git add .
git commit -m "Remove framework-included packages (System.Text.Json, System.Security.Cryptography.Xml)"
```

### Step 2: Update Web App Packages (Medium Risk)
```bash
# Update Swashbuckle (minor version)
cd RESTRunner.Web
dotnet add package Swashbuckle.AspNetCore --version 10.1.0

# Update WebSpark.Bootswatch (minor version)
dotnet add package WebSpark.Bootswatch --version 1.34.0

# CAREFUL: Major version - test thoroughly
dotnet add package WebSpark.HttpClientUtility --version 2.1.2

# Build and test
dotnet build --configuration Release
dotnet run --configuration Release
# Manual web app smoke test at https://localhost:7001

# If successful, commit
git add .
git commit -m "Update web app packages (Swashbuckle 10.1.0, WebSpark.Bootswatch 1.34.0, WebSpark.HttpClientUtility 2.1.2)"
```

### Step 3: Update Test Packages (Medium Risk)
```bash
# Research MSTest v4 breaking changes first!
# https://github.com/microsoft/testfx/releases/tag/v4.0.0

# Update test packages
cd RESTRunner.Domain.Tests
dotnet add package Microsoft.NET.Test.Sdk --version 18.0.1
dotnet add package MSTest.TestAdapter --version 4.0.2
dotnet add package MSTest.TestFramework --version 4.0.2

# Run tests
dotnet test --configuration Release

# If all tests pass, commit
git add .
git commit -m "Update test packages (MSTest v4, Test SDK 18.0.1)"
```

### Step 4: Merge and Deploy
```bash
# If all updates successful
git checkout upgrade-to-NET10
git merge package-optimization
git branch -d package-optimization

# Or create separate PR for package updates
git push origin package-optimization
# Create PR: package-optimization -> upgrade-to-NET10
```

---

## Post-Update Validation Checklist

After each package update, verify:

### Build Validation
- [ ] `dotnet restore RESTRunner.sln` succeeds
- [ ] `dotnet build RESTRunner.sln --configuration Release` succeeds with 0 errors
- [ ] No new warnings introduced (beyond existing 5)

### Test Validation
- [ ] All 21 unit tests pass: `dotnet test`
- [ ] Test execution time comparable (currently 0.8s)
- [ ] No test discovery failures

### Functional Validation
- [ ] Console app executes with sample collection.json
- [ ] Web app starts at https://localhost:7001
- [ ] Swagger UI loads (if Swashbuckle updated)
- [ ] Theme switching works (if WebSpark.Bootswatch updated)
- [ ] HTTP requests execute (if WebSpark.HttpClientUtility updated)

---

## Risk Assessment

| Action | Risk Level | Impact if Fails | Rollback |
|--------|-----------|-----------------|----------|
| Remove System.Text.Json | ?? Very Low | Compilation error (easy to detect) | Re-add package |
| Remove System.Security.Cryptography.Xml | ?? Low | Compilation error (easy to detect) | Re-add package |
| Update Swashbuckle.AspNetCore | ?? Low | Swagger UI may not work | Revert to 9.0.4 |
| Update WebSpark.Bootswatch | ?? Low | Theme switching may break | Revert to 1.30.0 |
| Update WebSpark.HttpClientUtility | ?? Medium | HTTP client decorators may break | Revert to 1.2.0 |
| Update MSTest packages | ?? Medium | Tests may fail or not run | Revert to 3.10.4 |

---

## Summary of Recommendations

### ? Safe to Proceed (Low Risk)
1. Remove `System.Text.Json` from RESTRunner.csproj
2. Remove `System.Security.Cryptography.Xml` from RESTRunner.Web.csproj
3. Update `Swashbuckle.AspNetCore` to 10.1.0
4. Update `WebSpark.Bootswatch` to 1.34.0

### ?? Proceed with Caution (Medium Risk)
5. Update `WebSpark.HttpClientUtility` to 2.1.2 (major version - test thoroughly)
6. Update MSTest packages to v4 (major version - review breaking changes first)

### ? No Action Needed
All other packages are at latest stable versions and actively used.

---

**Total Estimated Time**: 1-2 hours for all updates and testing  
**Recommended Approach**: Incremental - Do Priority 1 first, then Priority 2, test between each  
**Branch Strategy**: Create `package-optimization` branch off `upgrade-to-NET10`  
**Testing Required**: Full regression test suite after all updates
