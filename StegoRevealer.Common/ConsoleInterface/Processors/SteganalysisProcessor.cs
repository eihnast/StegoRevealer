using StegoRevealer.Common.ConsoleInterface.Tools;
using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.ComplexAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.CommonLib.Entities;
using StegoRevealer.StegoCore.ImageHandlerLib;
using System.Text;

namespace StegoRevealer.Common.ConsoleInterface.Processors;

public class SteganalysisProcessor
{
    private ImageHandler? _currentImage;
    private readonly LoggerHandler _logger;

    private readonly string? _fileName;
    private readonly bool _useChiSqr;
    private readonly bool _userRs;
    private readonly bool _useKzha;
    private readonly bool _useAllMethods;

    public SteganalysisProcessor(string? filename, bool chiMethodOptionValue, bool rsMethodOptionValue, bool kzhaMethodOptionValue, bool allMethodsOptionValue)
    {
        _fileName = filename;
        _useChiSqr = chiMethodOptionValue;
        _userRs = rsMethodOptionValue;
        _useKzha = kzhaMethodOptionValue;
        _useAllMethods = allMethodsOptionValue;

        _logger = new LoggerHandler();
    }

    public async Task ExecuteAsync()
    {
        var imageLoaded = UpdateCurrentImage(_fileName);
        if (!imageLoaded)
            return;

        _logger.LogInfo($"Starting steganalysis for file '{_fileName}'");
        var jointAnalysisParams = new JointAnalysisMethodsParameters();

        if (_currentImage is null)
            return;

        // Создание задач
        if (_useChiSqr || _useAllMethods)  // Хи-квадрат
            jointAnalysisParams.ChiSquareParameters = new ChiSquareParameters(_currentImage);
        if (_userRs || _useAllMethods)  // RS
            jointAnalysisParams.RsParameters = new RsParameters(_currentImage);
        if (_useKzha || _useAllMethods)  // KZHA
            jointAnalysisParams.KzhaParameters = new KzhaParameters(_currentImage);
        jointAnalysisParams.StatmParameters = new StatmParameters(_currentImage);
        jointAnalysisParams.ComplexSaMethodParameters = new ComplexSaMethodParameters(_currentImage);

        _logger.LogInfo("Starting steganalysis operations");

        var result = await JointAnalysisStarter.Start(jointAnalysisParams);

        _logger.LogInfo("Steganalysis operations completed");

        if (result is null)
        {
            _logger.LogWarning("Steganalysis result is null");
            return;
        }

        // Приведение к известным типам результатов
        var chiRes = result.ChiSquareResult;
        var rsRes = result.RsResult;
        var kzhaRes = result.KzhaResult;
        var statmRes = result.StatmResult;
        
        // Запись в лог результатов
        _logger.LogInfo("Received steganalysis results are:\n" + Logger.Separator
            + "\nChiSquare = " + Common.Tools.GetFormattedJson(chiRes)
            + "\nLogs of ChiSquare method = \n" + chiRes?.ToString(indent: 1)
            + "\n\nRegular-Singular = " + Common.Tools.GetFormattedJson(rsRes)
            + "\nLogs of Regular-Singular method = \n" + rsRes?.ToString(indent: 1)
            + "\n\nKoch-Zhao Analysis = " + Common.Tools.GetFormattedJson(kzhaRes)
            + "\nLogs of Koch-Zhao Analysis method = \n" + kzhaRes?.ToString(indent: 1)
            + "\n\nStatistical metrics = " + Common.Tools.GetFormattedJson(statmRes)
            + "\nLogs statistical metrics calculation = \n" + statmRes?.ToString(indent: 1)
            + $"\n\nElapsed time = {result.ElapsedTime}\n" + Logger.Separator);

        PrintResults(result, Path.GetFileNameWithoutExtension(_fileName ?? string.Empty));
        _logger.LogInfo($"Ended steganalysis for file '{_fileName}'");

        _logger.Flush();
        CloseImageHandler();
    }

    private bool UpdateCurrentImage(string? newFilename)
    {
        if (_currentImage is not null)
            TempManager.Instance.ForgetHandler(_currentImage);
        _currentImage?.CloseHandler();
        _currentImage = null;

        if (string.IsNullOrEmpty(newFilename))
            return false;

        var fullPath = Path.GetFullPath(newFilename);
        _logger.LogInfo($"Loading image '{newFilename}' with full path '{fullPath}'");

        try
        {
            _currentImage = LoadImage(fullPath);
        }
        catch
        {
            return false;
        }

        if (_currentImage is null)
        {
            _logger.LogError($"Unsuccess while loading image '{fullPath}'");
            WinConsole.WriteLine($"Не удалось загрузить файл изображения '{fullPath}'");
            return false;
        }

        return true;
    }

