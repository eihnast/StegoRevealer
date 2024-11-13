using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer_StegoCore_TrainingModule;
using System.Diagnostics;

namespace StegoRevealer.StegoCore.TrainingModule;

internal class Program
{
    //static async Task Main(string[] args)
    //{
    //    var inputImages = Directory.GetFiles("d:\\Temp\\_training\\_Test600Plus");
    //    var realImages = new List<string>();
    //    var analysedData = new List<StegoModelTest1.ModelInput>();
    //    foreach (var imgPath in inputImages)
    //    {
    //        var img = new ImageHandler(imgPath);

    //        var horizonalChiSqr = new ChiSquareAnalyser(img);
    //        horizonalChiSqr.Params.TraverseType = TraverseType.Horizontal;

    //        var verticalChiSqr = new ChiSquareAnalyser(img);
    //        verticalChiSqr.Params.TraverseType = TraverseType.Vertical;
    //        verticalChiSqr.Params.BlockWidth = 1;
    //        verticalChiSqr.Params.BlockHeight = img.Height;

    //        var horizonotalKzha = new KzhaAnalyser(img);
    //        horizonotalKzha.Params.TraverseType = TraverseType.Horizontal;

    //        var verticalKzha = new KzhaAnalyser(img);
    //        verticalKzha.Params.TraverseType = TraverseType.Vertical;

    //        var rs = new RsAnalyser(img);
    //        var statm = new StatmAnalyser(img);


    //        ChiSquareResult? horizonalChiSqrResult = null;
    //        ChiSquareResult? verticalChiSqrResult = null;
    //        KzhaResult? horizonotalKzhaResult = null;
    //        KzhaResult? verticalKzhaResult = null;
    //        RsResult? rsResult = null;
    //        StatmResult? statmResult = null;
    //        var analysisTasks = new List<Task>
    //        {
    //            Task.Run(() => horizonalChiSqrResult = horizonalChiSqr.Analyse()),
    //            Task.Run(() => verticalChiSqrResult = verticalChiSqr.Analyse()),
    //            Task.Run(() => horizonotalKzhaResult = horizonotalKzha.Analyse()),
    //            Task.Run(() => verticalKzhaResult = verticalKzha.Analyse()),
    //            Task.Run(() => rsResult = rs.Analyse()),
    //            Task.Run(() => statmResult = statm.Analyse())
    //        };

    //        foreach (var task in analysisTasks)
    //            await task;

    //        if (horizonalChiSqrResult is null || verticalChiSqrResult is null || horizonotalKzhaResult is null || verticalKzhaResult is null ||
    //            rsResult is null || statmResult is null)
    //            continue;

    //        var kzhVolume = ContainerVolumeForKzh(img);
    //        realImages.Add(imgPath);
    //        analysedData.Add(new StegoModelTest1.ModelInput
    //        {
    //            ChiSquareVolume = (float)horizonalChiSqrResult.MessageRelativeVolume,
    //            RsVolume = (float)rsResult.MessageRelativeVolume,
    //            KzhaThreshold = (float)horizonotalKzhaResult.Threshold,
    //            KzhaMessageVolume = horizonotalKzhaResult.MessageBitsVolume / kzhVolume,
    //            ChiSquareVolume_Vertical = (float)verticalChiSqrResult.MessageRelativeVolume,
    //            KzhaThreshold_Vertical = (float)verticalKzhaResult.Threshold,
    //            KzhaMessageVolume_Vertical = verticalKzhaResult.MessageBitsVolume / kzhVolume,
    //            NoiseValue = (float)statmResult.NoiseValue,
    //            SharpnessValue = (float)statmResult.SharpnessValue,
    //            BlurValue = (float)statmResult.BlurValue,
    //            ContrastValue = (float)statmResult.ContrastValue,
    //            EntropyShennonValue = (float)statmResult.EntropyValues.Shennon,
    //            EntropyRenyiValue = (float)statmResult.EntropyValues.Renyi
    //        });

    //        Console.WriteLine($"Завершён анализ для {imgPath}");
    //    }

    //    //var results = new float[] { 0f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 0f, 1f, 0f, 0f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 0f, 0f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 0f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 0f, 0f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 0f, 0f, 1f, 1f, 1f, 0f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 0f, 0f, 0f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 0f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 0f, 0f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 0f, 0f, 0f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f };
    //    var results = new float[] { 0f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 0f, 1f, 0f, 0f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 0f, 0f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 0f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 0f, 0f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 0f, 0f, 1f, 1f, 1f, 0f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 0f, 0f, 0f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 0f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 0f, 0f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 0f, 0f, 0f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };
    //    //var resultsList = Enumerable.Repeat(0f, 4316).ToList();
    //    //resultsList.AddRange(Enumerable.Repeat(1f, 4316).ToList());
    //    //var results = resultsList.ToArray();

    //    var boolResults = results.Select(r => r == 1f).ToArray();
    //    if (results.Length != analysedData.Count)
    //    {
    //        Console.WriteLine($"Размеры не совпадают: results.Count == {results.Length}, analysedData.Count == {analysedData.Count}");
    //        return;
    //    }

    //    for (int i = 0; i < analysedData.Count; i++)
    //    {
    //        var hided = StegoModelTest1.Predict(analysedData[i]);
    //        bool isHidedPredicted = hided.PredictedLabel;
    //        bool isHidedReal = boolResults[i];
    //        if (isHidedPredicted != isHidedReal)
    //            Console.WriteLine($"Не совпало для {Path.GetFileNameWithoutExtension(realImages[i])}. Получено: {isHidedPredicted}, Ожидалось: {isHidedReal}. Изображение: {realImages[i]}");
    //    }
    //}

