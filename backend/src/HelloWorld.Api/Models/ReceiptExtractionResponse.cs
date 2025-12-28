namespace HelloWorld.Api.Models;

public record ReceiptExtractionResponse(
    Dictionary<string, object> ExtractedData,
    string ProcessingStatus,
    DateTime ProcessedAt,
    string? ErrorMessage = null
);
