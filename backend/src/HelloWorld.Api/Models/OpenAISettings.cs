namespace HelloWorld.Api.Models;

public class OpenAISettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4-vision-preview";
    public int MaxTokens { get; set; } = 4096;
    public double Temperature { get; set; } = 0.1;
}
