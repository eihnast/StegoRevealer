using StegoRevealer.Common;
using StegoRevelaer.API.Services;

namespace StegoRevelaer.API;

public static class Program
{
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

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        Logger.LogInfo($"StegoRevelaer API started at {DateTime.Now}");
        app.Run();
    }
}
