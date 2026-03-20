using Microsoft.AspNetCore.Mvc;
using RESTRunner.Web.Models;
using RESTRunner.Web.Services;

namespace RESTRunner.Web.Controllers;

/// <summary>
/// Controller for managing test executions (both API and MVC views)
/// </summary>
[Route("[controller]")]
public class ExecutionController : Controller
{
    private readonly IExecutionService _executionService;
    private readonly IConfigurationService _configurationService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ExecutionController> _logger;

    public ExecutionController(
        IExecutionService executionService,
        IConfigurationService configurationService,
        IHttpClientFactory httpClientFactory,
        ILogger<ExecutionController> logger)
    {
        _executionService = executionService;
        _configurationService = configurationService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    // ============ MVC VIEW ACTIONS ============

    /// <summary>
    /// Execution history list page
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 50, string? configurationId = null)
    {
        try
        {
            var history = await _executionService.GetExecutionHistoryAsync(pageSize, pageNumber, configurationId);
            var configurations = await _configurationService.GetAllAsync();

            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.ConfigurationId = configurationId;
            ViewBag.Configurations = configurations;

            return View(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading execution history");
            TempData["Error"] = "Failed to load execution history";
            return View(new List<ExecutionHistory>());
        }
    }

    /// <summary>
    /// Execution results detail page
    /// </summary>
    [HttpGet("Results/{id}")]
    public async Task<IActionResult> Results(string id)
    {
        try
        {
            var history = await _executionService.GetExecutionHistoryAsync(id);

            if (history == null)
            {
                TempData["Error"] = $"Execution {id} not found";
                return RedirectToAction(nameof(Index));
            }

            return View(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading execution results for {ExecutionId}", id);
            TempData["Error"] = "Failed to load execution results";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Delete execution history (MVC POST)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var deleted = await _executionService.DeleteExecutionHistoryAsync(id);

            if (deleted)
            {
                TempData["Success"] = "Execution history deleted successfully";
            }
            else
            {
                TempData["Error"] = "Execution not found";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting execution {ExecutionId}", id);
            TempData["Error"] = "Failed to delete execution history";
            return RedirectToAction(nameof(Index));
        }
    }

    // ============ API ENDPOINTS ============

    /// <summary>
    /// Start a new test execution
    /// </summary>
    /// <param name="request">Execution start request</param>
    /// <returns>Created test execution</returns>
    [HttpPost("/api/execution/start")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(TestExecution), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TestExecution>> StartExecution([FromBody] StartExecutionRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.ConfigurationId))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = "ConfigurationId is required",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            // Verify configuration exists
            var config = await _configurationService.GetByIdAsync(request.ConfigurationId);
            if (config == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Configuration Not Found",
                    Detail = $"Configuration with ID '{request.ConfigurationId}' not found",
                    Status = StatusCodes.Status404NotFound
                });
            }

            var execution = await _executionService.StartExecutionAsync(
                request.ConfigurationId,
                request.ExecutedBy ?? "API");

            _logger.LogInformation("Started execution {ExecutionId} via API", execution.Id);

            return CreatedAtAction(
                nameof(GetExecutionStatus),
                new { executionId = execution.Id },
                execution);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start execution for configuration {ConfigurationId}", request.ConfigurationId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Execution Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get execution status and progress
    /// </summary>
    /// <param name="executionId">Execution ID</param>
    /// <returns>Test execution details</returns>
    [HttpGet("/api/execution/{executionId}/status")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(TestExecution), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TestExecution>> GetExecutionStatus(string executionId)
    {
        var execution = await _executionService.GetExecutionAsync(executionId);

        if (execution == null)
        {
            // Try history
            var history = await _executionService.GetExecutionHistoryAsync(executionId);
            if (history != null)
            {
                // Convert history to execution for consistent response
                return Ok(new TestExecution
                {
                    Id = history.Id,
                    ConfigurationId = history.ConfigurationId,
                    ConfigurationName = history.ConfigurationName,
                    SourceType = history.SourceType,
                    SourceId = history.SourceId,
                    SourceName = history.SourceName,
                    Status = history.Status,
                    StartTime = history.StartTime,
                    ExecutedBy = history.ExecutedBy,
                    ErrorMessage = history.ErrorMessage,
                    ProgressPercentage = 100,
                    CurrentPhase = history.Status.ToString(),
                    TotalRequests = history.Statistics?.TotalRequests ?? 0,
                    CompletedRequests = history.Statistics?.TotalRequests ?? 0,
                    SuccessfulRequests = history.Statistics?.SuccessfulRequests ?? 0,
                    FailedRequests = history.Statistics?.FailedRequests ?? 0,
                    CurrentAverageResponseTime = history.Statistics?.AverageResponseTime ?? 0
                });
            }

            return NotFound(new ProblemDetails
            {
                Title = "Execution Not Found",
                Detail = $"Execution with ID '{executionId}' not found",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(execution);
    }

    /// <summary>
    /// Cancel a running execution
    /// </summary>
    /// <param name="executionId">Execution ID</param>
    /// <param name="request">Cancel request</param>
    /// <returns>Cancellation result</returns>
    [HttpPost("/api/execution/{executionId}/cancel")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelExecution(string executionId, [FromBody] CancelExecutionRequest request)
    {
        var cancelled = await _executionService.CancelExecutionAsync(
            executionId,
            request.CancelledBy ?? "API");

        if (!cancelled)
        {
            var execution = await _executionService.GetExecutionAsync(executionId);
            if (execution == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Execution Not Found",
                    Detail = $"Execution with ID '{executionId}' not found",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return BadRequest(new ProblemDetails
            {
                Title = "Cannot Cancel",
                Detail = "Execution cannot be cancelled in its current state",
                Status = StatusCodes.Status400BadRequest
            });
        }

        _logger.LogInformation("Cancelled execution {ExecutionId} via API", executionId);
        return Ok(new { message = "Execution cancelled successfully", executionId });
    }

    /// <summary>
    /// Get execution results
    /// </summary>
    /// <param name="executionId">Execution ID</param>
    /// <returns>Execution history with results</returns>
    [HttpGet("/api/execution/{executionId}/results")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ExecutionHistory), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExecutionHistory>> GetExecutionResults(string executionId)
    {
        var history = await _executionService.GetExecutionHistoryAsync(executionId);

        if (history == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Results Not Found",
                Detail = $"Execution results for ID '{executionId}' not found",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(history);
    }

    /// <summary>
    /// Download execution results as CSV
    /// </summary>
    /// <param name="executionId">Execution ID</param>
    /// <returns>CSV file</returns>
    [HttpGet("/api/execution/{executionId}/download")]
    [Produces("text/csv")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadResults(string executionId)
    {
        var history = await _executionService.GetExecutionHistoryAsync(executionId);

        if (history == null || string.IsNullOrEmpty(history.ResultsFilePath))
        {
            return NotFound(new ProblemDetails
            {
                Title = "Results Not Found",
                Detail = $"Results file for execution '{executionId}' not found",
                Status = StatusCodes.Status404NotFound
            });
        }

        if (!System.IO.File.Exists(history.ResultsFilePath))
        {
            return NotFound(new ProblemDetails
            {
                Title = "File Not Found",
                Detail = "Results file does not exist on disk",
                Status = StatusCodes.Status404NotFound
            });
        }

        var fileBytes = await System.IO.File.ReadAllBytesAsync(history.ResultsFilePath);
        var fileName = $"execution_{executionId}_results.csv";

        return File(fileBytes, "text/csv", fileName);
    }

    /// <summary>
    /// Get all running executions
    /// </summary>
    /// <returns>List of running executions</returns>
    [HttpGet("/api/execution/running")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(List<TestExecution>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TestExecution>>> GetRunningExecutions()
    {
        var executions = await _executionService.GetRunningExecutionsAsync();
        return Ok(executions);
    }

    /// <summary>
    /// Get execution history
    /// </summary>
    /// <param name="pageSize">Page size</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="configurationId">Filter by configuration ID (optional)</param>
    /// <returns>List of execution history records</returns>
    [HttpGet("/api/execution/history")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(List<ExecutionHistory>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ExecutionHistory>>> GetExecutionHistoryApi(
        [FromQuery] int pageSize = 50,
        [FromQuery] int pageNumber = 1,
        [FromQuery] string? configurationId = null)
    {
        var history = await _executionService.GetExecutionHistoryAsync(pageSize, pageNumber, configurationId);
        return Ok(history);
    }

    /// <summary>
    /// Get recent executions
    /// </summary>
    /// <param name="count">Number of recent executions to return</param>
    /// <returns>List of recent executions</returns>
    [HttpGet("/api/execution/recent")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(List<ExecutionHistory>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ExecutionHistory>>> GetRecentExecutions([FromQuery] int count = 10)
    {
        var executions = await _executionService.GetRecentExecutionsAsync(count);
        return Ok(executions);
    }

    /// <summary>
    /// Delete execution history (API)
    /// </summary>
    /// <param name="executionId">Execution ID</param>
    /// <returns>Deletion result</returns>
    [HttpDelete("/api/execution/{executionId}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteExecutionHistoryApi(string executionId)
    {
        var deleted = await _executionService.DeleteExecutionHistoryAsync(executionId);

        if (!deleted)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Execution Not Found",
                Detail = $"Execution with ID '{executionId}' not found",
                Status = StatusCodes.Status404NotFound
            });
        }

        _logger.LogInformation("Deleted execution history {ExecutionId} via API", executionId);
        return NoContent();
    }

    /// <summary>
    /// Return parsed CSV rows for an execution, optionally filtered
    /// </summary>
    [HttpGet("/api/execution/{executionId}/results/rows")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(List<ExecutionResultRow>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ExecutionResultRow>>> GetResultRows(
        string executionId,
        [FromQuery] string? statusCode = null,
        [FromQuery] string? verb = null,
        [FromQuery] string? instance = null,
        [FromQuery] string? path = null)
    {
        var history = await _executionService.GetExecutionHistoryAsync(executionId);
        if (history == null || string.IsNullOrEmpty(history.ResultsFilePath) || !System.IO.File.Exists(history.ResultsFilePath))
        {
            return NotFound(new ProblemDetails
            {
                Title = "Results Not Found",
                Detail = $"CSV results for execution '{executionId}' not found",
                Status = StatusCodes.Status404NotFound
            });
        }

        var rows = new List<ExecutionResultRow>();

        // Open with FileShare.ReadWrite so we can read while CsvOutput still holds a write lock
        string[] lines;
        using (var fs = new FileStream(history.ResultsFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (var sr = new StreamReader(fs, System.Text.Encoding.UTF8))
        {
            var content = await sr.ReadToEndAsync();
            lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        }

        // Skip header (index 0), parse each data row
        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var row = ExecutionResultRow.ParseCsvLine(line);
            if (row == null) continue;

            if (statusCode != null && row.ResultCode != statusCode) continue;
            if (verb != null && !row.Verb.Equals(verb, StringComparison.OrdinalIgnoreCase)) continue;
            if (instance != null && !row.Instance.Equals(instance, StringComparison.OrdinalIgnoreCase)) continue;
            if (path != null && !row.Request.Contains(path, StringComparison.OrdinalIgnoreCase)) continue;

            rows.Add(row);
        }

        return Ok(rows.OrderByDescending(r => r.Duration).ToList());
    }

    /// <summary>
    /// Export execution results
    /// </summary>
    /// <param name="executionId">Execution ID</param>
    /// <param name="format">Export format (csv, json)</param>
    /// <returns>Export file path</returns>
    [HttpPost("/api/execution/{executionId}/export")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ExportResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExportResult>> ExportResults(string executionId, [FromQuery] string format = "csv")
    {
        var filePath = await _executionService.ExportExecutionResultsAsync(executionId, format);

        if (filePath == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Export Failed",
                Detail = $"Could not export results for execution '{executionId}'",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(new ExportResult
        {
            ExecutionId = executionId,
            Format = format,
            FilePath = filePath,
            FileName = Path.GetFileName(filePath)
        });
    }

    /// <summary>
    /// Execute a single request immediately for quick testing
    /// </summary>
    [HttpPost("/api/execution/test-request")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(SingleRequestResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SingleRequestResult>> TestSingleRequest([FromBody] SingleRequestTestRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.BaseUrl) || string.IsNullOrWhiteSpace(request.Path))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = "BaseUrl and Path are required",
                Status = StatusCodes.Status400BadRequest
            });
        }

        using var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(30);

        if (!string.IsNullOrEmpty(request.BearerToken))
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", request.BearerToken);

        if (request.Headers != null)
            foreach (var h in request.Headers)
                client.DefaultRequestHeaders.TryAddWithoutValidation(h.Key, h.Value);

        var baseUrl = request.BaseUrl.TrimEnd('/');
        var path = (request.Path ?? string.Empty).TrimStart('/');
        var requestUrl = string.IsNullOrEmpty(path) ? baseUrl : $"{baseUrl}/{path}";

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var uri = new Uri(requestUrl);
            HttpResponseMessage response = request.Method?.ToUpperInvariant() switch
            {
                "GET" => await client.GetAsync(uri),
                "DELETE" => await client.DeleteAsync(uri),
                "POST" => await client.PostAsync(uri, new StringContent(request.Body ?? string.Empty, System.Text.Encoding.UTF8, "application/json")),
                "PUT" => await client.PutAsync(uri, new StringContent(request.Body ?? string.Empty, System.Text.Encoding.UTF8, "application/json")),
                "PATCH" => await client.PatchAsync(uri, new StringContent(request.Body ?? string.Empty, System.Text.Encoding.UTF8, "application/json")),
                _ => throw new ArgumentException($"Unsupported HTTP method: {request.Method}")
            };

            stopwatch.Stop();
            var responseBody = await response.Content.ReadAsStringAsync();
            var headers = response.Headers
                .Concat(response.Content.Headers)
                .ToDictionary(h => h.Key, h => string.Join(", ", h.Value));

            return Ok(new SingleRequestResult
            {
                Success = response.IsSuccessStatusCode,
                StatusCode = (int)response.StatusCode,
                StatusText = response.ReasonPhrase ?? response.StatusCode.ToString(),
                ResponseBody = responseBody,
                ResponseHeaders = headers,
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                RequestUrl = requestUrl
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogWarning(ex, "Test request failed: {Url}", requestUrl);
            return Ok(new SingleRequestResult
            {
                Success = false,
                StatusCode = 0,
                StatusText = "Connection Error",
                ResponseBody = string.Empty,
                ResponseHeaders = new Dictionary<string, string>(),
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                RequestUrl = requestUrl,
                ErrorMessage = ex.Message
            });
        }
    }
}

/// <summary>
/// Request to start a new execution
/// </summary>
public class StartExecutionRequest
{
    /// <summary>
    /// Configuration ID to execute
    /// </summary>
    public string ConfigurationId { get; set; } = string.Empty;

    /// <summary>
    /// User who initiated the execution
    /// </summary>
    public string? ExecutedBy { get; set; }
}

/// <summary>
/// Request to cancel an execution
/// </summary>
public class CancelExecutionRequest
{
    /// <summary>
    /// User who cancelled the execution
    /// </summary>
    public string? CancelledBy { get; set; }
}

/// <summary>
/// Export result information
/// </summary>
public class ExportResult
{
    /// <summary>
    /// Execution ID
    /// </summary>
    public string ExecutionId { get; set; } = string.Empty;

    /// <summary>
    /// Export format
    /// </summary>
    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// Full file path
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// File name
    /// </summary>
    public string FileName { get; set; } = string.Empty;
}

/// <summary>
/// Request payload for testing a single endpoint
/// </summary>
public class SingleRequestTestRequest
{
    public string BaseUrl { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = "GET";
    public string? Body { get; set; }
    public string? BearerToken { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
}

/// <summary>
/// Result of a single test request
/// </summary>
public class SingleRequestResult
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public string? ResponseBody { get; set; }
    public Dictionary<string, string> ResponseHeaders { get; set; } = new();
    public long ResponseTimeMs { get; set; }
    public string RequestUrl { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// A single parsed row from the execution CSV results file
/// CSV columns: Verb,Instance,LastRunDate,Duration,Request,ResultCode,SessionId,StatusDescription,Success,UserName,Content
/// </summary>
public class ExecutionResultRow
{
    public string Verb { get; set; } = string.Empty;
    public string Instance { get; set; } = string.Empty;
    public string LastRunDate { get; set; } = string.Empty;
    public long Duration { get; set; }
    public string Request { get; set; } = string.Empty;
    public string ResultCode { get; set; } = string.Empty;
    public string StatusDescription { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string UserName { get; set; } = string.Empty;

    public static ExecutionResultRow? ParseCsvLine(string line)
    {
        // Simple split — values don't contain commas in practice
        var parts = line.Split(',');
        if (parts.Length < 10) return null;
        return new ExecutionResultRow
        {
            Verb = parts[0].Trim(),
            Instance = parts[1].Trim(),
            LastRunDate = parts[2].Trim(),
            Duration = long.TryParse(parts[3].Trim(), out var d) ? d : 0,
            Request = parts[4].Trim(),
            ResultCode = parts[5].Trim(),
            StatusDescription = parts[7].Trim(),
            Success = bool.TryParse(parts[8].Trim(), out var s) && s,
            UserName = parts[9].Trim()
        };
    }
}
