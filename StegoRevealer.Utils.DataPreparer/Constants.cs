using StegoRevealer.Utils.Common.Lib;
using StegoRevealer.Utils.DataPreparer.Entities;

namespace StegoRevealer.Utils.DataPreparer;

public static class Constants
{
    private const string InputDataDirName = "Input";
    private const string InputDataImagesDirName = "Images";
    private const string InputDataTextFilename = "DuneBook.txt";
    private const string OutputDirName = "Output";
    private const string OutputLogFilename = "_log.txt";
    private const string OutputAnalysisDataFilename = "_data.csv";
    private const string OutputTempAnalysisDataFilename = "_dataTemp.txt";

    public static string InputDataImagesDirPath = Path.Combine(Helper.GetAssemblyDir(), InputDataDirName, InputDataImagesDirName);
    public static string InputDataTextFilePath = Path.Combine(Helper.GetAssemblyDir(), InputDataDirName, InputDataTextFilename);
    public static string OutputDirPath = Path.Combine(Helper.GetAssemblyDir(), OutputDirName);
    public static string OutputLogFilePath = Path.Combine(Helper.GetAssemblyDir(), OutputDirName, OutputLogFilename);
    public static string OutputAnalysisDataFilePath = Path.Combine(Helper.GetAssemblyDir(), OutputDirName, OutputAnalysisDataFilename);
    public static string OutputTempAnalysisDataFilePath = Path.Combine(Helper.GetAssemblyDir(), OutputDirName, OutputTempAnalysisDataFilename);

    public const int NoHidingChangeAdvantage = 3;

    public const double EntropyRenyiAlpa = 1.1;

    public static List<string> ImagesExtensions = new List<string>() { ".png", ".bmp" };
    public const string InfoFilePostfix = "_Info";
    public const string InfoFileExt = ".txt";

    public static Dictionary<int, MinMaxData> Diapasones = new Dictionary<int, MinMaxData>
    {
        {  1, new MinMaxData { Min =  1, Max =  10 } },
        {  2, new MinMaxData { Min = 10, Max =  20 } },
        {  3, new MinMaxData { Min = 20, Max =  30 } },
        {  4, new MinMaxData { Min = 30, Max =  40 } },
        {  5, new MinMaxData { Min = 40, Max =  50 } },
        {  6, new MinMaxData { Min = 50, Max =  60 } },
        {  7, new MinMaxData { Min = 60, Max =  70 } },
        {  8, new MinMaxData { Min = 70, Max =  80 } },
        {  9, new MinMaxData { Min = 80, Max =  90 } },
        { 10, new MinMaxData { Min = 90, Max = 100 } },
    };
}
