namespace RESTRunner.Web.Models;

/// <summary>
/// Request to start a new execution.
/// </summary>
public class StartExecutionRequest
{
    /// <summary>
    /// Configuration ID to execute.
    /// </summary>
    public string ConfigurationId { get; set; } = string.Empty;

    /// <summary>
    /// User who initiated the execution.
    /// </summary>
    public string? ExecutedBy { get; set; }
}

/// <summary>
/// Request to cancel an execution.
/// </summary>
public class CancelExecutionRequest
{
    /// <summary>
    /// User who cancelled the execution.
    /// </summary>
    public string? CancelledBy { get; set; }
}

/// <summary>
/// Export result information.
/// </summary>
public class ExportResult
{
    /// <summary>
    /// Execution ID.
    /// </summary>
    public string ExecutionId { get; set; } = string.Empty;

    /// <summary>
    /// Export format.
    /// </summary>
    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// Full file path.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// File name.
    /// </summary>
    public string FileName { get; set; } = string.Empty;
}

/// <summary>
/// Request payload for testing a single endpoint.
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
/// Result of a single test request.
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
/// A single parsed row from the execution CSV results file.
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
        var parts = line.Split(',');
        if (parts.Length < 10) return null;

        return new ExecutionResultRow
        {
            Verb = parts[0].Trim(),
            Instance = parts[1].Trim(),
            LastRunDate = parts[2].Trim(),
            Duration = long.TryParse(parts[3].Trim(), out var duration) ? duration : 0,
            Request = parts[4].Trim(),
            ResultCode = parts[5].Trim(),
            StatusDescription = parts[7].Trim(),
            Success = bool.TryParse(parts[8].Trim(), out var success) && success,
            UserName = parts[9].Trim()
        };
    }
}