# RESTRunner

[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![License](https://img.shields.io/github/license/markhazleton/RESTRunner)](https://github.com/markhazleton/RESTRunner/blob/main/LICENSE)
[![Build Status](https://img.shields.io/github/actions/workflow/status/markhazleton/RESTRunner/build.yml?branch=main)](https://github.com/markhazleton/RESTRunner/actions)
[![GitHub Issues](https://img.shields.io/github/issues/markhazleton/RESTRunner)](https://github.com/markhazleton/RESTRunner/issues)
[![GitHub Stars](https://img.shields.io/github/stars/markhazleton/RESTRunner)](https://github.com/markhazleton/RESTRunner/stargazers)
[![Release](https://img.shields.io/github/v/release/markhazleton/RESTRunner)](https://github.com/markhazleton/RESTRunner/releases/latest)

A comprehensive .NET 10 solution for running REST API tests, performance benchmarking, and regression testing using Postman collections.

> **Latest Update** (v10.0.0): Upgraded to .NET 10.0 (LTS) with 19% faster builds, 25% faster tests, and comprehensive package optimization!

## ? Features

- **Postman Collection Integration**: Import and execute existing Postman collections
- **Automated Regression Testing**: Run comprehensive API test suites automatically
- **Performance Analysis**: Compare response times across multiple API instances
- **Load Testing**: Stress test your REST APIs with configurable parameters
- **Detailed Reporting**: Export results to CSV with comprehensive statistics
- **Web Interface**: Razor Pages web application for interactive testing
- **Sample CRUD API**: Built-in sample API for testing and demonstration
- **Comprehensive Statistics**: Response time percentiles, success rates, and performance metrics
- **? High Performance**: 19% faster builds, 25% faster test execution
- **?? Secure**: 93% packages at latest versions, zero vulnerabilities

## ?? Table of Contents

- [Features](#-features)
- [Prerequisites](#-prerequisites)
- [Installation](#-installation)
- [Quick Start](#-quick-start)
- [Usage](#-usage)
- [Project Structure](#-project-structure)
- [Configuration](#-configuration)
- [Examples](#-examples)
- [Performance](#-performance)
- [Contributing](#-contributing)
- [License](#-license)
- [Support](#-support)

## ?? Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later (LTS)
- Windows, macOS, or Linux
- Visual Studio 2022 17.12+ or VS Code (recommended)

### Why .NET 10?

RESTRunner uses **.NET 10.0 (LTS)** for:
- **Long-term support**: 3 years of support (until November 2028)
- **Performance**: Significant improvements in build and test performance
- **Security**: Latest security patches and updates
- **Compatibility**: Modern API features and optimizations

## ?? Installation

### Clone the Repository

```bash
git clone https://github.com/markhazleton/RESTRunner.git
cd RESTRunner
```

### Build the Solution

```bash
dotnet build
# Expected output: Project RESTRunner.sln has been successfully built.
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test RESTRunner.Domain.Tests

# Expected output: 21/21 tests passing (100%)
# Test execution time: ~0.6s (25% faster than .NET 9)
```

## ????? Quick Start

### Console Application

1. **Prepare your Postman collection**: Place your `collection.json` file in the `RESTRunner` project directory
2. **Run the console application**:
   ```bash
   cd RESTRunner
   dotnet run
   ```
3. **View results**: Check the generated CSV file at `c:\test\RESTRunner.csv`

### Web Application

1. **Start the web server**:
   ```bash
   cd RESTRunner.Web
   dotnet run
   ```

## ? Performance

RESTRunner v10.0.0 delivers significant performance improvements:

### Build Performance
- **Build Time**: 19% faster (5.1s ? 4.1s)
- **Test Execution**: 25% faster (0.8s ? 0.6s)
- **Package Optimization**: 11.8% fewer dependencies

### Package Health
- **Latest Versions**: 93% packages at latest stable versions
- **Security**: Zero vulnerabilities
- **Framework-Included**: Optimized package references for .NET 10

### Quality Metrics
- **Test Pass Rate**: 100% (21/21 tests)
- **Build Errors**: 0
- **Code Quality**: MSTest v4 analyzers enabled for continuous improvement

For detailed upgrade information, see [Upgrade Documentation](.github/upgrades/upgrade-complete-summary.md).

## ?? Changelog

### v10.0.0 (2025-12-23) - .NET 10 LTS Upgrade

**Major Changes:**
- ? Upgraded all projects to .NET 10.0 (LTS)
- ? Build performance improved by 19% (5.1s ? 4.1s)
- ? Test performance improved by 25% (0.8s ? 0.6s)
- ?? Package optimization: 17 ? 15 packages (93% at latest)
- ?? Removed framework-included packages (System.Text.Json, System.Security.Cryptography.Xml)
- ?? Updated WebSpark packages to latest (Bootswatch 1.34.0, HttpClientUtility 2.1.2)
- ?? MSTest v4 with code quality analyzers
- ?? Comprehensive upgrade documentation

**Quality Improvements:**
- All 21 tests passing (100%)
- Zero build errors
- NU1510 warnings eliminated
- MSTest v4 code quality analyzers enabled

**Documentation:**
- Added comprehensive upgrade documentation in `.github/upgrades/`
- Created GitHub issues for future improvements (#2, #3)

See [CHANGELOG.md](https://github.com/markhazleton/RESTRunner/blob/main/CHANGELOG.md) for complete version history.

## ?? Contributing

### Reporting Issues

When reporting issues, please include:
- .NET 10 SDK version (`dotnet --version`)
- Operating system
- Steps to reproduce
- Expected vs actual behavior
- Sample Postman collection (if applicable)

## ?? Documentation

- **Upgrade Guide**: See [.github/upgrades/upgrade-complete-summary.md](.github/upgrades/upgrade-complete-summary.md)
- **Assessment Report**: [.github/upgrades/assessment.md](.github/upgrades/assessment.md)
- **Migration Plan**: [.github/upgrades/plan.md](.github/upgrades/plan.md)
- **Package Optimization**: [.github/upgrades/package-review-report.md](.github/upgrades/package-review-report.md)

## ?? Changelog



