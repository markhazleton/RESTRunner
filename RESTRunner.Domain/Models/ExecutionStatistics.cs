using System.Collections.Concurrent;

namespace RESTRunner.Domain.Models;

/// <summary>
/// Statistics collected during REST runner execution
/// </summary>
public class ExecutionStatistics
{
    // Private fields for thread-safe operations
    private int _totalRequests;
    private int _successfulRequests;
    private int _failedRequests;
    private long _minResponseTime = long.MaxValue;
    private long _maxResponseTime;

    /// <summary>
    /// Total number of requests made
    /// </summary>
    public int TotalRequests => _totalRequests;

    /// <summary>
    /// Total number of successful requests (2xx status codes)
    /// </summary>
    public int SuccessfulRequests => _successfulRequests;

    /// <summary>
    /// Total number of failed requests (non-2xx status codes)
    /// </summary>
    public int FailedRequests => _failedRequests;

    /// <summary>
    /// Start time of the execution
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// End time of the execution
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Total execution duration
    /// </summary>
    public TimeSpan TotalDuration => EndTime - StartTime;

    /// <summary>
    /// Average response time in milliseconds
    /// </summary>
    public double AverageResponseTime { get; set; }

    /// <summary>
    /// Minimum response time in milliseconds
    /// </summary>
    public long MinResponseTime => _minResponseTime == long.MaxValue ? 0 : _minResponseTime;

    /// <summary>
    /// Maximum response time in milliseconds
    /// </summary>
    public long MaxResponseTime => _maxResponseTime;

    /// <summary>
    /// Number of requests per HTTP method
    /// </summary>
    public ConcurrentDictionary<string, int> RequestsByMethod { get; set; } = new();

    /// <summary>
    /// Number of requests per instance
    /// </summary>
    public ConcurrentDictionary<string, int> RequestsByInstance { get; set; } = new();

    /// <summary>
    /// Number of requests per status code
    /// </summary>
    public ConcurrentDictionary<string, int> RequestsByStatusCode { get; set; } = new();

    /// <summary>
    /// Number of requests per user
    /// </summary>
    public ConcurrentDictionary<string, int> RequestsByUser { get; set; } = new();

    /// <summary>
    /// Requests per second during execution
    /// </summary>
    public double RequestsPerSecond => TotalDuration.TotalSeconds > 0 ? TotalRequests / TotalDuration.TotalSeconds : 0;

    /// <summary>
    /// Success rate as a percentage (0-100)
    /// </summary>
    public double SuccessRate => TotalRequests > 0 ? (double)SuccessfulRequests / TotalRequests * 100 : 0;

    /// <summary>
    /// Collection of response times for percentile calculations
    /// </summary>
    private readonly ConcurrentBag<long> _responseTimes = new();

    /// <summary>
    /// Increment total request count in a thread-safe manner
    /// </summary>
    public void IncrementTotalRequests() => Interlocked.Increment(ref _totalRequests);

    /// <summary>
    /// Increment successful request count in a thread-safe manner
    /// </summary>
    public void IncrementSuccessfulRequests() => Interlocked.Increment(ref _successfulRequests);

    /// <summary>
    /// Increment failed request count in a thread-safe manner
    /// </summary>
    public void IncrementFailedRequests() => Interlocked.Increment(ref _failedRequests);

    /// <summary>
    /// Add a response time to the collection
    /// </summary>
    /// <param name="responseTime">Response time in milliseconds</param>
    public void AddResponseTime(long responseTime)
    {
        _responseTimes.Add(responseTime);
        
        // Update min/max using thread-safe operations
        var currentMin = _minResponseTime;
        while (responseTime < currentMin && 
               Interlocked.CompareExchange(ref _minResponseTime, responseTime, currentMin) != currentMin)
        {
            currentMin = _minResponseTime;
        }

        var currentMax = _maxResponseTime;
        while (responseTime > currentMax && 
               Interlocked.CompareExchange(ref _maxResponseTime, responseTime, currentMax) != currentMax)
        {
            currentMax = _maxResponseTime;
        }
    }

    /// <summary>
    /// Calculate the specified percentile of response times
    /// </summary>
    /// <param name="percentile">Percentile to calculate (0-100)</param>
    /// <returns>Response time at the specified percentile</returns>
    public long GetResponseTimePercentile(double percentile)
    {
        if (percentile < 0 || percentile > 100)
            throw new ArgumentOutOfRangeException(nameof(percentile), "Percentile must be between 0 and 100");

        var times = _responseTimes.ToArray();
        if (times.Length == 0) return 0;

        Array.Sort(times);
        var index = (int)Math.Ceiling(percentile / 100.0 * times.Length) - 1;
        return times[Math.Max(0, Math.Min(index, times.Length - 1))];
    }

    /// <summary>
    /// Finalize statistics calculation
    /// </summary>
    public void FinalizeStatistics()
    {
        EndTime = DateTime.UtcNow;
        
        var times = _responseTimes.ToArray();
        if (times.Length > 0)
        {
            AverageResponseTime = times.Average();
        }
    }

    /// <summary>
    /// Get a formatted summary of the statistics
    /// </summary>
    /// <returns>Formatted statistics summary</returns>
    public override string ToString()
    {
        return $"Total Requests: {TotalRequests}, " +
               $"Success Rate: {SuccessRate:F2}%, " +
               $"Avg Response: {AverageResponseTime:F2}ms, " +
               $"Duration: {TotalDuration:hh\\:mm\\:ss}, " +
               $"RPS: {RequestsPerSecond:F2}";
    }
}