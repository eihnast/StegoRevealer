using StegoRevealer.Utils.Common.Entities;
using StegoRevealer.Utils.Common.Lib;
using StegoRevealer.Utils.DataPreparer.Entities;

namespace StegoRevealer.Utils.DataPreparer;

public static class Program
{
    private static readonly List<string> SkipPreparingFlag = new List<string>() { "--nohide", "--noprepare", "-nh", "-np" };
    private static readonly List<string> SkipAnalysisFlag = new List<string>() { "--noanalyze", "-na" };
    private static readonly List<string> WeakCalculationsPoolFlag = new List<string>() { "--weakpoool", "-wp" };
    private static readonly List<string> ManyHidingsFlag = new List<string>() { "--manyhidings", "-mh" };
    private static readonly List<string> BasketOperationsFlag = new List<string>() { "--baskets", "-b" };
    private static readonly List<string> HelpFlag = new List<string>() { "--help", "-h" };

    private static readonly Dictionary<string, InputParameter> InputParameters = new Dictionary<string, InputParameter>()
    {
        { "SkipPreparingFlag", new InputParameter { AvailableNames = new List<string> { "--nohide", "-nh" }, HasValue = false } },
        { "SkipAnalysisFlag", new InputParameter { AvailableNames = new List<string> { "--noanalyze", "-na" }, HasValue = false } },
        { "WeakCalculationsPoolFlag", new InputParameter { AvailableNames = new List<string> { "--weakpoool", "-wp" }, HasValue = false } },
        { "ManyHidingsFlag", new InputParameter { AvailableNames = new List<string> { "--manyhidings", "-mh" }, HasValue = false } },
        { "BasketOperationsFlag", new InputParameter { AvailableNames = new List<string> { "--baskets", "-b" }, HasValue = false } },
        { "HelpFlag", new InputParameter { AvailableNames = new List<string> { "--help", "-h" }, HasValue = false } }
    };

    private static StartParams? ParseParams(string[] args)
    {
        var startParams = new StartParams();
        bool needHelp = !StartParametersHelper.IsParametersValid(args, InputParameters.Values.ToArray()) || StartParametersHelper.IsParameterSpecified(args, InputParameters["HelpFlag"]);

        if (needHelp)
        {
            PrintHelp();
            return null;
        }

        startParams.SkipPreparing = StartParametersHelper.IsParameterSpecified(args, InputParameters["SkipPreparingFlag"]);
        startParams.SkipAnalysis = StartParametersHelper.IsParameterSpecified(args, InputParameters["SkipAnalysisFlag"]);
        startParams.UseWeakPoolForCalculations = StartParametersHelper.IsParameterSpecified(args, InputParameters["WeakCalculationsPoolFlag"]);
        startParams.ManyHidings = StartParametersHelper.IsParameterSpecified(args, InputParameters["ManyHidingsFlag"]);
        startParams.BasketOperations = StartParametersHelper.IsParameterSpecified(args, InputParameters["BasketOperationsFlag"]);

        return startParams;
    }

    private static void PrintHelp() =>
        Console.WriteLine("Флаги:\n\tПропустить подготовку и скрытие: -nh\n\tПропустить анализ и сбор данных: -na" +
            "\n\tНестрогий пул вычислительных задач: -wp\n\tСкрытие одно ко многим: -mh");

    public static async Task Main(string[] args)
    {
        var startParams = ParseParams(args);
        if (startParams is null)
            return;

        var preparer = new DataPreparer(startParams);
        await preparer.Execute();
    }
}
