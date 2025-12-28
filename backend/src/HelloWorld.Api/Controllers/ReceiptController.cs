using Microsoft.AspNetCore.Mvc;
using HelloWorld.Api.Models;
using HelloWorld.Api.Services;
using Microsoft.Extensions.Options;

namespace HelloWorld.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReceiptController : ControllerBase
{
    private readonly IOpenAIService _openAIService;
    private readonly FileUploadSettings _uploadSettings;
    private readonly ILogger<ReceiptController> _logger;

    public ReceiptController(
        IOpenAIService openAIService, 
        IOptions<FileUploadSettings> uploadSettings,
        ILogger<ReceiptController> logger)
    {
        _openAIService = openAIService;
        _uploadSettings = uploadSettings.Value;
        _logger = logger;
    }

    [HttpPost("extract")]
    [RequestSizeLimit(10_000_000)] // 10MB limit
    [RequestFormLimits(MultipartBodyLengthLimit = 10_000_000)]
    public async Task<IActionResult> ExtractReceiptData(IFormFile file)
    {
        try
        {
            // Validate file
            var validationResult = ValidateFile(file);
            if (validationResult != null)
            {
                return validationResult;
            }

            _logger.LogInformation("Processing receipt image: {FileName}", file.FileName);

            // Process with OpenAI
            var result = await _openAIService.ExtractReceiptDataAsync(file);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing receipt extraction request");
            return StatusCode(500, new ReceiptExtractionResponse(
                new Dictionary<string, object>(),
                "Error",
                DateTime.UtcNow,
                "Internal server error occurred while processing the image"
            ));
        }
    }

    private IActionResult? ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new ReceiptExtractionResponse(
                new Dictionary<string, object>(),
                "Error",
                DateTime.UtcNow,
                "No file provided"
            ));
        }

        if (file.Length > _uploadSettings.MaxSizeBytes)
        {
            return BadRequest(new ReceiptExtractionResponse(
                new Dictionary<string, object>(),
                "Error",
                DateTime.UtcNow,
                $"File size exceeds maximum allowed size of {_uploadSettings.MaxSizeBytes / 1024 / 1024}MB"
            ));
        }

        var fileExtension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(fileExtension) || !_uploadSettings.AllowedExtensions.Contains(fileExtension))
        {
            return BadRequest(new ReceiptExtractionResponse(
                new Dictionary<string, object>(),
                "Error",
                DateTime.UtcNow,
                $"File type not supported. Allowed types: {string.Join(", ", _uploadSettings.AllowedExtensions)}"
            ));
        }

        return null;
    }
}