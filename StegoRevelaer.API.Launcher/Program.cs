using System.Diagnostics;

namespace StegoRevelaer.API.Launcher;

public class Program
{
    public static void Main(string[] args)
    {
        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
        new ApiHost().StartSync();
    }
}
