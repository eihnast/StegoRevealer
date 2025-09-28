using System.Text.Json;
using StegoRevealer.Common;
using StegoRevelaer.API.Entities.ApiConfig;

namespace StegoRevelaer.API.Services;

public class ApiConfigurator : IDisposable
{
    // Описание синглтона
    private static ApiConfigurator? _instance;
    private static readonly object _lock = new object();
    public static ApiConfigurator Instance
    {
        get
        {
            if (_instance is null)
            {
                lock (_lock)
                {
                    if (_instance is null)
                        _instance = new ApiConfigurator();
                }
            }
            return _instance;
        }
    }

    public static ApiConfig Settings { get => Instance.ApiConfig; }
    public static void SaveConfig() => Instance.SaveConfigToFile();


    private bool _isDisposed = false;
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        if (disposing)
        {
            SaveConfigToFile();
        }

        _isDisposed = true;
    }
    ~ApiConfigurator() => Dispose(false);


    private const string SettingsFileName = "StegoRevealerApiSettings.json";
    private readonly string SettingsPath = SettingsFileName;

    public ApiConfig ApiConfig { get; private set; } = null!;

    private ApiConfigurator()
    {
        try
        {
            string tempDir = Tools.GetOrCreateTempDirPath();
            SettingsPath = Path.Combine(tempDir, SettingsFileName);

            if (File.Exists(SettingsPath))
            {
                var loadedApiConfig = LoadConfigFromFile();
                if (loadedApiConfig is not null)
                    ApiConfig = loadedApiConfig;
                else
                    CreateAndSaveNewApiConfig();
            }
            else
            {
                CreateAndSaveNewApiConfig();
            }
        }
        catch (Exception ex)
        {
            ApiLogger.LogError($"API Configuration initializeing failed due to an error:\n" + ex.Message);
            ApiConfig = new ApiConfig();
            ApiLogger.LogInfo($"Created default ApiConfig with no saving operation");
        }
    }

    private void CreateAndSaveNewApiConfig()
    {
        ApiConfig = new ApiConfig();
        SaveConfigToFile();
    }

    private ApiConfig? LoadConfigFromFile()
    {
        try
        {
            string configJson = File.ReadAllText(SettingsPath);
            var appConfig = JsonSerializer.Deserialize<ApiConfig>(configJson, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = null
            });

            return appConfig;
        }
        catch (Exception ex)
        {
            ApiLogger.LogError($"Loading ApiConfig from '{SettingsPath}' failed due to an error:\n" + ex.Message);
        }

        return null;
    }

    private void SaveConfigToFile()
    {
        try
        {
            string configJson = JsonSerializer.Serialize(ApiConfig, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = null
            });
            File.WriteAllText(SettingsPath, configJson);
        }
        catch (Exception ex)
        {
            ApiLogger.LogError($"Saving actual ApiConfig to '{SettingsPath}' failed due to an error:\n" + ex.Message);
        }
    }
}
