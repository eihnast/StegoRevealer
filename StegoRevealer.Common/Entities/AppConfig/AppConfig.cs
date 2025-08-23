namespace StegoRevealer.Common.Entities.AppConfig;

public class AppConfig
{
    public bool IsLoggingEnabled { get; set; } = true;
    public string Language { get; set; } = Constants.Languages.First().Key;
}
