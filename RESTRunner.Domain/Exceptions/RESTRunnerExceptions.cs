namespace RESTRunner.Domain.Exceptions;

/// <summary>
/// Base exception for all RESTRunner domain-related exceptions
/// </summary>
public abstract class RESTRunnerException : Exception
{
    /// <summary>
    /// Initializes a new instance of the RESTRunnerException class
    /// </summary>
    /// <param name="message">The error message</param>
    protected RESTRunnerException(string message) : base(message) { }
    
    /// <summary>
    /// Initializes a new instance of the RESTRunnerException class
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="innerException">The inner exception</param>
    protected RESTRunnerException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when validation fails for domain objects
/// </summary>
public class ValidationException : RESTRunnerException
{
    /// <summary>
    /// The name of the property that failed validation
    /// </summary>
    public string? PropertyName { get; }
    
    /// <summary>
    /// Initializes a new instance of the ValidationException class
    /// </summary>
    /// <param name="message">The error message</param>
    public ValidationException(string message) : base(message) { }
    
    /// <summary>
    /// Initializes a new instance of the ValidationException class
    /// </summary>
    /// <param name="propertyName">The name of the property that failed validation</param>
    /// <param name="message">The error message</param>
    public ValidationException(string propertyName, string message) : base(message)
    {
        PropertyName = propertyName;
    }
}

/// <summary>
/// Exception thrown when configuration is invalid or missing
/// </summary>
public class ConfigurationException : RESTRunnerException
{
    /// <summary>
    /// Initializes a new instance of the ConfigurationException class
    /// </summary>
    /// <param name="message">The error message</param>
    public ConfigurationException(string message) : base(message) { }
    
    /// <summary>
    /// Initializes a new instance of the ConfigurationException class
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="innerException">The inner exception</param>
    public ConfigurationException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when request execution fails
/// </summary>
public class RequestExecutionException : RESTRunnerException
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
    /// Initializes a new instance of the RequestExecutionException class
    /// </summary>
    /// <param name="message">The error message</param>
    public RequestExecutionException(string message) : base(message) { }
    
    /// <summary>
    /// Initializes a new instance of the RequestExecutionException class
    /// </summary>
    /// <param name="requestPath">The path of the request that failed</param>
    /// <param name="statusCode">The HTTP status code</param>
    /// <param name="message">The error message</param>
    public RequestExecutionException(string requestPath, string statusCode, string message) : base(message)
    {
        RequestPath = requestPath;
        StatusCode = statusCode;
    }
    
    /// <summary>
    /// Initializes a new instance of the RequestExecutionException class
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="innerException">The inner exception</param>
    public RequestExecutionException(string message, Exception innerException) : base(message, innerException) { }
}