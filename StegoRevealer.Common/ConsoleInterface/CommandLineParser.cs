using StegoRevealer.Common.ConsoleInterface.Processors;
using System.CommandLine;

namespace StegoRevealer.Common.ConsoleInterface;

public static class CommandLineParser
{
    public static async Task<int> HandleCommand(string[] args)
    {
        var rootCommand = new RootCommand("Stego Revealer");

        var saCommand = new Command("sa", "Стегоанализ");
        rootCommand.Subcommands.Add(saCommand);

        var filenamesArgument = new Argument<string[]>(name: "filenames") { Description = "Пути к анализируемым изображениям", DefaultValueFactory = res => [], Arity = ArgumentArity.OneOrMore };
        saCommand.Arguments.Add(filenamesArgument);

        var chiMethodOption = new Option<bool>(name: "--chi") { Description = "Выполнить стегоанализ методом оценки по критерию Хи-квадрат", DefaultValueFactory = res => false, Arity = ArgumentArity.Zero };
        chiMethodOption.Aliases.Add("-c");
        saCommand.Options.Add(chiMethodOption);
        var rsMethodOption = new Option<bool>(name: "--rs") { Description = "Выполнить стегоанализ методом Regular-Singular", DefaultValueFactory = res => false, Arity = ArgumentArity.Zero };
        rsMethodOption.Aliases.Add("-r");
        saCommand.Options.Add(rsMethodOption);
        var kzhaMethodOption = new Option<bool>(name: "--kzha") { Description = "Выполнить стегоанализ реверсивным методом анализа скрытия по Коха-Жао", DefaultValueFactory = res => false, Arity = ArgumentArity.Zero };
        kzhaMethodOption.Aliases.Add("-k");
        saCommand.Options.Add(kzhaMethodOption);
        var allMethodsOption = new Option<bool>(name: "--all") { Description = "Выполнить стегоанализ всеми доступными методами", DefaultValueFactory = res => false, Arity = ArgumentArity.Zero, Required = false };
        saCommand.Options.Add(allMethodsOption);

        saCommand.SetAction(async parseResult => await ExecuteSaCommandAsync(
            parseResult.GetValue(filenamesArgument) ?? [],
            parseResult.GetValue(chiMethodOption),
            parseResult.GetValue(rsMethodOption),
            parseResult.GetValue(kzhaMethodOption),
            parseResult.GetValue(allMethodsOption)));

        try
        {
            var parseResult = rootCommand.Parse(args);
            await parseResult.InvokeAsync();
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
