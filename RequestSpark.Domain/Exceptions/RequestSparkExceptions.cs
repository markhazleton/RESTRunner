namespace RequestSpark.Domain.Exceptions;

/// <summary>
/// Base exception for all RequestSpark domain-related exceptions
/// </summary>
public abstract class RequestSparkException : Exception
{
    /// <summary>
    /// Initializes a new instance of the RequestSparkException class
    /// </summary>
    /// <param name="message">The error message</param>
    protected RequestSparkException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the RequestSparkException class
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="innerException">The inner exception</param>
    protected RequestSparkException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when validation fails for domain objects
/// </summary>
public class RequestSparkValidationException : RequestSparkException
{
    /// <summary>
    /// The name of the property that failed validation
    /// </summary>
    public string? PropertyName { get; }

    /// <summary>
    /// Initializes a new instance of the RequestSparkValidationException class
    /// </summary>
    /// <param name="message">The error message</param>
    public RequestSparkValidationException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the RequestSparkValidationException class
    /// </summary>
    /// <param name="propertyName">The name of the property that failed validation</param>
    /// <param name="message">The error message</param>
    public RequestSparkValidationException(string propertyName, string message) : base(message)
    {
        PropertyName = propertyName;
    }
}

/// <summary>
/// Exception thrown when configuration is invalid or missing
/// </summary>
public class RequestSparkConfigurationException : RequestSparkException
{
    /// <summary>
    /// Initializes a new instance of the RequestSparkConfigurationException class
    /// </summary>
    /// <param name="message">The error message</param>
    public RequestSparkConfigurationException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the RequestSparkConfigurationException class
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="innerException">The inner exception</param>
    public RequestSparkConfigurationException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when request execution fails
/// </summary>
public class RequestSparkRequestExecutionException : RequestSparkException
{
    /// <summary>
    /// The HTTP status code if available
    /// </summary>
    public string? StatusCode { get; }

    /// <summary>
    /// The request that failed
    /// </summary>
    public string? RequestPath { get; }

    /// <summary>
    /// Initializes a new instance of the RequestSparkRequestExecutionException class
    /// </summary>
    /// <param name="message">The error message</param>
    public RequestSparkRequestExecutionException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the RequestSparkRequestExecutionException class
    /// </summary>
    /// <param name="requestPath">The path of the request that failed</param>
    /// <param name="statusCode">The HTTP status code</param>
    /// <param name="message">The error message</param>
    public RequestSparkRequestExecutionException(string requestPath, string statusCode, string message) : base(message)
    {
        RequestPath = requestPath;
        StatusCode = statusCode;
    }

    /// <summary>
    /// Initializes a new instance of the RequestSparkRequestExecutionException class
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="innerException">The inner exception</param>
    public RequestSparkRequestExecutionException(string message, Exception innerException) : base(message, innerException) { }
}
