using Microsoft.AspNetCore.Mvc;
using StegoRevealer.Common;
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
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevelaer.API.Entities.RequestData;
using StegoRevelaer.API.Services;
using static System.Net.Mime.MediaTypeNames;

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
    public async Task<IActionResult> GetDecisionAsync(string path, bool verboseResult = false) => await ComplexSsaAsync(path, verboseResult);

    [HttpGet]
    public async Task<IActionResult> ComplexSsaAsync(string path, bool verboseResult = false)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
                return GetBadRequest("Передан пустой путь изображения");

            var image = CreateImageHandler(path);
            if (image is null)
                return GetBadRequest("Не удалось создать обработчик изображения");


            ComplexSaMethodResult? result = null;
            var complesSa = new ComplexSaMethodAnalyser(image);
            await Task.Run(() => result = complesSa.Analyse());

            ClearTempForProcessedImage(image);

            return new JsonResult(new
            {
                result?.IsHidingDetected,
                steganalysisResult = verboseResult ? result : null
            });
        }
        catch (Exception e)
        {
            return GetBadRequest(e.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> GetDecisionAsync(ComplexSsaRequest request) => await ComplexSsaAsync(request);

    [HttpPost]
    public async Task<IActionResult> ComplexSsaAsync(ComplexSsaRequest request)
    {
        try
        {
            var imgPath = await TryGetImage(request);

            if (string.IsNullOrEmpty(imgPath))
                return GetBadRequest("Не удалось загрузить указанное изображение");

            var image = CreateImageHandler(imgPath);
            if (image is null)
                return GetBadRequest("Не удалось создать обработчик изображения");

            ComplexSaMethodResult? result = null;
            var complexSsaParams = request.CreateParameters(image);
            var complexSsa = new ComplexSaMethodAnalyser(complexSsaParams);
            await Task.Run(() => result = complexSsa.Analyse());

            ClearTempForProcessedImage(image);

            return new JsonResult(result?.HasErrors is null or true
                ? new { Result = result, Errors = result?.GetErrors() }
                : result);
        }
        catch (Exception e)
        {
            return GetBadRequest(e.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> FullAsync(string path) => await FullAnalysisAsync(path);

    [HttpGet]
    public async Task<IActionResult> FullAnalysisAsync(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
                return GetBadRequest("Передан пустой путь изображения");

            var image = CreateImageHandler(path);
            if (image is null)
                return GetBadRequest("Не удалось создать обработчик изображения");


            var jointAnalysisParams = new JointAnalysisMethodsParameters();

            // Создание параметров всех анализаторов
            jointAnalysisParams.ChiSquareParameters = new ChiSquareParameters(image);
            jointAnalysisParams.RsParameters = new RsParameters(image);
            jointAnalysisParams.SpaParameters = new SpaParameters(image);
            jointAnalysisParams.FanParameters = new FanParameters(image);
            jointAnalysisParams.ZcaParameters = new ZcaParameters(image);
            jointAnalysisParams.KzhaParameters = new KzhaParameters(image);
            jointAnalysisParams.StatmParameters = new StatmParameters(image);
            jointAnalysisParams.ComplexSaMethodParameters = new ComplexSaMethodParameters(image);

            var result = await JointAnalysisStarter.Start(jointAnalysisParams);

            ClearTempForProcessedImage(image);

            return new JsonResult(result);
        }
        catch (Exception e)
        {
            return GetBadRequest(e.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> FullAsync(FullAnalysisRequest request) => await FullAnalysisAsync(request);

    [HttpPost]
    public async Task<IActionResult> FullAnalysisAsync(FullAnalysisRequest request)
    {
        try
        {
            var imgPath = await TryGetImage(request);

            if (string.IsNullOrEmpty(imgPath))
                return GetBadRequest("Не удалось загрузить указанное изображение");

            var image = CreateImageHandler(imgPath);
            if (image is null)
                return GetBadRequest("Не удалось создать обработчик изображения");

            var jointAnalysisParams = request.CreateParameters(image);
            var result = await JointAnalysisStarter.Start(jointAnalysisParams);

            ClearTempForProcessedImage(image);

            var errors = result?.CollectErrors();
            return new JsonResult(errors is null || errors.Count == 0
                ? new { Result = result, Errors = errors }
                : result);
        }
        catch (Exception e)
        {
            return GetBadRequest(e.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> CsaAsync(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
                return GetBadRequest("Передан пустой путь изображения");

            var image = CreateImageHandler(path);
            if (image is null)
                return GetBadRequest("Не удалось создать обработчик изображения");

            ChiSquareResult? result = null;
            var chiSqr = new ChiSquareAnalyser(image);
            await Task.Run(() => result = chiSqr.Analyse());

            ClearTempForProcessedImage(image);

            return new JsonResult(result);
        }
        catch (Exception e)
        {
            return GetBadRequest(e.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CsaAsync(CsaRequest request)
    {
        try
        {
            var imgPath = await TryGetImage(request);

            if (string.IsNullOrEmpty(imgPath))
                return GetBadRequest("Не удалось загрузить указанное изображение");

            var image = CreateImageHandler(imgPath);
            if (image is null)
                return GetBadRequest("Не удалось создать обработчик изображения");

            ChiSquareResult? result = null;
            var csaParams = request.CreateParameters(image);
            var csa = new ChiSquareAnalyser(csaParams);
            await Task.Run(() => result = csa.Analyse());

            ClearTempForProcessedImage(image);

            return new JsonResult(result?.HasErrors is null or true 
                ? new { Result = result, Errors = result?.GetErrors() }
                : result);
        }
        catch (Exception e)
        {
            return GetBadRequest(e.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> RsAsync(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
                return GetBadRequest("Передан пустой путь изображения");

            var image = CreateImageHandler(path);
            if (image is null)
                return GetBadRequest("Не удалось создать обработчик изображения");

            RsResult? result = null;
            var rs = new RsAnalyser(image);
            await Task.Run(() => result = rs.Analyse());

            ClearTempForProcessedImage(image);

            return new JsonResult(result);
        }
        catch (Exception e)
        {
            return GetBadRequest(e.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> RsAsync(RsRequest request)
    {
        try
        {
            var imgPath = await TryGetImage(request);

            if (string.IsNullOrEmpty(imgPath))
                return GetBadRequest("Не удалось загрузить указанное изображение");

            var image = CreateImageHandler(imgPath);
            if (image is null)
                return GetBadRequest("Не удалось создать обработчик изображения");

            RsResult? result = null;
            var rsParams = request.CreateParameters(image);
            var rs = new RsAnalyser(rsParams);
            await Task.Run(() => result = rs.Analyse());

            ClearTempForProcessedImage(image);

            return new JsonResult(result?.HasErrors is null or true
                ? new { Result = result, Errors = result?.GetErrors() }
                : result);
        }
        catch (Exception e)
        {
            return GetBadRequest(e.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> SpaAsync(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
                return GetBadRequest("Передан пустой путь изображения");

            var image = CreateImageHandler(path);
            if (image is null)
                return GetBadRequest("Не удалось создать обработчик изображения");

            SpaResult? result = null;
            var spa = new SpaAnalyser(image);
            await Task.Run(() => result = spa.Analyse());

            ClearTempForProcessedImage(image);

            return new JsonResult(result);
        }
        catch (Exception e)
        {
            return GetBadRequest(e.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> SpaAsync(SpaRequest request)
    {
        try
        {
            var imgPath = await TryGetImage(request);

            if (string.IsNullOrEmpty(imgPath))
                return GetBadRequest("Не удалось загрузить указанное изображение");

            var image = CreateImageHandler(imgPath);
            if (image is null)
                return GetBadRequest("Не удалось создать обработчик изображения");

            SpaResult? result = null;
            var spaParams = request.CreateParameters(image);
            var spa = new SpaAnalyser(spaParams);
            await Task.Run(() => result = spa.Analyse());

            ClearTempForProcessedImage(image);

            return new JsonResult(result?.HasErrors is null or true
                ? new { Result = result, Errors = result?.GetErrors() }
                : result);
        }
        catch (Exception e)
        {
            return GetBadRequest(e.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> FanAsync(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
                return GetBadRequest("Передан пустой путь изображения");

            var image = CreateImageHandler(path);
            if (image is null)
                return GetBadRequest("Не удалось создать обработчик изображения");

            FanResult? result = null;
            var fan = new FanAnalyser(image);
            await Task.Run(() => result = fan.Analyse());

            ClearTempForProcessedImage(image);

            return new JsonResult(result);
        }
        catch (Exception e)
        {
            return GetBadRequest(e.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> FanAsync(FanRequest request)
    {
        try
        {
            var imgPath = await TryGetImage(request);

            if (string.IsNullOrEmpty(imgPath))
                return GetBadRequest("Не удалось загрузить указанное изображение");

            var image = CreateImageHandler(imgPath);
            if (image is null)
                return GetBadRequest("Не удалось создать обработчик изображения");

            FanResult? result = null;
            var fanParams = request.CreateParameters(image);
            var fan = new FanAnalyser(fanParams);
            await Task.Run(() => result = fan.Analyse());

            ClearTempForProcessedImage(image);

            return new JsonResult(result?.HasErrors is null or true
                ? new { Result = result, Errors = result?.GetErrors() }
                : result);
        }
        catch (Exception e)
        {
            return GetBadRequest(e.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> CkzhaAsync(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
                return GetBadRequest("Передан пустой путь изображения");

            var image = CreateImageHandler(path);
            if (image is null)
                return GetBadRequest("Не удалось создать обработчик изображения");

            KzhaResult? result = null;
            var kzha = new KzhaAnalyser(image);
            await Task.Run(() => result = kzha.Analyse());

            ClearTempForProcessedImage(image);

            return new JsonResult(result);
        }
        catch (Exception e)
        {
            return GetBadRequest(e.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CkzhaAsync(CkzhaRequest request)
    {
        try
        {
            var imgPath = await TryGetImage(request);

            if (string.IsNullOrEmpty(imgPath))
                return GetBadRequest("Не удалось загрузить указанное изображение");

            var image = CreateImageHandler(imgPath);
            if (image is null)
                return GetBadRequest("Не удалось создать обработчик изображения");

            KzhaResult? result = null;
            var kzhaParams = request.CreateParameters(image);
            var kzha = new KzhaAnalyser(kzhaParams);
            await Task.Run(() => result = kzha.Analyse());

            ClearTempForProcessedImage(image);

            return new JsonResult(result?.HasErrors is null or true
                ? new { Result = result, Errors = result?.GetErrors() }
                : result);
        }
        catch (Exception e)
        {
            return GetBadRequest(e.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> ZcaAsync(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
                return GetBadRequest("Передан пустой путь изображения");

            var image = CreateImageHandler(path);
            if (image is null)
                return GetBadRequest("Не удалось создать обработчик изображения");

            ZcaResult? result = null;
            var zca = new ZcaAnalyser(image);
            await Task.Run(() => result = zca.Analyse());

            ClearTempForProcessedImage(image);

            return new JsonResult(result);
        }
        catch (Exception e)
        {
            return GetBadRequest(e.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> ZcaAsync(ZcaRequest request)
    {
        try
        {
            var imgPath = await TryGetImage(request);

            if (string.IsNullOrEmpty(imgPath))
                return GetBadRequest("Не удалось загрузить указанное изображение");

            var image = CreateImageHandler(imgPath);
            if (image is null)
                return GetBadRequest("Не удалось создать обработчик изображения");

            ZcaResult? result = null;
            var zcaParams = request.CreateParameters(image);
            var zca = new ZcaAnalyser(zcaParams);
            await Task.Run(() => result = zca.Analyse());

            ClearTempForProcessedImage(image);

            return new JsonResult(result?.HasErrors is null or true
                ? new { Result = result, Errors = result?.GetErrors() }
                : result);
        }
        catch (Exception e)
        {
            return GetBadRequest(e.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> StatmAsync(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
                return GetBadRequest("Передан пустой путь изображения");

            var image = CreateImageHandler(path);
            if (image is null)
                return GetBadRequest("Не удалось создать обработчик изображения");

            StatmResult? result = null;
            var statm = new StatmAnalyser(image);
            statm.Params.EntropyMethods = EntropyMethods.All;
            await Task.Run(() => result = statm.Analyse());

            ClearTempForProcessedImage(image);

            return new JsonResult(result);
        }
        catch (Exception e)
        {
            return GetBadRequest(e.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> StatmAsync(StatmRequest request)
    {
        try
        {
            var imgPath = await TryGetImage(request);

            if (string.IsNullOrEmpty(imgPath))
                return GetBadRequest("Не удалось загрузить указанное изображение");

            var image = CreateImageHandler(imgPath);
            if (image is null)
                return GetBadRequest("Не удалось создать обработчик изображения");

            StatmResult? result = null;
            var statmParams = request.CreateParameters(image);
            var statm = new StatmAnalyser(statmParams);
            await Task.Run(() => result = statm.Analyse());

            ClearTempForProcessedImage(image);

            return new JsonResult(result?.HasErrors is null or true
                ? new { Result = result, Errors = result?.GetErrors() }
                : result);
        }
        catch (Exception e)
        {
            return GetBadRequest(e.Message);
        }
    }


    /* HELPERS */

    private ContentResult GetSimpleResult(string message, int code = 400) =>
        new ContentResult()
        {
            Content = message ?? string.Empty,
            ContentType = "text/plain; charset=utf-8",
            StatusCode = code
        };

    private ContentResult GetNotFound(string message) => GetSimpleResult(message, 404);
    private ContentResult GetBadRequest(string message) => GetSimpleResult(message, 400);

    private async Task<string?> TryGetImage(BaseAnalysisRequest baseRequest)
    {
        if (!string.IsNullOrEmpty(baseRequest.ImageUrl))
        {
            if (Tools.IsWebPath(baseRequest.ImageUrl))
            {
                string filename = Path.GetFileName(baseRequest.ImageUrl);
                string tempFilename = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(filename);
                string tempPath = Path.Combine(Tools.GetOrCreateTempDirPath(), tempFilename);

                try
                {
                    using var client = new HttpClient();
                    using var stream = await client.GetStreamAsync(baseRequest.ImageUrl);
                    using var fs = new FileStream(tempPath, FileMode.OpenOrCreate);
                    await stream.CopyToAsync(fs);
                }
                catch (Exception ex)
                {
                    ApiLogger.LogError($"Error while loading image by url '{baseRequest.ImageUrl}': {ex.Message}");
                    return null;
                }

                TempManager.Instance.RememberTempImage(baseRequest.ImageUrl, tempPath);
                return tempPath;
            }
            else if (Tools.IsLocalPath(baseRequest.ImageUrl))
            {
                try
                {
                    var tempPath = Tools.CopyFileToTemp(baseRequest.ImageUrl);
                    if (!string.IsNullOrEmpty(tempPath))
                    {
                        TempManager.Instance.RememberTempImage(baseRequest.ImageUrl, tempPath);
                        return tempPath;
                    }
                }
                catch (Exception ex)
                {
                    ApiLogger.LogError($"Error while loading local image '{baseRequest.ImageUrl}': {ex.Message}");
                    return null;
                }
            }
            else
            {
                ApiLogger.LogError($"Error while loading image: cannot correctly handle ImageUrl");
            }

            return null;
        }
        else if (!string.IsNullOrEmpty(baseRequest.ImageData))
        {
            var data = Convert.FromBase64String(baseRequest.ImageData);

            string tempFilename = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".png";
            string tempPath = Path.Combine(Tools.GetOrCreateTempDirPath(), tempFilename);

            try
            {
                if (!Tools.TrySaveImageFromBytes(data, tempPath))
                    return null;
            }
            catch (Exception ex)
            {
                ApiLogger.LogError($"Error while loading base64 encoded image: {ex.Message}");
                return null;
            }

            TempManager.Instance.RememberTempImage(Tools.GetStartOfBase64(baseRequest.ImageData), tempPath);
            return tempPath;
        }

        return null;
    }

    private ImageHandler? CreateImageHandler(string imgPath)
    {
        try
        {
            var handler = new ImageHandler(imgPath);
            TempManager.Instance.RememberHandler(handler);
            ApiLogger.LogInfo($"Loaded new image for steganalysis: {handler.ImgPath}");
            return handler;
        }
        catch (Exception ex)
        {
            ApiLogger.LogError($"Error while creating image handler for '{imgPath}': {ex.Message}");
        }

        return null;
    }

    private static void ClearTempForProcessedImage(ImageHandler image)
    {
        image.CloseHandler();
        TempManager.Instance.ForgetHandler(image);
        TempManager.Instance.DeleteTempImages(onlyWithoutHandlers: true, writeToLog: false, logger: ApiLogger.Instance);
        TempManager.Instance.DeleteTempFiles(writeToLog: false, logger: ApiLogger.Instance);
    }
}
