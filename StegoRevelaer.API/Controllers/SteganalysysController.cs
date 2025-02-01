using Microsoft.AspNetCore.Mvc;
using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.ComplexAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.DecisionModule;
using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevelaer.API.Controllers;

[ApiController]
[Route("api/sa/[action]")]
public class SteganalysysController : ControllerBase
{
    private readonly ILogger<SteganalysysController> _logger;

    public SteganalysysController(ILogger<SteganalysysController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetDecisionAsync(string path)
    {
        if (string.IsNullOrEmpty(path))
            return GetErrorResult("Передан пустой путь изображения");

        // Запуск комплексного стегоанализа
        var image = new ImageHandler(path);
        var complexMethodTasks = new List<Task>();
        ComplexSaMethodResults? complexSaMethodResults = null;
        if (image is null)
            return GetErrorResult("Не удалось создать обработчик изображения");

        complexSaMethodResults = new ComplexSaMethodResults();

        var complexChiSqrMethodHorizontalAnalyzer = new ChiSquareAnalyser(image);
        complexChiSqrMethodHorizontalAnalyzer.Params.TraverseType = TraverseType.Horizontal;

        var complexChiSqrMethodVerticalAnalyzer = new ChiSquareAnalyser(image);
        complexChiSqrMethodVerticalAnalyzer.Params.TraverseType = TraverseType.Vertical;

        var complexRsMethodAnalyzer = new RsAnalyser(image);

        var complexKzhaMethodHorizontalAnalyzer = new KzhaAnalyser(image);
        complexKzhaMethodHorizontalAnalyzer.Params.TraverseType = TraverseType.Horizontal;

        var complexKzhaMethodVerticalAnalyzer = new KzhaAnalyser(image);
        complexKzhaMethodVerticalAnalyzer.Params.TraverseType = TraverseType.Horizontal;

        var statmAnalyzer = new StatmAnalyser(image);
        statmAnalyzer.Params.EntropyCalcSensitivity = 1.1;

        complexSaMethodResults.PixelsNum = image.Width * image.Height;

        // Задачи
        complexMethodTasks.Add(Task.Run(() => { try { complexSaMethodResults.ChiSquareHorizontalResult = complexChiSqrMethodHorizontalAnalyzer.Analyse(); } catch { } }));
        complexMethodTasks.Add(Task.Run(() => { try { complexSaMethodResults.ChiSquareVerticalResult = complexChiSqrMethodVerticalAnalyzer.Analyse(); } catch { } }));
        complexMethodTasks.Add(Task.Run(() => { try { complexSaMethodResults.RsResult = complexRsMethodAnalyzer.Analyse(); } catch { } }));
        complexMethodTasks.Add(Task.Run(() => { try { complexSaMethodResults.KzhaHorizontalResult = complexKzhaMethodHorizontalAnalyzer.Analyse(); } catch { } }));
        complexMethodTasks.Add(Task.Run(() => { try { complexSaMethodResults.KzhaVerticalResult = complexKzhaMethodVerticalAnalyzer.Analyse(); } catch { } }));
        complexMethodTasks.Add(Task.Run(() => { try { complexSaMethodResults.StatmResult = statmAnalyzer.Analyse(); } catch { } }));

        // Ожидание
        foreach (var complesMethodTask in complexMethodTasks)
            await complesMethodTask;

        var saResult = new SteganalysisResults
        {
            ChiSquareHorizontalVolume = complexSaMethodResults.ChiSquareHorizontalResult.MessageRelativeVolume,
            ChiSquareVerticalVolume = complexSaMethodResults.ChiSquareVerticalResult.MessageRelativeVolume,
            RsVolume = complexSaMethodResults.RsResult.MessageRelativeVolume,
            KzhaHorizontalThreshold = complexSaMethodResults.KzhaHorizontalResult.Threshold,
            KzhaHorizontalMessageBitVolume = complexSaMethodResults.KzhaHorizontalResult.MessageBitsVolume,
            KzhaVerticalThreshold = complexSaMethodResults.KzhaVerticalResult.Threshold,
            KzhaVerticalMessageBitVolume = complexSaMethodResults.KzhaVerticalResult.MessageBitsVolume,
            NoiseValue = complexSaMethodResults.StatmResult.NoiseValue,
            SharpnessValue = complexSaMethodResults.StatmResult.SharpnessValue,
            BlurValue = complexSaMethodResults.StatmResult.BlurValue,
            ContrastValue = complexSaMethodResults.StatmResult.ContrastValue,
            EntropyShennonValue = complexSaMethodResults.StatmResult.EntropyValues.Shennon,
            EntropyRenyiValue = complexSaMethodResults.StatmResult.EntropyValues.Renyi,
            PixelsNumber = (image?.Width ?? 0) * (image?.Height ?? 0)
        };

        bool isHidingDetected = SteganalysisDecision.Calculate(saResult);

        return new JsonResult(isHidingDetected);
    }

    private ContentResult GetErrorResult(string message) =>
        new ContentResult()
        {
            Content = message ?? string.Empty,
            ContentType = "text/plain; charset=utf-8",
            StatusCode = 400
        };
}
