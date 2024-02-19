using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics;
using StegoRevealer.StegoCore.ImageHandlerLib;
using System.Collections.Concurrent;

namespace StegoRevealer.StegoCore.ModuleTests;

[TestClass]
public class ImageHandlerTests
{
    [TestMethod]
    public void CaHandleInMultithreading()
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

                results.TryUpdate(filename, (rsResult is null ? -1.0 : rsResult.MessageRelativeVolume, statmResult is null ? -1.0 : statmResult.NoiseValueMethod2), (0.0, 0.0));
            }));
        }

        foreach (var imgTask in imgTasks)
            imgTask.Start();
        foreach (var imgTask in imgTasks)
            imgTask.Wait();
    }
}