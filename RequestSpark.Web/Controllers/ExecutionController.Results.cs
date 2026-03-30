using Microsoft.AspNetCore.Mvc;
using RequestSpark.Web.Models;
using System.Text;

namespace RequestSpark.Web.Controllers;

public partial class ExecutionController
{
    /// <summary>
    /// Return parsed CSV rows for an execution, optionally filtered.
    /// </summary>
    /// <param name="executionId">Execution ID.</param>
    /// <param name="statusCode">Optional status code filter.</param>
    /// <param name="verb">Optional HTTP verb filter.</param>
    /// <param name="instance">Optional instance filter.</param>
    /// <param name="path">Optional path substring filter.</param>
    /// <returns>Ordered result rows for the execution.</returns>
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
        string[] lines;

        using (var fs = new FileStream(history.ResultsFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (var sr = new StreamReader(fs, Encoding.UTF8))
        {
            var content = await sr.ReadToEndAsync();
            lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        }

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
    /// Export execution results.
    /// </summary>
    /// <param name="executionId">Execution ID.</param>
    /// <param name="format">Export format.</param>
    /// <returns>Information about the exported file.</returns>
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
    /// Executes a single request immediately for quick testing.
    /// </summary>
    /// <param name="request">Request details to execute.</param>
    /// <returns>The test result.</returns>
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
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", request.BearerToken);
        }

        if (request.Headers != null)
        {
            foreach (var header in request.Headers)
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

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
                "POST" => await client.PostAsync(uri, new StringContent(request.Body ?? string.Empty, Encoding.UTF8, "application/json")),
                "PUT" => await client.PutAsync(uri, new StringContent(request.Body ?? string.Empty, Encoding.UTF8, "application/json")),
                "PATCH" => await client.PatchAsync(uri, new StringContent(request.Body ?? string.Empty, Encoding.UTF8, "application/json")),
                _ => throw new ArgumentException($"Unsupported HTTP method: {request.Method}")
            };

            stopwatch.Stop();
            var responseBody = await response.Content.ReadAsStringAsync();
            var headers = response.Headers
                .Concat(response.Content.Headers)
                .ToDictionary(header => header.Key, header => string.Join(", ", header.Value));

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
