using StegoRevealer.Common.ConsoleInterface.Processors;
using System.CommandLine;

namespace StegoRevealer.Common.ConsoleInterface;

public static class CommandLineParser
{
    public static async Task<int> HandleCommand(string[] args)
    {
        var rootCommand = new RootCommand("Stego Revealer");

        var saCommand = new Command("sa", "Стегоанализ");
        rootCommand.AddCommand(saCommand);

        var filenamesArgument = new Argument<string[]>(name: "filenames", description: "Пути к анализируемым изображениям", getDefaultValue: () => []) { Arity = ArgumentArity.OneOrMore };
        saCommand.AddArgument(filenamesArgument);

        var chiMethodOption = new Option<bool>(name: "--chi", description: "Выполнить стегоанализ методом оценки по критерию Хи-квадрат", getDefaultValue: () => false) { Arity = ArgumentArity.Zero };
        chiMethodOption.AddAlias("-c");
        saCommand.AddOption(chiMethodOption);
        var rsMethodOption = new Option<bool>(name: "--rs", description: "Выполнить стегоанализ методом Regular-Singular", getDefaultValue: () => false) { Arity = ArgumentArity.Zero };
        rsMethodOption.AddAlias("-r");
        saCommand.AddOption(rsMethodOption);
        var kzhaMethodOption = new Option<bool>(name: "--kzha", description: "Выполнить стегоанализ реверсивным методом анализа скрытия по Коха-Жао", getDefaultValue: () => false) { Arity = ArgumentArity.Zero };
        kzhaMethodOption.AddAlias("-k");
        saCommand.AddOption(kzhaMethodOption);
        var allMethodsOption = new Option<bool>(name: "--all", description: "Выполнить стегоанализ всеми доступными методами", getDefaultValue: () => false) { Arity = ArgumentArity.Zero, IsRequired = false };
        saCommand.AddOption(allMethodsOption);

        saCommand.SetHandler(ExecuteSaCommandAsync, filenamesArgument, chiMethodOption, rsMethodOption, kzhaMethodOption, allMethodsOption);

        try
        {
            await rootCommand.InvokeAsync(args);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("При выполнении команды возникла ошибка: " + ex.ToString());
            return 1;
        }

        return 0;
    }

    private static async Task ExecuteSaCommandAsync(string[] filenames, bool chiMethodOptionValue, bool rsMethodOptionValue, bool kzhaMethodOptionValue, bool allMethodsOptionValue)
    {
        if (!chiMethodOptionValue && !rsMethodOptionValue && !kzhaMethodOptionValue && !allMethodsOptionValue)
            allMethodsOptionValue = true;

        CommonLogger.LogInfo("Starting steganalysis");

        try
        {
            var tasks = filenames.Select(f => new SteganalysisProcessor(f, chiMethodOptionValue, rsMethodOptionValue, kzhaMethodOptionValue, allMethodsOptionValue).ExecuteAsync());
            await Task.WhenAll(tasks);
            ClearTemp();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Ошибка выполнения команды:" + ex);
            CommonLogger.LogError("Error:" + ex);
            throw; // пусть верх вернёт ненулевой код
        }

        CommonLogger.LogInfo("Ending steganalysis");
    }

    private static void ClearTemp()
    {
        TempManager.Instance.DeleteImageHandlers();
        TempManager.Instance.DeleteTempImages();
    }
}
