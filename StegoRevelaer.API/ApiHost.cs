using Microsoft.Extensions.Logging;
using StegoRevealer.Common;
using StegoRevelaer.API.Services;

namespace StegoRevelaer.API;

public class ApiHost
{
    private CancellationTokenSource? _apiCts;

    private Action<string>? LogsPush;

    public ApiHost(Action<string>? logsPush = null)
    {
        LogsPush = logsPush;

        ApiLogger.LogInfo("Starting StegoRevelaer API...");

        var config = ApiConfigurator.Instance.ApiConfig;
        var builder = WebApplication.CreateBuilder();

        // Логирование
        if (LogsPush is not null)
        {
            builder.Logging.ClearProviders();
            builder.Logging.AddProvider(new SrLoggerProvider(LogsPush));
        }

        builder.WebHost.UseUrls(config.HttpAddress, config.HttpsAddress);

        builder.Services
            .AddControllers()
            .AddApplicationPart(typeof(ApiHost).Assembly)
            .AddControllersAsServices();
        builder.Services.AddOpenApi();

        var app = builder.Build();
        ApiLogger.LogInfo("Building StegoRevelaer API Host...");

        RegisterClosingOperations(app.Lifetime);

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        if (config.HttpsRedirection)
            app.UseHttpsRedirection();

        app.UseAuthorization();
        app.MapControllers();

        App = app;
    }

    private bool ClosingOperationsExecuted = false;

    private WebApplication? App;

    public CancellationTokenSource Start()
    {
        _apiCts = new CancellationTokenSource();

        ApiLogger.LogInfo($"StegoRevelaer API started at {DateTime.Now}.");
        if (App is not null)
            App.RunAsync(_apiCts.Token);

        ApiLogger.LogInfo($"StegoRevelaer API started on {string.Join("; ", App?.Urls ?? [])}.");

        return _apiCts;
    }

    public void StartSync()
    {
        ApiLogger.LogInfo($"StegoRevelaer API started at {DateTime.Now}.");
        if (App is not null)
            App.Run();
    }

    public async Task Stop()
    {
        _apiCts?.Cancel();
        ApiLogger.LogInfo($"StegoRevelaer API stopped at {DateTime.Now}.");
        ExecuteClosingOperations();
        if (App is not null)
            await App.StopAsync();
    }

    private void ExecuteClosingOperations()
    {
        if (!ClosingOperationsExecuted)
        {
            ApiLogger.LogInfo("StegoRevelaer API is stopping...");
            ApiConfigurator.SaveConfig();
            TempManager.Instance.DeleteImageHandlers(logger: ApiLogger.Instance);
            TempManager.Instance.DeleteTempImages(logger: ApiLogger.Instance);
            TempManager.Instance.DeleteTempFiles(logger: ApiLogger.Instance);
            ApiLogger.LogInfo("StegoRevelaer API stopped successfully.");

            ClosingOperationsExecuted = true;
        }
    }

    private void RegisterClosingOperations(IHostApplicationLifetime lifetime)
    {
        lifetime.ApplicationStopping.Register(ExecuteClosingOperations);
        AppDomain.CurrentDomain.ProcessExit += (s, e) => ExecuteClosingOperations();
        AppDomain.CurrentDomain.UnhandledException += (s, e) => ExecuteClosingOperations();
        TaskScheduler.UnobservedTaskException += (s, e) => ExecuteClosingOperations();
    }
}
