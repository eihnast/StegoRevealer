using Microsoft.AspNetCore.Mvc;
using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.ComplexAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.FanAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.AnalysisMethods.SamplePairAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Entities;
using StegoRevealer.StegoCore.AnalysisMethods.ZhilkinCompressionAnalysis;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.CommonLib.Entities;
using StegoRevealer.StegoCore.DecisionModule;
using StegoRevealer.StegoCore.ImageHandlerLib;
using System.Security.Cryptography;

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
    public async Task<IActionResult> GetDecisionAsync(string path, bool verboseResult = false)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
                return GetErrorResult("������� ������ ���� �����������");

            // ������ ������������ ������������
            var image = new ImageHandler(path);
            var complexMethodTasks = new List<Task>();
            if (image is null)
                return GetErrorResult("�� ������� ������� ���������� �����������");


            var jointAnalysisParams = new JointAnalysisMethodsParameters();

            // �������� �����
            jointAnalysisParams.ChiSquareParameters = new ChiSquareParameters(image);
            jointAnalysisParams.RsParameters = new RsParameters(image);
            jointAnalysisParams.SpaParameters = new SpaParameters(image);
            jointAnalysisParams.FanParameters = new FanParameters(image);
            jointAnalysisParams.KzhaParameters = new KzhaParameters(image);
            jointAnalysisParams.StatmParameters = new StatmParameters(image);
            jointAnalysisParams.ComplexSaMethodParameters = new ComplexSaMethodParameters(image);

            var result = await JointAnalysisStarter.Start(jointAnalysisParams);

            return new JsonResult(new
            {
                result.ComplexSaMethodResults?.IsHidingDetected,
                steganalysisResult = verboseResult ? result : null
            });
        }
        catch (Exception e)
        {
            return GetErrorResult(e.Message);
        }
    }
    
    [HttpGet]
    public async Task<IActionResult> ChiSqrAsync(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
                return GetErrorResult("������� ������ ���� �����������");

            var image = new ImageHandler(path);
            if (image is null)
                return GetErrorResult("�� ������� ������� ���������� �����������");

            ChiSquareResult? result = null;
            var chiSqr = new ChiSquareAnalyser(image);
            await Task.Run(() => result = chiSqr.Analyse());

            return new JsonResult(result);
        }
        catch (Exception e)
        {
            return GetErrorResult(e.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> RsAsync(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
                return GetErrorResult("������� ������ ���� �����������");

            var image = new ImageHandler(path);
            if (image is null)
                return GetErrorResult("�� ������� ������� ���������� �����������");

            RsResult? result = null;
            var rs = new RsAnalyser(image);
            await Task.Run(() => result = rs.Analyse());

            return new JsonResult(result);
        }
        catch (Exception e)
        {
            return GetErrorResult(e.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> SpaAsync(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
                return GetErrorResult("������� ������ ���� �����������");

            var image = new ImageHandler(path);
            if (image is null)
                return GetErrorResult("�� ������� ������� ���������� �����������");

            SpaResult? result = null;
            var spa = new SpaAnalyser(image);
            await Task.Run(() => result = spa.Analyse());

            return new JsonResult(result);
        }
        catch (Exception e)
        {
            return GetErrorResult(e.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> FanAsync(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
                return GetErrorResult("������� ������ ���� �����������");

            var image = new ImageHandler(path);
            if (image is null)
                return GetErrorResult("�� ������� ������� ���������� �����������");

            FanResult? result = null;
            var fan = new FanAnalyser(image);
            await Task.Run(() => result = fan.Analyse());

            return new JsonResult(result);
        }
        catch (Exception e)
        {
            return GetErrorResult(e.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> KzhaAsync(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
                return GetErrorResult("������� ������ ���� �����������");

            var image = new ImageHandler(path);
            if (image is null)
                return GetErrorResult("�� ������� ������� ���������� �����������");

            KzhaResult? result = null;
            var kzha = new KzhaAnalyser(image);
            await Task.Run(() => result = kzha.Analyse());

            return new JsonResult(result);
        }
        catch (Exception e)
        {
            return GetErrorResult(e.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> ZcaAsync(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
                return GetErrorResult("������� ������ ���� �����������");

            var image = new ImageHandler(path);
            if (image is null)
                return GetErrorResult("�� ������� ������� ���������� �����������");

            ZcaResult? result = null;
            var zca = new ZcaAnalyser(image);
            await Task.Run(() => result = zca.Analyse());

            return new JsonResult(result);
        }
        catch (Exception e)
        {
            return GetErrorResult(e.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> StatsAsync(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
                return GetErrorResult("������� ������ ���� �����������");

            var image = new ImageHandler(path);
            if (image is null)
                return GetErrorResult("�� ������� ������� ���������� �����������");

            StatmResult? result = null;
            var statm = new StatmAnalyser(image);
            statm.Params.EntropyMethods = EntropyMethods.All;
            await Task.Run(() => result = statm.Analyse());

            return new JsonResult(result);
        }
        catch (Exception e)
        {
            return GetErrorResult(e.Message);
        }
    }

    private ContentResult GetErrorResult(string message) =>
        new ContentResult()
        {
            Content = message ?? string.Empty,
            ContentType = "text/plain; charset=utf-8",
            StatusCode = 400
        };
}
