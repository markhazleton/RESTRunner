namespace RESTRunner.Web.Services;

/// <summary>
/// Service for file storage operations
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Initialize storage directories
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Save a file to the configurations directory
    /// </summary>
    /// <param name="fileName">File name</param>
    /// <param name="content">File content</param>
    /// <returns>Full file path</returns>
    Task<string> SaveConfigurationAsync(string fileName, string content);

    /// <summary>
    /// Save a collection file
    /// </summary>
    /// <param name="fileName">File name</param>
    /// <param name="file">File content</param>
    /// <returns>Full file path</returns>
    Task<string> SaveCollectionAsync(string fileName, IFormFile file);

    /// <summary>
    /// Save a collection file from content
    /// </summary>
    /// <param name="fileName">File name</param>
    /// <param name="content">File content</param>
    /// <returns>Full file path</returns>
    Task<string> SaveCollectionAsync(string fileName, string content);

    /// <summary>
    /// Save execution results
    /// </summary>
    /// <param name="fileName">File name</param>
    /// <param name="content">File content</param>
    /// <returns>Full file path</returns>
    Task<string> SaveResultsAsync(string fileName, string content);

    /// <summary>
    /// Save execution log
    /// </summary>
    /// <param name="fileName">File name</param>
    /// <param name="content">Log content</param>
    /// <returns>Full file path</returns>
    Task<string> SaveLogAsync(string fileName, string content);

    /// <summary>
    /// Read file content
    /// </summary>
    /// <param name="filePath">Full file path</param>
    /// <returns>File content or null if not found</returns>
    Task<string?> ReadFileAsync(string filePath);

    /// <summary>
    /// Delete a file
    /// </summary>
    /// <param name="filePath">Full file path</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteFileAsync(string filePath);

    /// <summary>
    /// Check if file exists
    /// </summary>
    /// <param name="filePath">Full file path</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> FileExistsAsync(string filePath);

    /// <summary>
    /// Get file info
    /// </summary>
    /// <param name="filePath">Full file path</param>
    /// <returns>File info or null if not found</returns>
    Task<FileInfo?> GetFileInfoAsync(string filePath);

    /// <summary>
    /// List files in a directory
    /// </summary>
    /// <param name="directoryType">Directory type (configurations, collections, results, logs)</param>
    /// <param name="pattern">File pattern (optional)</param>
    /// <returns>List of file paths</returns>
    Task<List<string>> ListFilesAsync(string directoryType, string pattern = "*");

    /// <summary>
    /// Generate unique file name
    /// </summary>
    /// <param name="baseName">Base file name</param>
    /// <param name="extension">File extension</param>
    /// <returns>Unique file name</returns>
    string GenerateUniqueFileName(string baseName, string extension);

    /// <summary>
    /// Get full path for a directory type
    /// </summary>
    /// <param name="directoryType">Directory type</param>
    /// <returns>Full directory path</returns>
    string GetDirectoryPath(string directoryType);

    /// <summary>
    /// Clean up old files
    /// </summary>
    /// <param name="directoryType">Directory type</param>
    /// <param name="olderThan">Delete files older than this date</param>
    /// <returns>Number of files deleted</returns>
    Task<int> CleanupOldFilesAsync(string directoryType, DateTime olderThan);
}