using HelloWorld.Api.Models;

namespace HelloWorld.Api.Services;

public interface IOpenAIService
{
    Task<ReceiptExtractionResponse> ExtractReceiptDataAsync(IFormFile imageFile);
}
