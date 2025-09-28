# RESTRunner

[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![License](https://img.shields.io/github/license/markhazleton/RESTRunner)](https://github.com/markhazleton/RESTRunner/blob/main/LICENSE)
[![Build Status](https://img.shields.io/github/actions/workflow/status/markhazleton/RESTRunner/build.yml?branch=main)](https://github.com/markhazleton/RESTRunner/actions)
[![GitHub Issues](https://img.shields.io/github/issues/markhazleton/RESTRunner)](https://github.com/markhazleton/RESTRunner/issues)
[![GitHub Stars](https://img.shields.io/github/stars/markhazleton/RESTRunner)](https://github.com/markhazleton/RESTRunner/stargazers)

A comprehensive .NET 9 solution for running REST API tests, performance benchmarking, and regression testing using Postman collections.

## ?? Features

- **Postman Collection Integration**: Import and execute existing Postman collections
- **Automated Regression Testing**: Run comprehensive API test suites automatically
- **Performance Analysis**: Compare response times across multiple API instances
- **Load Testing**: Stress test your REST APIs with configurable parameters
- **Detailed Reporting**: Export results to CSV with comprehensive statistics
- **Web Interface**: Razor Pages web application for interactive testing
- **Sample CRUD API**: Built-in sample API for testing and demonstration
- **Comprehensive Statistics**: Response time percentiles, success rates, and performance metrics

## ?? Table of Contents

- [Features](#-features)
- [Prerequisites](#-prerequisites)
- [Installation](#-installation)
- [Quick Start](#-quick-start)
- [Usage](#-usage)
- [Project Structure](#-project-structure)
- [Configuration](#-configuration)
- [Examples](#-examples)
- [Contributing](#-contributing)
- [License](#-license)
- [Support](#-support)

## ?? Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later
- Windows, macOS, or Linux
- Visual Studio 2022 or VS Code (recommended)

## ?? Installation

### Clone the Repository

```bash
git clone https://github.com/markhazleton/RESTRunner.git
cd RESTRunner
```

### Build the Solution

```bash
dotnet build
```

### Run Tests

```bash
dotnet test
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
2. **Open your browser**: Navigate to `https://localhost:7001` (or the URL shown in console)
3. **Explore the API**: Visit `/docs` for Swagger documentation

## ?? Usage

### Console Application Features

The console application provides comprehensive REST API testing with detailed statistics:

```
?? REST RUNNER EXECUTION STATISTICS
================================================================================

?? OVERALL SUMMARY
----------------------------------------
Total Requests:           1,250
Successful Requests:      1,225 (98.00%)
Failed Requests:          25 (2.00%)
Start Time:               2024-01-15 10:30:00 UTC
End Time:                 2024-01-15 10:32:15 UTC
Total Duration:           00:02:15
Requests per Second:      9.26

? PERFORMANCE METRICS
----------------------------------------
Average Response Time:    125.50 ms
Minimum Response Time:    45 ms
Maximum Response Time:    2,340 ms

?? RESPONSE TIME PERCENTILES
----------------------------------------
50th Percentile (P50):    98 ms
75th Percentile (P75):    156 ms
90th Percentile (P90):    234 ms
95th Percentile (P95):    345 ms
99th Percentile (P99):    567 ms
99.9th Percentile:        1,234 ms
```

### Web Application Features

- **Interactive API Testing**: Test endpoints through a web interface
- **Sample CRUD Operations**: Employee and Department management
- **Swagger Documentation**: Complete API documentation at `/docs`
- **Real-time Results**: View test results in real-time

### Postman Collection Format

Ensure your `collection.json` follows Postman Collection v2.1.0 format:

```json
{
  "info": {
    "name": "Your API Collection",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Your Endpoint",
      "request": {
        "method": "GET",
        "url": "{{api-url}}/api/endpoint"
      }
    }
  ]
}
```

## ??? Project Structure

```
RESTRunner/
??? RESTRunner/                          # Console application
?   ??? Program.cs                       # Main entry point
?   ??? collection.json                  # Postman collection
?   ??? RESTRunner.csproj
??? RESTRunner.Web/                      # Web application (Razor Pages)
?   ??? Controllers/                     # MVC Controllers
?   ??? SampleCRUD/                      # Sample API implementation
?   ??? Views/                           # Razor views
?   ??? Program.cs                       # Web app configuration
?   ??? RESTRunner.Web.csproj
??? RESTRunner.Domain/                   # Core domain models
?   ??? Models/                          # Data models
?   ??? Interfaces/                      # Service interfaces
?   ??? Extensions/                      # Extension methods
?   ??? Outputs/                         # Output formatters
??? RESTRunner.Services.HttpClient/      # HTTP client services
??? RESTRunner.PostmanImport/           # Postman collection parser
??? RESTRunner.Domain.Tests/            # Unit tests
??? README.md
```

## ?? Configuration

### Environment Variables

- `api-url`: Base URL for your API endpoints (used in Postman collections)

### Output Configuration

Results are exported to CSV format with the following columns:
- Request URL
- HTTP Method
- Response Time (ms)
- Status Code
- Response Size
- Timestamp
- Instance Information

## ?? Examples

### Basic Usage

```bash
# Run with default settings
dotnet run --project RESTRunner

# Run web application
dotnet run --project RESTRunner.Web
```

### Custom Collection

```bash
# Place your collection.json in the RESTRunner directory
cp your-collection.json RESTRunner/collection.json
dotnet run --project RESTRunner
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test RESTRunner.Domain.Tests
```

## ?? Contributing

We welcome contributions! Please see our [Contributing Guidelines](https://github.com/markhazleton/RESTRunner/blob/main/CONTRIBUTING.md) for details.

### Development Setup

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature-name`
3. Make your changes and add tests
4. Run tests: `dotnet test`
5. Commit your changes: `git commit -m 'Add some feature'`
6. Push to the branch: `git push origin feature/your-feature-name`
7. Submit a pull request

### Code of Conduct

This project adheres to a [Code of Conduct](https://github.com/markhazleton/RESTRunner/blob/main/CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code.

## ?? License

This project is licensed under the MIT License - see the [LICENSE](https://github.com/markhazleton/RESTRunner/blob/main/LICENSE) file for details.

## ?? Support

- **Documentation**: Check the [Wiki](https://github.com/markhazleton/RESTRunner/wiki) for detailed guides
- **Issues**: Report bugs or request features via [GitHub Issues](https://github.com/markhazleton/RESTRunner/issues)
- **Discussions**: Join the conversation in [GitHub Discussions](https://github.com/markhazleton/RESTRunner/discussions)

### Reporting Issues

When reporting issues, please include:
- .NET version
- Operating system
- Steps to reproduce
- Expected vs actual behavior
- Sample Postman collection (if applicable)

## ?? Changelog

See [CHANGELOG.md](https://github.com/markhazleton/RESTRunner/blob/main/CHANGELOG.md) for a list of changes and version history.

## ??? Versioning

This project uses [Semantic Versioning](https://semver.org/). For available versions, see the [tags on this repository](https://github.com/markhazleton/RESTRunner/tags).

---

**Made with ?? by [Mark Hazleton](https://github.com/markhazleton)**

*RESTRunner - Streamline your API testing workflow*



