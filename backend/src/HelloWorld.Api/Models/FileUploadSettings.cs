namespace HelloWorld.Api.Models;

public class FileUploadSettings
{
    public long MaxSizeBytes { get; set; } = 10485760; // 10MB
    public string[] AllowedExtensions { get; set; } = [".png", ".jpg", ".jpeg"];
}
