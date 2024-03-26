using StegoRevealer.Utils.Common.Lib;

namespace StegoRevealer.Utils.CorrelationAnalyser;

public static class Constants
{
    private const string InputDataDirName = "Input";
    private const string InputDataImagesDirName = "Images";
    private const string OutputDirName = "Output";
    private const string OutputAnalysisDataFilename = "_data.csv";
    private const string OutputCorrelationsFilename = "_correlation.txt";

    public static string InputDataImagesDirPath = Path.Combine(Helper.GetAssemblyDir(), InputDataDirName, InputDataImagesDirName);
    public static string OutputDirPath = Path.Combine(Helper.GetAssemblyDir(), OutputDirName);
    public static string OutputAnalysisDataFilePath = Path.Combine(Helper.GetAssemblyDir(), OutputDirName, OutputAnalysisDataFilename);
    public static string OutputCorrelationFilePath = Path.Combine(Helper.GetAssemblyDir(), OutputDirName, OutputCorrelationsFilename);

    public static List<string> ImagesExtensions = new List<string>() { ".png", ".bmp" };

    public const double RenyiTestAlphaStartValue = 1.1;
    public const double RenyiTestAlphaEndValue = 4.0;
    public const double RenyiTestAlphaStep = 0.1;
}
