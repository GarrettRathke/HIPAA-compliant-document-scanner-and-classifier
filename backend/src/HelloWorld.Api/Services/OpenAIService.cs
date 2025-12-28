using HelloWorld.Api.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;
using OpenAI;
using OpenAI.Chat;

namespace HelloWorld.Api.Services;

public class OpenAIService : IOpenAIService
{
    private readonly OpenAISettings _settings;
    private readonly ILogger<OpenAIService> _logger;
    private readonly OpenAIClient _openAIClient;

    public OpenAIService(IOptions<OpenAISettings> settings, ILogger<OpenAIService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _openAIClient = new OpenAIClient(_settings.ApiKey);
    }

    public async Task<ReceiptExtractionResponse> ExtractReceiptDataAsync(IFormFile imageFile)
    {
        try
        {
            _logger.LogInformation("Processing receipt image: {FileName}, Size: {FileSize} bytes", 
                imageFile.FileName, imageFile.Length);

            // Validate file
            if (imageFile == null || imageFile.Length == 0)
            {
                return new ReceiptExtractionResponse(
                    new Dictionary<string, object>(),
                    "Error",
                    DateTime.UtcNow,
                    "No file provided or file is empty"
                );
            }

            // Check if OpenAI API key is configured
            if (string.IsNullOrEmpty(_settings.ApiKey) || _settings.ApiKey == "your-openai-api-key-here")
            {
                _logger.LogWarning("OpenAI API key not configured, returning mock data");
                return await GenerateMockDataAsync();
            }

            // Validate image content type
            if (!IsValidImageType(imageFile.ContentType))
            {
                return new ReceiptExtractionResponse(
                    new Dictionary<string, object>(),
                    "Error",
                    DateTime.UtcNow,
                    $"Unsupported file type: {imageFile.ContentType}. Supported types: PNG, JPEG, JPG"
                );
            }

            // Convert image to bytes for OpenAI API
            var imageBytes = await ConvertToByteArrayAsync(imageFile);
            var mimeType = GetMimeType(imageFile.ContentType);

            // Create the vision chat completion request
            var messages = new List<ChatMessage>
            {
                new UserChatMessage(new List<ChatMessageContentPart>
                {
                    ChatMessageContentPart.CreateTextPart(CreateExtractionPrompt()),
                    ChatMessageContentPart.CreateImagePart(BinaryData.FromBytes(imageBytes), mimeType)
                })
            };

            _logger.LogInformation("Sending request to OpenAI Vision API");

            // Call OpenAI API with messages directly  
            var response = await _openAIClient.GetChatClient(_settings.Model).CompleteChatAsync(messages);

            if (response?.Value?.Content?.Count > 0)
            {
                var content = response.Value.Content[0].Text;
                var extractedData = ParseExtractionResponse(content);

                _logger.LogInformation("Successfully extracted data from receipt");

                return new ReceiptExtractionResponse(
                    extractedData,
                    "Success",
                    DateTime.UtcNow
                );
            }

            _logger.LogWarning("No content received from OpenAI API");
            return new ReceiptExtractionResponse(
                new Dictionary<string, object> { ["message"] = "No content extracted from image" },
                "Error",
                DateTime.UtcNow,
                "No content received from OpenAI API"
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

    private async Task<ReceiptExtractionResponse> GenerateMockDataAsync()
    {
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

    private static string GetMimeType(string contentType)
    {
        return contentType switch
        {
            "image/png" => "image/png",
            "image/jpeg" => "image/jpeg",
            "image/jpg" => "image/jpeg",
            _ => "image/jpeg"
        };
    }

    private static bool IsValidImageType(string contentType)
    {
        return contentType switch
        {
            "image/png" or "image/jpeg" or "image/jpg" => true,
            _ => false
        };
    }

    private async Task<byte[]> ConvertToByteArrayAsync(IFormFile file)
    {
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    private string CreateExtractionPrompt()
    {
        return """
        You are an expert at analyzing receipts and invoices. Please carefully examine this image and extract ALL visible text and data into a structured JSON format.

        Extract the following information if available:
        - Business information: name, address, phone number, website
        - Transaction details: date, time, receipt/invoice number, order number
        - All line items: product names, quantities, individual prices
        - Financial details: subtotal, tax amounts, discounts, tips, final total
        - Payment information: payment method, card details (if safe to include)
        - Any additional text, numbers, or codes visible on the receipt

        Guidelines:
        1. For unclear or partially visible text, provide your best interpretation
        2. Use descriptive keys (e.g., "item_1_name", "item_1_price", "item_1_quantity")
        3. Keep all monetary values as strings with currency symbols if present
        4. Include dates in ISO format (YYYY-MM-DD) when possible
        5. If you see multiple similar items, number them sequentially
        6. Extract any barcodes, QR codes, or reference numbers you can see

        Return ONLY a valid JSON object with descriptive string keys. Example format:
        {
          "business_name": "Coffee Corner",
          "business_address": "123 Main Street, City, State 12345",
          "business_phone": "(555) 123-4567",
          "transaction_date": "2024-12-28",
          "transaction_time": "14:35",
          "receipt_number": "R12345",
          "item_1_name": "Large Cappuccino",
          "item_1_price": "$4.50",
          "item_2_name": "Blueberry Muffin",
          "item_2_price": "$3.25",
          "subtotal": "$7.75",
          "tax": "$0.62",
          "total": "$8.37",
          "payment_method": "Credit Card",
          "card_last_four": "1234"
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
