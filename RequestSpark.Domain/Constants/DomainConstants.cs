namespace RequestSpark.Domain.Constants;

/// <summary>
/// Domain-wide constants for the RequestSpark application
/// </summary>
public static class DomainConstants
{
    /// <summary>
    /// Explicit non-secret placeholder used for sample credential fields.
    /// </summary>
    public const string PlaceholderPassword = "REPLACE_ME_PASSWORD";

    /// <summary>
    /// Default session ID prefix
    /// </summary>
    public const string SessionIdPrefix = "RequestSpark";

    /// <summary>
    /// Date format for session IDs
    /// </summary>
    public const string SessionDateFormat = "yyyy-MM-dd";

    /// <summary>
    /// Maximum length for instance names
    /// </summary>
    public const int MaxInstanceNameLength = 100;

    /// <summary>
    /// Maximum length for request paths
    /// </summary>
    public const int MaxRequestPathLength = 2000;

    /// <summary>
    /// Default timeout for requests in seconds
    /// </summary>
    public const int DefaultRequestTimeoutSeconds = 30;

    /// <summary>
    /// Common HTTP headers
    /// </summary>
    public static class Headers
    {
        /// <summary>
        /// HTTP Authorization header name
        /// </summary>
        public const string Authorization = "Authorization";

        /// <summary>
        /// HTTP Content-Type header name
        /// </summary>
        public const string ContentType = "Content-Type";

        /// <summary>
        /// Custom session ID header name
        /// </summary>
        public const string SessionId = "X-Session-Id";

        /// <summary>
        /// HTTP User-Agent header name
        /// </summary>
        public const string UserAgent = "User-Agent";
    }

    /// <summary>
    /// Common content types
    /// </summary>
    public static class ContentTypes
    {
        /// <summary>
        /// JSON content type
        /// </summary>
        public const string Json = "application/json";

        /// <summary>
        /// XML content type
        /// </summary>
        public const string Xml = "application/xml";

        /// <summary>
        /// Form URL-encoded content type
        /// </summary>
        public const string FormUrlEncoded = "application/x-www-form-urlencoded";

        /// <summary>
        /// Plain text content type
        /// </summary>
        public const string TextPlain = "text/plain";
    }
}
