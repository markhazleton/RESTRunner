namespace RESTRunner.Web.Services;

/// <summary>
/// File-based storage service implementation
/// </summary>
public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileStorageService> _logger;
    private readonly string _dataPath;

    public FileStorageService(IWebHostEnvironment environment, ILogger<FileStorageService> logger)
    {
        _environment = environment;
        _logger = logger;
        _dataPath = Path.Combine(_environment.ContentRootPath, "Data");
    }

    public async Task InitializeAsync()
    {
        try
        {
            var directories = new[]
            {
                "configurations",
                "collections", 
                "results",
                "logs"
            };

            foreach (var dir in directories)
            {
                var fullPath = Path.Combine(_dataPath, dir);
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                    _logger.LogInformation("Created directory: {Directory}", fullPath);
                }
            }

            _logger.LogInformation("File storage initialized at: {DataPath}", _dataPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize file storage");
            throw;
        }
    }

    public async Task<string> SaveConfigurationAsync(string fileName, string content)
    {
        var filePath = Path.Combine(_dataPath, "configurations", fileName);
        await File.WriteAllTextAsync(filePath, content);
        _logger.LogDebug("Saved configuration file: {FilePath}", filePath);
        return filePath;
    }

    public async Task<string> SaveCollectionAsync(string fileName, IFormFile file)
    {
        var filePath = Path.Combine(_dataPath, "collections", fileName);
        
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);
        
        _logger.LogDebug("Saved collection file: {FilePath}", filePath);
        return filePath;
    }

    public async Task<string> SaveCollectionAsync(string fileName, string content)
    {
        var filePath = Path.Combine(_dataPath, "collections", fileName);
        await File.WriteAllTextAsync(filePath, content);
        _logger.LogDebug("Saved collection file: {FilePath}", filePath);
        return filePath;
    }

    public async Task<string> SaveResultsAsync(string fileName, string content)
    {
        var filePath = Path.Combine(_dataPath, "results", fileName);
        await File.WriteAllTextAsync(filePath, content);
        _logger.LogDebug("Saved results file: {FilePath}", filePath);
        return filePath;
    }

    public async Task<string> SaveLogAsync(string fileName, string content)
    {
        var filePath = Path.Combine(_dataPath, "logs", fileName);
        await File.WriteAllTextAsync(filePath, content);
        _logger.LogDebug("Saved log file: {FilePath}", filePath);
        return filePath;
    }

    public async Task<string?> ReadFileAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return null;

            return await File.ReadAllTextAsync(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read file: {FilePath}", filePath);
            return null;
        }
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return false;

            File.Delete(filePath);
            _logger.LogDebug("Deleted file: {FilePath}", filePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file: {FilePath}", filePath);
            return false;
        }
    }

    public async Task<bool> FileExistsAsync(string filePath)
    {
        return File.Exists(filePath);
    }

    public async Task<FileInfo?> GetFileInfoAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return null;

            return new FileInfo(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get file info: {FilePath}", filePath);
            return null;
        }
    }

    public async Task<List<string>> ListFilesAsync(string directoryType, string pattern = "*")
    {
        try
        {
            var directory = Path.Combine(_dataPath, directoryType);
            
            if (!Directory.Exists(directory))
                return new List<string>();

            return Directory.GetFiles(directory, pattern).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list files in directory: {DirectoryType}", directoryType);
            return new List<string>();
        }
    }

    public string GenerateUniqueFileName(string baseName, string extension)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        
        return $"{baseName}_{timestamp}_{uniqueId}{extension}";
    }

    public string GetDirectoryPath(string directoryType)
    {
        return Path.Combine(_dataPath, directoryType);
    }

    public async Task<int> CleanupOldFilesAsync(string directoryType, DateTime olderThan)
    {
        try
        {
            var directory = Path.Combine(_dataPath, directoryType);
            
            if (!Directory.Exists(directory))
                return 0;

            var files = Directory.GetFiles(directory);
            var deletedCount = 0;

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.CreationTime < olderThan)
                {
                    File.Delete(file);
                    deletedCount++;
                    _logger.LogDebug("Cleaned up old file: {FilePath}", file);
                }
            }

            _logger.LogInformation("Cleaned up {Count} old files from {Directory}", deletedCount, directoryType);
            return deletedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup old files in directory: {DirectoryType}", directoryType);
            return 0;
        }
    }
}