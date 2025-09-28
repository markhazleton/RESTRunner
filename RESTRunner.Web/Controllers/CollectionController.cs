using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using RESTRunner.Web.Services;
using RESTRunner.Web.Models.ViewModels;
using System.Diagnostics;

namespace RESTRunner.Web.Controllers
{
    /// <summary>
    /// Controller for managing Postman collections
    /// </summary>
    public class CollectionController : Controller
    {
        private readonly ICollectionService _collectionService;
        private readonly ILogger<CollectionController> _logger;

        public CollectionController(ICollectionService collectionService, ILogger<CollectionController> logger)
        {
            _collectionService = collectionService;
            _logger = logger;
        }

        /// <summary>
        /// Display list of collections
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var collections = await _collectionService.GetAllAsync();
                return View(collections);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading collections");
                TempData["Error"] = "Failed to load collections";
                return View(new List<RESTRunner.Web.Models.CollectionMetadata>());
            }
        }

        /// <summary>
        /// Display collection details
        /// </summary>
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            try
            {
                var metadata = await _collectionService.GetByIdAsync(id);
                if (metadata == null)
                    return NotFound();

                var structure = await _collectionService.GetStructureAsync(id);
                
                var viewModel = new CollectionDetailsViewModel
                {
                    Metadata = metadata,
                    Structure = structure ?? new CollectionStructure()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading collection details {Id}", id);
                TempData["Error"] = "Failed to load collection details";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Show upload collection form
        /// </summary>
        public IActionResult Upload()
        {
            return View(new CollectionUploadViewModel());
        }

        /// <summary>
        /// Diagnostic endpoint to check upload configuration
        /// </summary>
        public IActionResult UploadDiagnostics()
        {
            var diagnostics = new
            {
                MaxRequestBodySize = HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>()?.MaxRequestBodySize,
                FormOptions = HttpContext.RequestServices.GetService<IOptions<FormOptions>>()?.Value,
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                ServerType = HttpContext.Request.Headers.ContainsKey("Server") ? HttpContext.Request.Headers["Server"].ToString() : "Unknown",
                ContentLength = HttpContext.Request.ContentLength,
                ContentType = HttpContext.Request.ContentType,
                Method = HttpContext.Request.Method,
                Timestamp = DateTime.UtcNow
            };

            return Json(diagnostics);
        }

        /// <summary>
        /// Handle collection upload
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit
        [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
        public async Task<IActionResult> Upload(CollectionUploadViewModel viewModel)
        {
            _logger.LogInformation("Upload attempt started for collection: {Name}", viewModel?.Name ?? "Unknown");

            // Check model state first
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model validation failed for upload. Errors: {Errors}", 
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return View(viewModel);
            }

            try
            {
                // Validate file existence and basic properties
                if (viewModel.CollectionFile == null)
                {
                    _logger.LogWarning("No file provided in upload request");
                    ModelState.AddModelError("CollectionFile", "Please select a file to upload");
                    return View(viewModel);
                }

                if (viewModel.CollectionFile.Length == 0)
                {
                    _logger.LogWarning("Empty file provided in upload request");
                    ModelState.AddModelError("CollectionFile", "The selected file is empty");
                    return View(viewModel);
                }

                if (viewModel.CollectionFile.Length > 10 * 1024 * 1024) // 10MB
                {
                    _logger.LogWarning("File too large: {Size} bytes", viewModel.CollectionFile.Length);
                    ModelState.AddModelError("CollectionFile", "File size cannot exceed 10MB");
                    return View(viewModel);
                }

                if (!viewModel.CollectionFile.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Invalid file extension: {FileName}", viewModel.CollectionFile.FileName);
                    ModelState.AddModelError("CollectionFile", "Only JSON files are allowed");
                    return View(viewModel);
                }

                _logger.LogInformation("File validation passed. File: {FileName}, Size: {Size} bytes", 
                    viewModel.CollectionFile.FileName, viewModel.CollectionFile.Length);

                // Validate collection content
                var validation = await _collectionService.ValidateAsync(viewModel.CollectionFile);
                if (!validation.IsValid)
                {
                    _logger.LogWarning("Collection validation failed. Errors: {Errors}", 
                        string.Join(", ", validation.Errors));

                    foreach (var error in validation.Errors)
                        ModelState.AddModelError("", error);

                    foreach (var warning in validation.Warnings)
                        TempData["Warning"] = warning;

                    return View(viewModel);
                }

                _logger.LogInformation("Collection validation passed. RequestCount: {RequestCount}", validation.RequestCount);

                // Create metadata
                var metadata = new RESTRunner.Web.Models.CollectionMetadata
                {
                    Name = viewModel.Name,
                    Description = viewModel.Description,
                    FileName = viewModel.CollectionFile.FileName,
                    Tags = viewModel.GetTags(),
                    PostmanId = validation.PostmanId,
                    SchemaVersion = validation.SchemaVersion,
                    RequestCount = validation.RequestCount,
                    EnvironmentVariables = validation.EnvironmentVariables,
                    HttpMethods = validation.HttpMethods,
                    FileSize = viewModel.CollectionFile.Length
                };

                // Upload and save
                var savedMetadata = await _collectionService.UploadAsync(viewModel.CollectionFile, metadata);
                
                _logger.LogInformation("Collection uploaded successfully: {Name} ({Id})", 
                    savedMetadata.Name, savedMetadata.Id);
                
                TempData["Success"] = $"Collection '{savedMetadata.Name}' uploaded successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading collection: {Name}", viewModel?.Name ?? "Unknown");
                ModelState.AddModelError("", $"Failed to upload collection: {ex.Message}");
                return View(viewModel);
            }
        }

        /// <summary>
        /// Show edit collection form
        /// </summary>
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            try
            {
                var collection = await _collectionService.GetByIdAsync(id);
                if (collection == null)
                    return NotFound();

                return View(collection);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading collection for edit {Id}", id);
                TempData["Error"] = "Failed to load collection";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Handle collection metadata update
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, RESTRunner.Web.Models.CollectionMetadata collection)
        {
            if (id != collection.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(collection);

            try
            {
                await _collectionService.UpdateAsync(collection);
                TempData["Success"] = $"Collection '{collection.Name}' updated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating collection {Id}", id);
                ModelState.AddModelError("", "Failed to update collection");
                return View(collection);
            }
        }

        /// <summary>
        /// Show delete confirmation
        /// </summary>
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            try
            {
                var collection = await _collectionService.GetByIdAsync(id);
                if (collection == null)
                    return NotFound();

                return View(collection);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading collection for delete {Id}", id);
                TempData["Error"] = "Failed to load collection";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Handle collection deletion
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var collection = await _collectionService.GetByIdAsync(id);
                if (collection == null)
                    return NotFound();

                await _collectionService.DeleteAsync(id);
                TempData["Success"] = $"Collection '{collection.Name}' deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting collection {Id}", id);
                TempData["Error"] = "Failed to delete collection";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Download collection file
        /// </summary>
        public async Task<IActionResult> Download(string id)
        {
            try
            {
                var metadata = await _collectionService.GetByIdAsync(id);
                if (metadata == null)
                    return NotFound();

                var content = await _collectionService.GetContentAsync(id);
                if (content == null)
                    return NotFound();

                return File(System.Text.Encoding.UTF8.GetBytes(content), "application/json", metadata.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading collection {Id}", id);
                TempData["Error"] = "Failed to download collection";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// View collection structure (JSON viewer)
        /// </summary>
        public async Task<IActionResult> View(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            try
            {
                var content = await _collectionService.GetContentAsync(id);
                if (content == null)
                    return NotFound();

                ViewBag.CollectionJson = content;
                ViewBag.CollectionId = id;
                
                var metadata = await _collectionService.GetByIdAsync(id);
                ViewBag.CollectionName = metadata?.Name ?? "Unknown";

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error viewing collection {Id}", id);
                TempData["Error"] = "Failed to load collection content";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}