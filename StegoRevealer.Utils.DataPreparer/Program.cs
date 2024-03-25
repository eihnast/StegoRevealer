using StegoRevealer.Utils.DataPreparer.Entities;

namespace StegoRevealer.Utils.DataPreparer;

public static class Program
{
    private static readonly List<string> SkipPreparingFlag = new List<string>() { "--nohide", "--noprepare", "-nh", "-np" };
    private static readonly List<string> SkipAnalysisFlag = new List<string>() { "--noanalyze", "-na" };
    private static readonly List<string> WeakCalculationsPoolFlag = new List<string>() { "--weakpoool", "-wp" };
    private static readonly List<string> ManyHidingsFlag = new List<string>() { "--manyhidings", "-mh" };
    private static readonly List<string> HelpFlag = new List<string>() { "--help", "-h" };

    private static StartParams? ParseParams(string[] args)
    {
        var startParams = new StartParams();

        bool needHelp = args.Any(arg => HelpFlag.Any(flag => flag.Equals(arg, StringComparison.OrdinalIgnoreCase)));
        if (!needHelp)
        {
            var usedFlags = new List<string>();

            foreach (var arg in args)
            {
                if (SkipPreparingFlag.Contains(arg))
                {
                    if (usedFlags.Contains(nameof(SkipPreparingFlag)))
                        needHelp = true;
                    usedFlags.Add(nameof(SkipPreparingFlag));
                }
                else if (SkipAnalysisFlag.Contains(arg))
                {
                    if (usedFlags.Contains(nameof(SkipAnalysisFlag)))
                        needHelp = true;
                    usedFlags.Add(nameof(SkipAnalysisFlag));
                }
                else if (WeakCalculationsPoolFlag.Contains(arg))
                {
                    if (usedFlags.Contains(nameof(WeakCalculationsPoolFlag)))
                        needHelp = true;
                    usedFlags.Add(nameof(WeakCalculationsPoolFlag));
                }
                else if (ManyHidingsFlag.Contains(arg))
                {
                    if (usedFlags.Contains(nameof(ManyHidingsFlag)))
                        needHelp = true;
                    usedFlags.Add(nameof(ManyHidingsFlag));
                }
                else
                    needHelp = true;

                if (needHelp)
                    break;
            }
        }
        
        if (needHelp)
        {
            PrintHelp();
            return null;
        }

        startParams.SkipPreparing = args.Any(arg => SkipPreparingFlag.Any(flag => flag.Equals(arg, StringComparison.OrdinalIgnoreCase)));
        startParams.SkipAnalysis = args.Any(arg => SkipAnalysisFlag.Any(flag => flag.Equals(arg, StringComparison.OrdinalIgnoreCase)));
        startParams.UseWeakPoolForCalculations = args.Any(arg => WeakCalculationsPoolFlag.Any(flag => flag.Equals(arg, StringComparison.OrdinalIgnoreCase)));
        startParams.ManyHidings = args.Any(arg => ManyHidingsFlag.Any(flag => flag.Equals(arg, StringComparison.OrdinalIgnoreCase)));

        return startParams;
    }

    private static void PrintHelp() =>
        Console.WriteLine("Флаги:\n\tПропустить подготовку и скрытие: -nh\n\tПропустить анализ и сбор данных: -na" +
            "\n\tНестрогий пул вычислительных задач: -sp\n\tСкрытие одно ко многим: -mh");

    public static async Task Main(string[] args)
    {
        var startParams = ParseParams(args);
        if (startParams is null)
            return;

        var preparer = new DataPreparer(startParams);
        await preparer.Execute();
    }
}
