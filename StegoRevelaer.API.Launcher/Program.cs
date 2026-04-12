using System.Diagnostics;

namespace StegoRevelaer.API.Launcher;

public class Program
{
    public static void Main(string[] args)
    {
        bool isWindows = Environment.OSVersion.Platform is PlatformID.Win32NT;
        if (isWindows)
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
        new ApiHost().StartSync();
    }
}
