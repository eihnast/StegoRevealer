using StegoRevealer.Common;
using StegoRevelaer.API.Services;

namespace StegoRevelaer.API;

public static class Program
{
    private static bool ClosingOperationsExecuted = false;

    public static void Main(string[] args)
    {
        Logger.FileSuffix = "API";
        Logger.LogInfo("Starting StegoRevelaer API...");

        var config = ApiConfigurator.Instance.ApiConfig;
        var builder = WebApplication.CreateBuilder(args);

        builder.WebHost.UseUrls(config.HttpAddress, config.HttpsAddress);

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        var app = builder.Build();
        Logger.LogInfo("Building StegoRevelaer API Host...");

        RegisterClosingOperations(app.Lifetime);

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        if (config.HttpsRedirection)
            app.UseHttpsRedirection();

        app.UseAuthorization();
        app.MapControllers();

        Logger.LogInfo($"StegoRevelaer API started at {DateTime.Now}.");
        app.Run();
    }

    private static void ExecuteClosingOperations()
    {
        if (!ClosingOperationsExecuted)
        {
            Logger.LogInfo("StegoRevelaer API is stopping...");
            ApiConfigurator.SaveConfig();
            TempManager.Instance.DeleteImageHandlers();
            TempManager.Instance.DeleteTempImages();
            Logger.LogInfo("StegoRevelaer API stopped successfully.");

            ClosingOperationsExecuted = true;
        }
    }

    private static void RegisterClosingOperations(this IHostApplicationLifetime lifetime)
    {
        lifetime.ApplicationStopping.Register(ExecuteClosingOperations);
        AppDomain.CurrentDomain.ProcessExit += (s, e) => ExecuteClosingOperations();
        AppDomain.CurrentDomain.UnhandledException += (s, e) => ExecuteClosingOperations();
        TaskScheduler.UnobservedTaskException += (s, e) => ExecuteClosingOperations();
    }
}
