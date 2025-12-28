using HelloWorld.Api.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace HelloWorld.Api.Services;

public class OpenAIService : IOpenAIService
{
    private readonly OpenAISettings _settings;
    private readonly ILogger<OpenAIService> _logger;

    public OpenAIService(IOptions<OpenAISettings> settings, ILogger<OpenAIService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<ReceiptExtractionResponse> ExtractReceiptDataAsync(IFormFile imageFile)
    {
        try
        {
            _logger.LogInformation("Processing receipt image: {FileName}", imageFile.FileName);

            // For now, return mock data until OpenAI API key is configured
            if (string.IsNullOrEmpty(_settings.ApiKey) || _settings.ApiKey == "your-openai-api-key-here")
            {
                _logger.LogWarning("OpenAI API key not configured, returning mock data");
                
                var mockData = new Dictionary<string, object>
                {
                    ["business_name"] = "Demo Coffee Shop",
                    ["date"] = DateTime.Now.ToString("yyyy-MM-dd"),
                    ["time"] = DateTime.Now.ToString("HH:mm"),
                    ["total"] = "15.47",
                    ["item_1"] = "Large Coffee - $4.50",
                    ["item_2"] = "Blueberry Muffin - $3.25",
                    ["item_3"] = "Sandwich - $7.72",
                    ["subtotal"] = "15.47",
                    ["tax"] = "0.00",
                    ["payment_method"] = "Credit Card",
                    ["receipt_number"] = "12345",
                    ["note"] = "Mock data - configure OpenAI API key for real extraction"
                };

                return new ReceiptExtractionResponse(
                    mockData,
                    "Success",
                    DateTime.UtcNow
                );
            }

            // TODO: Implement real OpenAI Vision API when API key is configured
            // For now, return an error message prompting for API key configuration
            return new ReceiptExtractionResponse(
                new Dictionary<string, object>
                {
                    ["message"] = "OpenAI Vision API integration requires configuration",
                    ["instruction"] = "Set your OpenAI API key in appsettings.Development.json"
                },
                "Error",
                DateTime.UtcNow,
                "OpenAI API key not properly configured"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting receipt data");
            return new ReceiptExtractionResponse(
                new Dictionary<string, object>(),
                "Error",
                DateTime.UtcNow,
                ex.Message
            );
        }
    }

    private async Task<string> ConvertToBase64Async(IFormFile file)
    {
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var bytes = memoryStream.ToArray();
        return Convert.ToBase64String(bytes);
    }

    private string CreateExtractionPrompt()
    {
        return """
        Analyze this receipt or invoice image and extract ALL visible text data into a JSON format with flexible key-value pairs. 
        
        Extract everything you can see including:
        - Business/vendor information (name, address, phone, etc.)
        - Transaction details (date, time, receipt number, etc.)
        - All line items with descriptions and prices
        - Subtotal, tax, tips, total amounts
        - Any other visible text or numbers
        
        For unclear or partial text, include your best interpretation. Choose the most probable option for ambiguous text.
        
        Return ONLY a valid JSON object with string keys and values (numbers should be strings). Example format:
        {
          "business_name": "Store Name",
          "date": "2024-01-15",
          "total": "25.99",
          "item_1": "Coffee - $4.50",
          "item_2": "Sandwich - $12.99",
          "tax": "2.15",
          "address": "123 Main St"
        }
        """;
    }

    private Dictionary<string, object> ParseExtractionResponse(string response)
    {
        try
        {
            // Clean up the response to extract just the JSON
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}') + 1;
            
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonString = response.Substring(jsonStart, jsonEnd - jsonStart);
                var result = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);
                return result ?? new Dictionary<string, object>();
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse OpenAI response as JSON: {Response}", response);
        }

        // Fallback: return raw response as single key-value
        return new Dictionary<string, object> { ["raw_response"] = response };
    }
}
