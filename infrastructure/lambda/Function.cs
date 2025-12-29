using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using OpenAI;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ReceiptParserLambda;

public class Function
{
    private readonly AmazonSecretsManagerClient _secretsClient;
    private readonly HttpClient _httpClient;
    private string? _openaiApiKey;
    private OpenAIClient? _openAIClient;
    private const string OPENAI_MODEL = "gpt-4.1-mini";

    public Function()
    {
        _secretsClient = new AmazonSecretsManagerClient();
        _httpClient = new HttpClient();
    }

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        LambdaLogger.Log($"Request: {request.HttpMethod} {request.Path}");

        try
        {
            // Route requests based on path
            var proxyPath = request.PathParameters?.ContainsKey("proxy") == true 
                ? request.PathParameters["proxy"]
                : "";

            // Handle different endpoints
            if (proxyPath.StartsWith("hello", StringComparison.OrdinalIgnoreCase))
            {
                return HandleHelloRequest(proxyPath);
            }
            else if (proxyPath.StartsWith("receipt/extract", StringComparison.OrdinalIgnoreCase))
            {
                return await HandleReceiptExtractRequest(request);
            }
            else
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = 404,
                    Body = JsonSerializer.Serialize(new { error = "Not Found" }),
                    Headers = CorsHeaders()
                };
            }
        }
        catch (Exception ex)
        {
            LambdaLogger.Log($"Error: {ex.Message}\n{ex.StackTrace}");
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = JsonSerializer.Serialize(new { error = "Internal server error", message = ex.Message }),
                Headers = CorsHeaders()
            };
        }
    }

    private APIGatewayProxyResponse HandleHelloRequest(string path)
    {
        var response = new
        {
            message = "Hello World from .NET Lambda!",
            timestamp = DateTime.UtcNow
        };

        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = JsonSerializer.Serialize(response),
            Headers = CorsHeaders("application/json")
        };
    }

    private async Task<APIGatewayProxyResponse> HandleReceiptExtractRequest(APIGatewayProxyRequest request)
    {
        try
        {
            // Load OpenAI API key from Secrets Manager (cached)
            _openaiApiKey ??= await GetOpenAIApiKey();

            // Check if OpenAI API key is configured
            if (string.IsNullOrEmpty(_openaiApiKey))
            {
                LambdaLogger.Log("OpenAI API key not configured, returning mock data");
                return GenerateMockDataResponse();
            }

            // Initialize OpenAI client
            _openAIClient ??= new OpenAIClient(_openaiApiKey);

            // Parse multipart form data - get body as bytes first
            var bodyBytes = request.IsBase64Encoded 
                ? Convert.FromBase64String(request.Body ?? "")
                : Encoding.UTF8.GetBytes(request.Body ?? "");

            if (bodyBytes.Length == 0)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = 400,
                    Body = JsonSerializer.Serialize(new { error = "Request body is required" }),
                    Headers = CorsHeaders()
                };
            }

            // Parse multipart form data
            var boundary = ExtractBoundary(request.Headers);
            if (string.IsNullOrEmpty(boundary))
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = 400,
                    Body = JsonSerializer.Serialize(new { error = "Invalid Content-Type header" }),
                    Headers = CorsHeaders()
                };
            }

            var imageBytes = ExtractImageFromMultipartBytes(bodyBytes, boundary);
            if (imageBytes == null || imageBytes.Length == 0)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = 400,
                    Body = JsonSerializer.Serialize(new { error = "No image file provided" }),
                    Headers = CorsHeaders()
                };
            }

            LambdaLogger.Log($"Processing receipt image, size: {imageBytes.Length} bytes");

            // Detect image format from the binary data
            var mimeType = DetectImageMimeType(imageBytes);

            // Create the vision chat completion request
            var messages = new List<ChatMessage>
            {
                new UserChatMessage(new List<ChatMessageContentPart>
                {
                    ChatMessageContentPart.CreateTextPart(CreateExtractionPrompt()),
                    ChatMessageContentPart.CreateImagePart(BinaryData.FromBytes(imageBytes), mimeType)
                })
            };

            LambdaLogger.Log("Sending request to OpenAI Vision API");

            // Call OpenAI API
            var response = await _openAIClient.GetChatClient(OPENAI_MODEL).CompleteChatAsync(messages);

            if (response?.Value?.Content?.Count > 0)
            {
                var content = response.Value.Content[0].Text;
                var extractedData = ParseExtractionResponse(content);

                LambdaLogger.Log("Successfully extracted data from receipt");

                return new APIGatewayProxyResponse
                {
                    StatusCode = 200,
                    Body = JsonSerializer.Serialize(new
                    {
                        extractedData,
                        processingStatus = "Success",
                        processedAt = DateTime.UtcNow
                    }),
                    Headers = CorsHeaders("application/json")
                };
            }

            LambdaLogger.Log("No content received from OpenAI API");
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = JsonSerializer.Serialize(new { error = "No content received from OpenAI API" }),
                Headers = CorsHeaders()
            };
        }
        catch (Exception ex)
        {
            LambdaLogger.Log($"Error processing receipt: {ex.Message}\n{ex.StackTrace}");
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = JsonSerializer.Serialize(new { error = "Failed to process receipt", message = ex.Message }),
                Headers = CorsHeaders()
            };
        }
    }

    private APIGatewayProxyResponse GenerateMockDataResponse()
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

        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = JsonSerializer.Serialize(new
            {
                extractedData = mockData,
                processingStatus = "Success",
                processedAt = DateTime.UtcNow
            }),
            Headers = CorsHeaders("application/json")
        };
    }

    private string? ExtractBoundary(IDictionary<string, string>? headers)
    {
        if (headers == null) return null;

        var contentType = headers.FirstOrDefault(h => 
            h.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)).Value;

        if (string.IsNullOrEmpty(contentType)) return null;

        var boundaryPrefix = "boundary=";
        var boundaryIndex = contentType.IndexOf(boundaryPrefix, StringComparison.OrdinalIgnoreCase);
        if (boundaryIndex == -1) return null;

        return contentType.Substring(boundaryIndex + boundaryPrefix.Length).Trim('"');
    }

    private byte[]? ExtractImageFromMultipartBytes(byte[] bodyBytes, string boundary)
    {
        try
        {
            var boundaryBytes = Encoding.UTF8.GetBytes($"--{boundary}");
            var doubleCrlfBytes = Encoding.UTF8.GetBytes("\r\n\r\n");
            var doubleLfBytes = Encoding.UTF8.GetBytes("\n\n");
            var crlfBytes = Encoding.UTF8.GetBytes("\r\n");
            var lfBytes = Encoding.UTF8.GetBytes("\n");

            // Find all boundary positions
            var parts = new List<(int start, int end)>();
            var currentPos = 0;

            while (currentPos < bodyBytes.Length)
            {
                var boundaryPos = FindBytes(bodyBytes, boundaryBytes, currentPos);
                if (boundaryPos == -1) break;

                var nextBoundaryPos = FindBytes(bodyBytes, boundaryBytes, boundaryPos + boundaryBytes.Length);
                if (nextBoundaryPos == -1) nextBoundaryPos = bodyBytes.Length;

                parts.Add((boundaryPos, nextBoundaryPos));
                currentPos = nextBoundaryPos;
            }

            // Find the part with the image
            foreach (var (partStart, partEnd) in parts)
            {
                var partBytes = new byte[partEnd - partStart];
                Array.Copy(bodyBytes, partStart, partBytes, 0, partBytes.Length);
                var partString = Encoding.UTF8.GetString(partBytes);

                if (partString.Contains("filename="))
                {
                    // Find header end
                    var headerEndIndex = FindBytes(partBytes, doubleCrlfBytes, 0);
                    if (headerEndIndex == -1) headerEndIndex = FindBytes(partBytes, doubleLfBytes, 0);

                    if (headerEndIndex != -1)
                    {
                        var dataStart = headerEndIndex == FindBytes(partBytes, doubleCrlfBytes, 0) ? headerEndIndex + 4 : headerEndIndex + 2;

                        // Find data end (before next boundary or end of part)
                        var dataEnd = FindBytes(partBytes, crlfBytes, dataStart);
                        if (dataEnd == -1) dataEnd = FindBytes(partBytes, lfBytes, dataStart);
                        if (dataEnd == -1) dataEnd = partBytes.Length;

                        if (dataEnd > dataStart)
                        {
                            var imageData = new byte[dataEnd - dataStart];
                            Array.Copy(partBytes, dataStart, imageData, 0, imageData.Length);
                            
                            LambdaLogger.Log($"Extracted image: {imageData.Length} bytes");
                            return imageData;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LambdaLogger.Log($"Error extracting image from multipart bytes: {ex.Message}");
        }

        return null;
    }

    private static int FindBytes(byte[] source, byte[] pattern, int startIndex = 0)
    {
        for (int i = startIndex; i <= source.Length - pattern.Length; i++)
        {
            bool match = true;
            for (int j = 0; j < pattern.Length; j++)
            {
                if (source[i + j] != pattern[j])
                {
                    match = false;
                    break;
                }
            }
            if (match) return i;
        }
        return -1;
    }

    private string DetectImageMimeType(byte[] imageBytes)
    {
        // PNG magic bytes: 89 50 4E 47
        if (imageBytes.Length >= 4 && imageBytes[0] == 0x89 && imageBytes[1] == 0x50 && 
            imageBytes[2] == 0x4E && imageBytes[3] == 0x47)
        {
            LambdaLogger.Log("Detected PNG image format");
            return "image/png";
        }

        // JPEG magic bytes: FF D8 FF
        if (imageBytes.Length >= 3 && imageBytes[0] == 0xFF && imageBytes[1] == 0xD8 && imageBytes[2] == 0xFF)
        {
            LambdaLogger.Log("Detected JPEG image format");
            return "image/jpeg";
        }

        // GIF magic bytes: 47 49 46
        if (imageBytes.Length >= 3 && imageBytes[0] == 0x47 && imageBytes[1] == 0x49 && imageBytes[2] == 0x46)
        {
            LambdaLogger.Log("Detected GIF image format");
            return "image/gif";
        }

        // Default to JPEG
        LambdaLogger.Log("Image format not recognized, defaulting to image/jpeg");
        return "image/jpeg";
    }

    private byte[]? ExtractImageFromMultipart(string body, string boundary)
    {
        try
        {
            var parts = body.Split(new[] { $"--{boundary}" }, StringSplitOptions.None);

            foreach (var part in parts)
            {
                if (part.Contains("filename="))
                {
                    // Find the start of the actual image data (after headers and blank line)
                    var headerEndIndex = part.IndexOf("\r\n\r\n");
                    if (headerEndIndex == -1) headerEndIndex = part.IndexOf("\n\n");

                    if (headerEndIndex != -1)
                    {
                        var dataStart = headerEndIndex + 4;
                        var dataEnd = part.LastIndexOf("\r\n");
                        if (dataEnd == -1) dataEnd = part.LastIndexOf("\n");

                        if (dataEnd > dataStart)
                        {
                            var imageData = part.Substring(dataStart, dataEnd - dataStart);
                            return Encoding.UTF8.GetBytes(imageData);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LambdaLogger.Log($"Error extracting image from multipart: {ex.Message}");
        }

        return null;
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
            LambdaLogger.Log($"Failed to parse OpenAI response as JSON: {ex.Message}");
        }

        // Fallback: return raw response as single key-value
        return new Dictionary<string, object> { ["raw_response"] = response };
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

    private Dictionary<string, string> CorsHeaders(string contentType = "application/json")
    {
        return new Dictionary<string, string>
        {
            { "Access-Control-Allow-Origin", "*" },
            { "Access-Control-Allow-Methods", "GET,HEAD,OPTIONS,POST,PUT,DELETE" },
            { "Access-Control-Allow-Headers", "*" },
            { "Content-Type", contentType }
        };
    }

    private async Task<string> GetOpenAIApiKey()
    {
        try
        {
            var secretName = Environment.GetEnvironmentVariable("OPENAI_API_KEY_SECRET");
            if (string.IsNullOrEmpty(secretName))
            {
                throw new InvalidOperationException("OPENAI_API_KEY_SECRET environment variable not set");
            }

            var request = new GetSecretValueRequest { SecretId = secretName };
            var response = await _secretsClient.GetSecretValueAsync(request);
            return response.SecretString;
        }
        catch (Exception ex)
        {
            LambdaLogger.Log($"Failed to retrieve OpenAI API key: {ex.Message}");
            throw;
        }
    }
}
