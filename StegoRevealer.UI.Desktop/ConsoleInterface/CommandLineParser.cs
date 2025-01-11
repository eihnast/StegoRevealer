using StegoRevealer.UI.Desktop.ConsoleInterface.CommandHandlers;
using StegoRevealer.UI.Tools;
using System.Collections.Generic;
using System.CommandLine;
using System.Threading.Tasks;

namespace StegoRevealer.UI.Desktop.ConsoleInterface;

public static class CommandLineParser
{
    public static async Task HandleCommand(string[] args)
    {
        var rootCommand = new RootCommand("Stego Revealer");

        var saCommand = new Command("sa", "Стегоанализ");
        rootCommand.AddCommand(saCommand);

        var filenamesArgument = new Argument<string[]>(name: "filenames", description: "Анализируемое изображение", getDefaultValue: () => []);
        saCommand.AddArgument(filenamesArgument);

        var chiMethodOption = new Option<bool>(name: "--chi", description: "Выполнить стегоанализ методом оценки по критерию Хи-квадрат", getDefaultValue: () => true);
        chiMethodOption.AddAlias("-c");
        saCommand.AddOption(chiMethodOption);
        var rsMethodOption = new Option<bool>(name: "--rs", description: "Выполнить стегоанализ методом Regular-Singular", getDefaultValue: () => true);
        rsMethodOption.AddAlias("-r");
        saCommand.AddOption(rsMethodOption);
        var kzhaMethodOption = new Option<bool>(name: "--kzha", description: "Выполнить стегоанализ реверсивным методом анализа скрытия по Коха-Жао", getDefaultValue: () => true);
        kzhaMethodOption.AddAlias("-k");
        saCommand.AddOption(kzhaMethodOption);
        var allMethodsOption = new Option<bool>(name: "--all", description: "Выполнить стегоанализ всеми доступными методами", getDefaultValue: () => true);
        saCommand.AddOption(allMethodsOption);

        saCommand.SetHandler(ExecuteSaCommandAsync, filenamesArgument, chiMethodOption, rsMethodOption, kzhaMethodOption, allMethodsOption);

        await rootCommand.InvokeAsync(args);
    }

    private static async Task ExecuteSaCommandAsync(string[] filenames, bool chiMethodOptionValue, bool rsMethodOptionValue, bool kzhaMethodOptionValue, bool allMethodsOptionValue)
    {
        Logger.LogInfo("Starting steganalysis");

        var tasks = new List<Task>();
        foreach (var filename in filenames)
            tasks.Add(Task.Run(() => new SteganalysisProcessor(filename, chiMethodOptionValue, rsMethodOptionValue, kzhaMethodOptionValue, allMethodsOptionValue).ExecuteAsync()));
        foreach (var task in tasks)
            await task;

        ClearTemp();
        Logger.LogInfo("Ending steganalysis");
    }

    private static void ClearTemp()
    {
        TempManager.Instance.DeleteImageHandlers();
        TempManager.Instance.DeleteTempImages();
    }
}
