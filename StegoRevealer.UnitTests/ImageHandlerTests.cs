using Accord.Math;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics;
using StegoRevealer.StegoCore.ImageHandlerLib;
using System.Collections.Concurrent;

namespace StegoRevealer.StegoCore.ModuleTests;

[TestClass]
public class ImageHandlerTests
{
    [TestMethod]
    public void CanHandleInMultithreading()
    {
        string imgPattern = "mt_test_";

        var imgPaths = new List<string>();
        for (int i = 1; i <= 10; i++)
            imgPaths.Add(Path.Combine(Helper.GetAssemblyDir(), "TestData", $"{imgPattern}{i}.png"));

        var results = new ConcurrentDictionary<string, (double Rs, double Noise)>();
        for (int i = 0; i < 10; i++)
            results.TryAdd(Path.GetFileNameWithoutExtension(imgPaths[i]), (0.0, 0.0));

        var imgTasks = new List<Task>();
        foreach (var imgPath in imgPaths)
        {
            imgTasks.Add(new Task(() =>
            {
                string filename = Path.GetFileNameWithoutExtension(imgPath);

                var img = new ImageHandler(imgPath.ToString());
                var rsAnalyzer = new RsAnalyser(img);
                var statmAnalyzer = new StatmAnalyser(img);

                RsResult? rsResult = null;
                StatmResult? statmResult = null;

                var rsTask = new Task(() => rsResult = rsAnalyzer.Analyse());
                var statmTask = new Task(() => statmResult = statmAnalyzer.Analyse());

                rsTask.Start();
                statmTask.Start();
                rsTask.Wait();
                statmTask.Wait();

                results.TryUpdate(filename, (rsResult is null ? -1.0 : rsResult.MessageRelativeVolume, statmResult is null ? -1.0 : statmResult.NoiseValue), (0.0, 0.0));
            }));
        }

        string error = string.Empty;

        try
        {
            foreach (var imgTask in imgTasks)
                imgTask.Start();
            foreach (var imgTask in imgTasks)
                imgTask.Wait();
        }
        catch (Exception ex)
        {
            error = ex.Message;
        }

        Assert.IsTrue(string.IsNullOrEmpty(error));
    }

    [TestMethod]
    public void CloningTest()
    {
        const string imgName = "HandlerCloningTest.png";
        string path = Path.Combine(Helper.GetAssemblyDir(), "TestData", imgName);
        const byte oldColorByteValue = 0;
        const byte newColorByteValue = 255;

        var handler = new ImageHandler(path);  // Создали обработчик
        var clonedBeforeChangeHandler = handler.Clone();  // Склонировали обработчик сразу, до любых изменений

        Assert.AreEqual(oldColorByteValue, handler.ImgArray[0, 0, 0]);    // На картинке там #000000, убедимся в этом на всякий случай
        Assert.AreEqual(oldColorByteValue, clonedBeforeChangeHandler.ImgArray[0, 0, 0]);    // В склонированном обработчике тоже 0

        handler.ImgArray[0, 0, 0] = newColorByteValue;  // Меняем в оригинальном обработчике 0 на 255
        Assert.AreEqual(newColorByteValue, handler.ImgArray[0, 0, 0]);  // На всякий случай убедимся, что в оригинальном обработчике значение поменялось
        Assert.AreEqual(oldColorByteValue, clonedBeforeChangeHandler.ImgArray[0, 0, 0]);  // Проверяем, что в склонированном до изменений обработчике значение не поменялось

        var clonedAfterHandler = handler.Clone();  // Склонировали обработчик (уже после изменения)
        Assert.AreEqual(0, clonedAfterHandler.ImgArray[0, 0, 0]);  // Промеряем, что в склонированном обработчике осталось старое значение


        // Проверка сохранения склонированных с многопоточностью
        var cloneTasks = new List<Task<ImageHandler>>();
        for (int i = 0; i < 20; i++)
            cloneTasks.Add(new Task<ImageHandler>(handler.Clone));

        for (int i = 0; i < 20; i++)
            cloneTasks[i].Start();
        for (int i = 0; i < 20; i++)
            cloneTasks[i].Wait();

        var saveTasks = new List<Task<string?>>();
        for (int i = 0; i < 20; i++)
        {
            string newImgName = $"cloned_{i}";
            var clonedHandler = cloneTasks[i].Result;
            saveTasks.Add(new Task<string?>(() => clonedHandler.SaveNear(newImgName)));
        }

        for (int i = 0; i < 20; i++)
            saveTasks[i].Start();
        for (int i = 0; i < 20; i++)
            saveTasks[i].Wait();

        for (int i = 0; i < 20; i++)
            Assert.IsFalse(string.IsNullOrEmpty(saveTasks[i].Result));
    }
}