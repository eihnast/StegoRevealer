using StegoRevelaer.API.Services;

namespace StegoRevelaer.API;

public static class Program
{
    public static void Main(string[] args)
    {
        var config = ApiConfigurator.Instance.ApiConfig;
        var builder = WebApplication.CreateBuilder(args);

        builder.WebHost.UseUrls(config.HttpAddress, config.HttpsAddress);

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
