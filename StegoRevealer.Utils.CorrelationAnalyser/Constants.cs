using StegoRevealer.Utils.DataPreparer.Lib;

namespace StegoRevealer.Utils.CorrelationAnalyser;

public static class Constants
{
    private const string InputDataDirName = "Input";
    private const string InputDataImagesDirName = "Images";
    private const string OutputDirName = "Output";
    private const string OutputAnalysisDataFilename = "_data.csv";

    public static string InputDataImagesDirPath = Path.Combine(Helper.GetAssemblyDir(), InputDataDirName, InputDataImagesDirName);
    public static string OutputDirPath = Path.Combine(Helper.GetAssemblyDir(), OutputDirName);
    public static string OutputAnalysisDataFilePath = Path.Combine(Helper.GetAssemblyDir(), OutputDirName, OutputAnalysisDataFilename);

    public static List<string> ImagesExtensions = new List<string>() { ".png", ".bmp" };
}
