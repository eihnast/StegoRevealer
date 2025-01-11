using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.ComplexAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.UI.Tools;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;
using StegoRevealer.UI.Lib;

namespace StegoRevealer.UI.Desktop.ConsoleInterface.CommandHandlers;

public class SteganalysisProcessor
{
    private static ImageHandler? _currentImage;
    private LoggerHandler _logger;

    private string? _fileName;
    private bool _useChiSqr;
    private bool _userRs;
    private bool _useKzha;
    private bool _useAllMethods;

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
        var complexAnalysisParams = new ComplexAnalysisParameters();

        if (_currentImage is null)
            return;

        // Создание задач
        if (_useChiSqr || _useAllMethods)  // Хи-квадрат
            complexAnalysisParams.ChiSquareParameters = new ChiSquareParameters(_currentImage);
        if (_userRs || _useAllMethods)  // RS
            complexAnalysisParams.RsParameters = new RsParameters(_currentImage);
        if (_useKzha || _useAllMethods)  // KZHA
            complexAnalysisParams.KzhaParameters = new KzhaParameters(_currentImage);
        complexAnalysisParams.Image = _currentImage;

        _logger.LogInfo("Starting steganalysis operations");

        var result = await ComplexAnalysisStarter.Start(complexAnalysisParams);

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
            + "\nChiSquare = " + CommonTools.GetFormattedJson(chiRes)
            + "\nLogs of ChiSquare method = \n" + chiRes?.ToString(indent: 1)
            + "\n\nRegular-Singular = " + CommonTools.GetFormattedJson(rsRes)
            + "\nLogs of Regular-Singular method = \n" + rsRes?.ToString(indent: 1)
            + "\n\nKoch-Zhao Analysis = " + CommonTools.GetFormattedJson(kzhaRes)
            + "\nLogs of Koch-Zhao Analysis method = \n" + kzhaRes?.ToString(indent: 1)
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
            Console.Error.WriteLine($"Не удалось загрузить файл изображения '{fullPath}'");
            return false;
        }

        return true;
    }

    private void PrintResults(ComplexAnalysisResults result, string imgName)
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
            (result.IsHidingDetected is null
            ? Constants.ResultsDefaults.IsHidingDecisionCannotBeCalculated
            : (result.IsHidingDetected.Value ? Constants.ResultsDefaults.Deceted.ToUpper() : Constants.ResultsDefaults.NotDetected.ToUpper())));
        //outputStr.AppendLine();

        outputStr.AppendLine(CommonTools.AddColon(Constants.ResultsNames.ChiSqrValue) + CommonTools.GetValueAsPercents(chiRes?.MessageRelativeVolume ?? 0.0));
        outputStr.AppendLine(CommonTools.AddColon(Constants.ResultsNames.RsValue + CommonTools.GetValueAsPercents(rsRes?.MessageRelativeVolume ?? 0.0)));
        outputStr.AppendLine(CommonTools.AddColon(Constants.ResultsNames.KzhaDetection) +
            (!kzhaRes?.SuspiciousIntervalIsFound ?? false ? Constants.ResultsDefaults.No : Constants.ResultsDefaults.Yes));
        if (kzhaRes is not null && kzhaRes.SuspiciousIntervalIsFound)
        {
            outputStr.AppendLine("\t" + CommonTools.AddColon(Constants.ResultsNames.KzhaBitsNum) + kzhaRes.MessageBitsVolume);
            outputStr.AppendLine("\t" + CommonTools.AddColon(Constants.ResultsNames.KzhaThreshold) + CommonTools.GetFormattedDouble(kzhaRes.Threshold));

            // Если порог или предполагаемое количество бит равно 0, то остальные данные явно неактуальны
            bool kzhaHasRealData = kzhaRes.MessageBitsVolume > 0.0 && kzhaRes.Threshold > 0.0;

            if (kzhaHasRealData)
            {
                outputStr.AppendLine("\t" + CommonTools.AddColon(Constants.ResultsNames.KzhaIndexes) + kzhaRes.Coefficients.ToString());
                outputStr.AppendLine("\t" + CommonTools.AddColon(Constants.ResultsNames.KzhaCoeffs) +
                    $"[{kzhaRes.SuspiciousInterval?.leftInd}, {kzhaRes.SuspiciousInterval?.rightInd}]");
            }

            if (kzhaRes.ExtractedData is not null)
            {
                outputStr.AppendLine("\t" + CommonTools.AddColon(Constants.ResultsNames.KzhaExtractedInfo, false));
                outputStr.AppendLine(kzhaRes.ExtractedData);
            }
        }

        //outputStr.AppendLine();
        outputStr.AppendLine(CommonTools.AddColon(Constants.ResultsNames.StatmLabel, false));
        outputStr.AppendLine("\t" + CommonTools.AddColon(Constants.ResultsNames.StatmNoise) + CommonTools.GetLongFormattedDouble(statmRes?.NoiseValue));
        outputStr.AppendLine("\t" + CommonTools.AddColon(Constants.ResultsNames.StatmSharpness) + CommonTools.GetLongFormattedDouble(statmRes?.SharpnessValue));
        outputStr.AppendLine("\t" + CommonTools.AddColon(Constants.ResultsNames.StatmBlur) + CommonTools.GetLongFormattedDouble(statmRes?.BlurValue));
        outputStr.AppendLine("\t" + CommonTools.AddColon(Constants.ResultsNames.StatmContrast) + CommonTools.GetLongFormattedDouble(statmRes?.ContrastValue));
        outputStr.AppendLine("\t" + CommonTools.AddColon(Constants.ResultsNames.StatmShennon) + CommonTools.GetLongFormattedDouble(statmRes?.EntropyValues.Shennon));
        outputStr.AppendLine("\t" + CommonTools.AddColon(Constants.ResultsNames.StatmRenyi) + CommonTools.GetLongFormattedDouble(statmRes?.EntropyValues.Renyi));

        //outputStr.AppendLine();
        outputStr.AppendLine(CommonTools.AddColon(Constants.ResultsNames.ElapsedTime) + CommonTools.GetElapsedTime(result.ElapsedTime));

        Console.WriteLine(outputStr.ToString());
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
            Console.Error.WriteLine($"Возникла ошибка при загрузке файла изображения '{path}'\n" + ex.Message);
        }

        return null;
    }

    /// <summary>Осуществляет загрузку изображения</summary>
    private ImageHandler? LoadImage(string path)
    {
        // Загрузка
        var tempPath = CommonTools.CopyFileToTemp(path);

        if (!string.IsNullOrEmpty(tempPath))
        {
            TempManager.Instance.RememberTempImage(tempPath);
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
