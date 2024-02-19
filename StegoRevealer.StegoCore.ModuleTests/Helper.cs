namespace StegoRevealer.StegoCore.ModuleTests;

public static class Helper
{
    public static string GetAssemblyDir() =>
        Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location) ?? string.Empty;
}
