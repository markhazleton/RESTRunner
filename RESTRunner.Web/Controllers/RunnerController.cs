using Microsoft.AspNetCore.Mvc;
using RESTRunner.Web.Services;
using RESTRunner.Web.Models.ViewModels;
using System.Diagnostics;

namespace RESTRunner.Web.Controllers
{
    /// <summary>
    /// Controller for the RESTRunner dashboard and main functionality
    /// </summary>
    public class RunnerController : Controller
    {
        private readonly IConfigurationService _configurationService;
        private readonly ICollectionService _collectionService;
        private readonly IExecutionService _executionService;
        private readonly ILogger<RunnerController> _logger;

        public RunnerController(
            IConfigurationService configurationService,
            ICollectionService collectionService,
            IExecutionService executionService,
            ILogger<RunnerController> logger)
        {
            _configurationService = configurationService;
            _collectionService = collectionService;
            _executionService = executionService;
            _logger = logger;
        }

        /// <summary>
        /// Main dashboard for RESTRunner
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var dashboardViewModel = new DashboardViewModel();

                // Load configurations
                var configurations = await _configurationService.GetAllAsync();
                dashboardViewModel.Configurations = configurations.Take(10).Select(c => new ConfigurationSummary
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    TotalTestCount = c.GetTotalTestCount(),
                    ModifiedAt = c.ModifiedAt,
                    IsActive = c.IsActive,
                    Tags = c.Tags
                }).ToList();

                // Load collections if service is available
                if (_collectionService != null)
                {
                    var collections = await _collectionService.GetAllAsync();
                    dashboardViewModel.Collections = collections.Take(10).Select(c => new CollectionSummary
                    {
                        Id = c.Id,
                        Name = c.Name,
                        FileName = c.FileName,
                        RequestCount = c.RequestCount,
                        UploadedAt = c.UploadedAt,
                        IsActive = c.IsActive,
                        FileSize = c.FileSize,
                        Tags = c.Tags
                    }).ToList();
                }

                // Load recent executions if service is available
                if (_executionService != null)
                {
                    var recentExecutions = await _executionService.GetRecentExecutionsAsync(5);
                    dashboardViewModel.RecentExecutions = recentExecutions.Select(e => new ExecutionSummary
                    {
                        Id = e.Id,
                        ConfigurationName = e.ConfigurationName,
                        StartTime = e.StartTime,
                        EndTime = e.EndTime,
                        Status = e.Status,
                        Duration = e.Duration,
                        SuccessRate = e.Statistics?.SuccessRate,
                        AverageResponseTime = e.Statistics?.AverageResponseTime,
                        TotalRequests = e.Statistics?.TotalRequests,
                        ExecutedBy = e.ExecutedBy
                    }).ToList();

                    // Load running executions
                    var runningExecutions = await _executionService.GetRunningExecutionsAsync();
                    dashboardViewModel.RunningExecutions = runningExecutions;
                }

                // Calculate statistics
                dashboardViewModel.Statistics = new SystemStatistics
                {
                    TotalConfigurations = configurations.Count,
                    ActiveConfigurations = configurations.Count(c => c.IsActive),
                    TotalCollections = dashboardViewModel.Collections.Count,
                    TotalExecutions = dashboardViewModel.RecentExecutions.Count,
                    RunningExecutions = dashboardViewModel.RunningExecutions.Count,
                    LastExecution = dashboardViewModel.RecentExecutions.FirstOrDefault()?.StartTime,
                    AverageSuccessRate = dashboardViewModel.RecentExecutions.Where(e => e.SuccessRate.HasValue).Average(e => e.SuccessRate ?? 0),
                    SystemUptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()
                };

                return View(dashboardViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                TempData["Error"] = "Failed to load dashboard data";
                return View(new DashboardViewModel());
            }
        }

        /// <summary>
        /// Quick start guide
        /// </summary>
        public IActionResult QuickStart()
        {
            return View();
        }

        /// <summary>
        /// System settings and configuration
        /// </summary>
        public IActionResult Settings()
        {
            return View();
        }

        /// <summary>
        /// API status and health check
        /// </summary>
        public async Task<IActionResult> Status()
        {
            var status = new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0",
                Uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime(),
                Services = new
                {
                    ConfigurationService = _configurationService != null ? "Available" : "Unavailable",
                    CollectionService = _collectionService != null ? "Available" : "Unavailable",
                    ExecutionService = _executionService != null ? "Available" : "Unavailable"
                }
            };

            return Json(status);
        }
    }
}