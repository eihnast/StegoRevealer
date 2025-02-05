using StegoRevealer.Common;
using StegoRevealer.Common.ConsoleInterface;

namespace StegoRevealer.CLI;

public class Program
{
    public static void Main(string[] args)
    {
        Logger.LogInfo($"Started with command line args: {string.Join(", ", args)}");
        CommandLineParser.HandleCommand(args).Wait();
    }
}
