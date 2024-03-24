using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer_StegoCore_TrainingModule;

namespace StegoRevealer.StegoCore.TrainingModule;

internal class Program
{
    static void Main(string[] args)
    {
        var inputImages = Directory.GetFiles("d:\\Temp\\_training\\_Test");
        var realImages = new List<string>();
        var analysedData = new List<StegoModel.ModelInput>();
        foreach (var imgPath in inputImages)
        {
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
            ChiSquareResult? verticalChiSqrResult = null;
            KzhaResult? horizonotalKzhaResult = null;
            KzhaResult? verticalKzhaResult = null;
            RsResult? rsResult = null;
            StatmResult? statmResult = null;
            var analysisTasks = new List<Task>
            {
                Task.Run(() => horizonalChiSqrResult = horizonalChiSqr.Analyse()),
                Task.Run(() => verticalChiSqrResult = verticalChiSqr.Analyse()),
                Task.Run(() => horizonotalKzhaResult = horizonotalKzha.Analyse()),
                Task.Run(() => verticalKzhaResult = verticalKzha.Analyse()),
                Task.Run(() => rsResult = rs.Analyse()),
                Task.Run(() => statmResult = statm.Analyse())
            };

            foreach (var task in analysisTasks)
                task.Wait();

            if (horizonalChiSqrResult is null || verticalChiSqrResult is null || horizonotalKzhaResult is null || verticalKzhaResult is null ||
                rsResult is null || statmResult is null)
                continue;

            realImages.Add(imgPath);
            analysedData.Add(new StegoModel.ModelInput
            {
                ChiSquareVolume = (float)horizonalChiSqrResult.MessageRelativeVolume,
                RsVolume = (float)rsResult.MessageRelativeVolume,
                KzhaThreshold = (float)horizonotalKzhaResult.Threshold,
                KzhaMessageBitVolume = horizonotalKzhaResult.MessageBitsVolume,
                ChiSquareVolume_Vertical = (float)verticalChiSqrResult.MessageRelativeVolume,
                KzhaThreshold_Vertical = (float)verticalKzhaResult.Threshold,
                KzhaMessageBitVolume_Vertical = verticalKzhaResult.MessageBitsVolume,
                NoiseValue = (float)statmResult.NoiseValueMethod2,
                SharpnessValue = (float)statmResult.SharpnessValue,
                BlurValue = (float)statmResult.BlurValue,
                ContrastValue = (float)statmResult.ContrastValue,
                EntropyShennonValue = (float)statmResult.EntropyValues.Shennon,
                EntropyVaidaValue = (float)statmResult.EntropyValues.Vaida,
                EntropyTsallisValue = (float)statmResult.EntropyValues.Tsallis,
                EntropyRenyiValue = (float)statmResult.EntropyValues.Renyi,
                EntropyHavardValue = (float)statmResult.EntropyValues.Havard,
                PixelsNum = img.Height * img.Width,
            });

            Console.WriteLine($"Завершён анализ для {imgPath}");
        }

        var results = new List<float>() { 0f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 0f, 1f, 0f, 0f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 0f, 0f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 0f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 0f, 0f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 0f, 0f, 1f, 1f, 1f, 0f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 0f, 0f, 0f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 0f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 0f, 0f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 0f, 0f, 0f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f };
        if (results.Count != analysedData.Count)
        {
            Console.WriteLine($"Размеры не совпадают: results.Count == {results.Count}, analysedData.Count == {analysedData.Count}");
            return;
        }

        for (int i = 0; i < analysedData.Count; i++)
        {
            var hided = StegoModel.Predict(analysedData[i]);
            if (hided.PredictedLabel != results[i])
                Console.WriteLine($"Не совпало для {i}. Получено: {hided.PredictedLabel}, Ожидалось: {results[i]}. Изображение: {realImages[i]}");
        }
    }
}
