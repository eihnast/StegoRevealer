using StegoRevealer.UI.Lib.Entities.AppConfig;
using System.IO;
using System.Text.Json;
using System;
using System.Reflection.Metadata;

namespace StegoRevealer.UI.Tools;

public class Configurator
{
    // Описание синглтона
    private static Configurator? _instance;
    private static object _lock = new object();
    public static Configurator Instance
    {
        get
        {
            if (_instance is null)
            {
                lock (_lock)
                {
                    if (_instance is null)
                        _instance = new Configurator();
                }
            }
            return _instance;
        }
    }

    public static AppConfig Settings { get => Instance.AppConfig; }
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
    ~Configurator() => Dispose(false);


    private const string SettingsFileName = "StegoRevealerSettings.json";
    private string SettingsPath = SettingsFileName;

    public AppConfig AppConfig { get; private set; } = null!;

    private Configurator()
    {
        try
        {
            string tempDir = CommonTools.GetOrCreateTempDirPath();
            SettingsPath = Path.Combine(tempDir, SettingsFileName);

            if (File.Exists(SettingsPath))
            {
                var loadedAppConfig = LoadConfigFromFile();
                if (loadedAppConfig is not null)
                    AppConfig = loadedAppConfig;
                else
                    CreateAndSaveNewAppConfig();
            }
            else
            {
                CreateAndSaveNewAppConfig();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Configuration initializeing failed due to an error:\n" + ex.Message);
            AppConfig = new AppConfig();
            Logger.LogInfo($"Created default AppConfig with no saving operation");
        }
    }

    private void CreateAndSaveNewAppConfig()
    {
        AppConfig = new AppConfig();
        SaveConfigToFile();
    }

    private AppConfig? LoadConfigFromFile()
    {
        try
        {
            string configJson = File.ReadAllText(SettingsPath);
            var appConfig = JsonSerializer.Deserialize<AppConfig>(configJson, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = null
            });

            return appConfig;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Loading AppConfig from '{SettingsPath}' failed due to an error:\n" + ex.Message);
        }

        return null;
    }

    private void SaveConfigToFile()
    {
        try
        {
            string configJson = JsonSerializer.Serialize(AppConfig, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = null
            });
            File.WriteAllText(SettingsPath, configJson);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Saving actual AppConfig to '{SettingsPath}' failed due to an error:\n" + ex.Message);
        }
    }
}
