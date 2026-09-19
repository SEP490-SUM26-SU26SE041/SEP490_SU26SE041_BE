namespace SmartFarmSEP490.Model.DTOs;

/// <summary>
/// Config cho 2 AI microservice trong appsettings.json:
///   "AI": {
///     "TomatoUrl": "https://tomato-onnx-backend.onrender.com",
///     "ArgoPestUrl": "https://argo-pest-api.onrender.com",
///     "TimeoutSeconds": 120,
///     "MaxRetries": 3
///   }
/// </summary>
public class AIOptions
{
    public const string SectionName = "AI";

    public string TomatoUrl { get; set; } = "https://tomato-onnx-backend.onrender.com";
    public string ArgoPestUrl { get; set; } = "https://argo-pest-api.onrender.com";
    public int TimeoutSeconds { get; set; } = 120;
    public int MaxRetries { get; set; } = 3;
}
