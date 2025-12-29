using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ReceiptParserLambda;

public class Function
{
    private readonly AmazonSecretsManagerClient _secretsClient;
    private readonly HttpClient _httpClient;
    private string? _openaiApiKey;

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
            var path = request.Path ?? "";
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
        // Load OpenAI API key from Secrets Manager (cached)
        _openaiApiKey ??= await GetOpenAIApiKey();

        // Parse the request body
        var body = request.IsBase64Encoded 
            ? Encoding.UTF8.GetString(Convert.FromBase64String(request.Body ?? ""))
            : request.Body;

        if (string.IsNullOrEmpty(body))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = JsonSerializer.Serialize(new { error = "Request body is required" }),
                Headers = CorsHeaders()
            };
        }

        try
        {
            // Parse the FormData or JSON body
            // For now, return a placeholder response
            var response = new
            {
                success = true,
                data = new
                {
                    vendor = "Sample Vendor",
                    total = 99.99,
                    date = DateTime.UtcNow.ToString("yyyy-MM-dd")
                }
            };

            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = JsonSerializer.Serialize(response),
                Headers = CorsHeaders("application/json")
            };
        }
        catch (Exception ex)
        {
            LambdaLogger.Log($"Error processing receipt: {ex.Message}");
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = JsonSerializer.Serialize(new { error = "Failed to process receipt", message = ex.Message }),
                Headers = CorsHeaders()
            };
        }
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