    static async Task Main(string[] args)
    {
        var inputImages = Directory.GetFiles("d:\\Temp\\_StegoRevealer\\_training\\_Test600Plus");
        var realImages = new List<string>();
        var analysedData = new List<DecisionModel.ModelInput>();

        var elapsed1920 = new List<long>();
        var elapsed1280 = new List<long>();
        var timer = new Stopwatch();

        foreach (var imgPath in inputImages)
        {
            timer.Start();

            var img = new ImageHandler(imgPath);

            var horizonalChiSqr = new ChiSquareAnalyser(img);
            horizonalChiSqr.Params.TraverseType = TraverseType.Horizontal;

            var verticalChiSqr = new ChiSquareAnalyser(img);
            verticalChiSqr.Params.TraverseType = TraverseType.Vertical;
            verticalChiSqr.Params.BlockWidth = 1;
            verticalChiSqr.Params.BlockHeight = img.Height;

            var horizonotalKzha = new KzhaAnalyser(img);
            horizonotalKzha.Params.TraverseType = TraverseType.Horizontal;

            var verticalKzha = new KzhaAnalyser(img);
            verticalKzha.Params.TraverseType = TraverseType.Vertical;

            var rs = new RsAnalyser(img);
            var statm = new StatmAnalyser(img);


            ChiSquareResult? horizonalChiSqrResult = null;
            KzhaResult? horizonotalKzhaResult = null;
            RsResult? rsResult = null;
            StatmResult? statmResult = null;
            var analysisTasks = new List<Task>
            {
                Task.Run(() => horizonalChiSqrResult = horizonalChiSqr.Analyse()),
                Task.Run(() => horizonotalKzhaResult = horizonotalKzha.Analyse()),
                Task.Run(() => rsResult = rs.Analyse()),
                Task.Run(() => statmResult = statm.Analyse())
            };

            foreach (var task in analysisTasks)
                await task;

            if (horizonalChiSqrResult is null || horizonotalKzhaResult is null || rsResult is null || statmResult is null)
                continue;

            var kzhVolume = ContainerVolumeForKzh(img);
            realImages.Add(imgPath);
            analysedData.Add(new DecisionModel.ModelInput
            {
                ChiSquareVolume = (float)horizonalChiSqrResult.MessageRelativeVolume,
                RsVolume = (float)rsResult.MessageRelativeVolume,
                KzhaThreshold = (float)horizonotalKzhaResult.Threshold,
                KzhaMessageVolume = horizonotalKzhaResult.MessageBitsVolume / kzhVolume,
                NoiseValue = (float)statmResult.NoiseValue,
                SharpnessValue = (float)statmResult.SharpnessValue,
                BlurValue = (float)statmResult.BlurValue,
                ContrastValue = (float)statmResult.ContrastValue,
                EntropyShennonValue = (float)statmResult.EntropyValues.Shennon,
                EntropyRenyiValue = (float)statmResult.EntropyValues.Renyi
            });

            Console.WriteLine($"Завершён анализ для {imgPath}");

            timer.Stop();
            if (img.Width == 1920 && img.Height == 1080)
                elapsed1920.Add(timer.ElapsedMilliseconds);
            else if (img.Width == 1280 && img.Height == 720)
                elapsed1280.Add(timer.ElapsedMilliseconds);
            timer.Reset();
        }

        var results = new float[] { 0f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 0f, 1f, 0f, 0f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 0f, 0f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 0f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 0f, 0f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 0f, 0f, 1f, 1f, 1f, 0f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 0f, 0f, 0f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 0f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 0f, 0f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 0f, 0f, 0f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };
        //var resultsList = Enumerable.Repeat(0f, 4316).ToList();
        //resultsList.AddRange(Enumerable.Repeat(1f, 4316).ToList());
        //var results = resultsList.ToArray();

        if (results.Length != analysedData.Count)
        {
            Console.WriteLine($"Размеры не совпадают: results.Count == {results.Length}, analysedData.Count == {analysedData.Count}");
            return;
        }

        var boolResults = results.Select(r => r == 1f).ToArray();
        for (int i = 0; i < analysedData.Count; i++)
        {
            var hided = DecisionModel.Predict(analysedData[i]);
            bool isHidedPredicted = hided.PredictedLabel;
            bool isHidedReal = boolResults[i];
            if (isHidedPredicted != isHidedReal)
                Console.WriteLine($"Не совпало для {i}. Получено: {isHidedPredicted}, Ожидалось: {isHidedReal}. Изображение: {realImages[i]}");
        }

        double averageelapsed1920Elapsed = elapsed1920.Average();
        double averageelapsed1280Elapsed = elapsed1280.Average();
        Console.WriteLine($"1920. Среднее время: {averageelapsed1920Elapsed}. Максимальное: {elapsed1920.Max()}. Минимальное: {elapsed1920.Min()}.");
        Console.WriteLine($"1280. Среднее время: {averageelapsed1280Elapsed}. Максимальное: {elapsed1280.Max()}. Минимальное: {elapsed1280.Min()}.");
    }

    private static int ContainerVolumeForKzh(ImageHandler img) => (img.Height / 8) * (img.Width / 8);

    // static void Main(string[] args) { }
}
