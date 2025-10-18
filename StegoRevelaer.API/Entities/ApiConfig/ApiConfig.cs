namespace StegoRevelaer.API.Entities.ApiConfig;

public class ApiConfig
{
    public string HttpAddress { get; set; } = "http://localhost:11038";
    public bool EnableHttps { get; set; } = false;
    public string HttpsAddress { get; set; } = "https://localhost:11040";
    public bool HttpsRedirection { get; set; } = false;
}