    private static void PrintResults(JointAnalysisResult result, string imgName)
    {
        // Приведение к известным типам результатов
        var chiRes = result.ChiSquareResult;
        var rsRes = result.RsResult;
        var kzhaRes = result.KzhaResult;
        var statmRes = result.StatmResult;

        // Вывод результатов стегоанализа
        var outputStr = new StringBuilder();
        outputStr.AppendLine($"Результаты стегоанализа изображения '{imgName}'");

        outputStr.AppendLine(Constants.ResultsNames.HidingDesicionDetection + " " +
            (result.ComplexSaMethodResults is null
            ? Constants.ResultsDefaults.IsHidingDecisionCannotBeCalculated
            : (result.ComplexSaMethodResults.IsHidingDetected ? Constants.ResultsDefaults.Deceted.ToUpper() : Constants.ResultsDefaults.NotDetected.ToUpper())));

        outputStr.AppendLine(Common.Tools.AddColon(Constants.ResultsNames.ChiSqrValue) + Common.Tools.GetValueAsPercents(chiRes?.MessageRelativeVolume ?? 0.0));
        outputStr.AppendLine(Common.Tools.AddColon(Constants.ResultsNames.RsValue) + Common.Tools.GetValueAsPercents(rsRes?.MessageRelativeVolume ?? 0.0));
        outputStr.AppendLine(Common.Tools.AddColon(Constants.ResultsNames.KzhaDetection) +
            (!kzhaRes?.SuspiciousIntervalIsFound ?? false ? Constants.ResultsDefaults.No : Constants.ResultsDefaults.Yes));
        if (kzhaRes is not null && kzhaRes.SuspiciousIntervalIsFound)
        {
            outputStr.AppendLine("\t" + Common.Tools.AddColon(Constants.ResultsNames.KzhaBitsNum) + kzhaRes.MessageBitsVolume);
            outputStr.AppendLine("\t" + Common.Tools.AddColon(Constants.ResultsNames.KzhaThreshold) + Common.Tools.GetFormattedDouble(kzhaRes.Threshold));

            // Если порог или предполагаемое количество бит равно 0, то остальные данные явно неактуальны
            bool kzhaHasRealData = kzhaRes.MessageBitsVolume > 0.0 && kzhaRes.Threshold > 0.0;

            if (kzhaHasRealData)
            {
                outputStr.AppendLine("\t" + Common.Tools.AddColon(Constants.ResultsNames.KzhaIndexes) + kzhaRes.Coefficients.ToString());
                outputStr.AppendLine("\t" + Common.Tools.AddColon(Constants.ResultsNames.KzhaCoeffs) +
                    $"[{kzhaRes.SuspiciousInterval?.leftInd}, {kzhaRes.SuspiciousInterval?.rightInd}]");
            }

            if (kzhaRes.ExtractedData is not null)
            {
                outputStr.AppendLine("\t" + Common.Tools.AddColon(Constants.ResultsNames.KzhaExtractedInfo, false));
                outputStr.AppendLine(kzhaRes.ExtractedData);
            }
        }

        outputStr.AppendLine(Common.Tools.AddColon(Constants.ResultsNames.StatmLabel, false));
        outputStr.AppendLine("\t" + Common.Tools.AddColon(Constants.ResultsNames.StatmNoise) + Common.Tools.GetLongFormattedDouble(statmRes?.NoiseValue));
        outputStr.AppendLine("\t" + Common.Tools.AddColon(Constants.ResultsNames.StatmSharpness) + Common.Tools.GetLongFormattedDouble(statmRes?.SharpnessValue));
        outputStr.AppendLine("\t" + Common.Tools.AddColon(Constants.ResultsNames.StatmBlur) + Common.Tools.GetLongFormattedDouble(statmRes?.BlurValue));
        outputStr.AppendLine("\t" + Common.Tools.AddColon(Constants.ResultsNames.StatmContrast) + Common.Tools.GetLongFormattedDouble(statmRes?.ContrastValue));
        outputStr.AppendLine("\t" + Common.Tools.AddColon(Constants.ResultsNames.StatmShennon) + Common.Tools.GetLongFormattedDouble(statmRes?.EntropyValues.Shennon));
        outputStr.AppendLine("\t" + Common.Tools.AddColon(Constants.ResultsNames.StatmRenyi) + Common.Tools.GetLongFormattedDouble(statmRes?.EntropyValues.Renyi));

        outputStr.AppendLine(Common.Tools.AddColon(Constants.ResultsNames.ElapsedTime) + Common.Tools.GetElapsedTime(result.ElapsedTime));

        WinConsole.WriteLine(outputStr.ToString());
    }

    /// <summary>Создаёт обработчик изображения</summary>
    private ImageHandler? CreateImageHandler(string path)
    {
        try
        {
            var imageHandler = new ImageHandler(path);
            TempManager.Instance.RememberHandler(imageHandler);
            _logger.LogInfo($"Loaded new image for steganalysis: {imageHandler.ImgPath}");

            return imageHandler;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error while loading image '{path}':\n" + ex.Message);
            WinConsole.WriteLine($"Возникла ошибка при загрузке файла изображения '{path}'\n" + ex.Message);
        }

        return null;
    }

    /// <summary>Осуществляет загрузку изображения</summary>
    private ImageHandler? LoadImage(string path)
    {
        // Загрузка
        var tempPath = Common.Tools.CopyFileToTemp(path);

        if (!string.IsNullOrEmpty(tempPath))
        {
            TempManager.Instance.RememberTempImage(path, tempPath);
            return CreateImageHandler(tempPath);
        }

        return null;
    }

    private void CloseImageHandler()
    {
        _currentImage?.CloseHandler();
        if (_currentImage is not null)
            TempManager.Instance.ForgetHandler(_currentImage);
    }
}

