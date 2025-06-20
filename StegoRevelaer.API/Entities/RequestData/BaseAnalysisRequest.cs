namespace StegoRevelaer.API.Entities.RequestData;

public abstract class BaseAnalysisRequest
{
    public string? ImageUrl { get; set; }
    public string? ImageData { get; set; }
}
