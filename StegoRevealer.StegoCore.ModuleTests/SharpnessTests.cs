using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Calculators;
using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevealer.StegoCore.ModuleTests;

[TestClass]
public class SharpnessTests
{
    [TestMethod]
    public void CannyDetectionTest()
    {
        string imagePath = Path.Combine(Helper.GetAssemblyDir(), "TestData", "sharpness_canny.png");
        var image = new ImageHandler(imagePath);

        var parameters = new StatmParameters(image);
        parameters.SharpnessCalcCannyUpThreshold = 0.7;  // Для тестового хороший: 0.7
        parameters.SharpnessCalcCannyDownThreshold = 0.65;  // Для тестового хороший: 0.65

        var sharpnessCalculator = new SharpnessCalculator(parameters);
        var canny = sharpnessCalculator.CannyEdgeDetection();

        var edgesImar = canny.EdgePixelsArray;
        var cannyImage = image.Clone();
        for (int y = 0; y < cannyImage.Height; y++)
            for (int x = 0; x < cannyImage.Width; x++)
                for (int channelId = 0; channelId < 3; channelId++)
                    cannyImage.ImgArray[y, x, channelId] = edgesImar[y, x];
        cannyImage.Save("sharpness_canny_applied");
    }

    [TestMethod]
    public void SobelDetectionTest()
    {
        string imagePath = Path.Combine(Helper.GetAssemblyDir(), "TestData", "sharpness_sobel.png");
        var image = new ImageHandler(imagePath);

        var parameters = new StatmParameters(image);
        var sharpnessCalculator = new SharpnessCalculator(parameters);
        var sobel = sharpnessCalculator.SobelEdgeDetection();

        var edgesImar = sobel;
        var cannyImage = image.Clone();
        for (int y = 0; y < cannyImage.Height; y++)
            for (int x = 0; x < cannyImage.Width; x++)
                for (int channelId = 0; channelId < 3; channelId++)
                    cannyImage.ImgArray[y, x, channelId] = edgesImar[y, x];
        cannyImage.Save("sharpness_sobel_applied");
    }

    [TestMethod]
    public void SharpnessTest()
    {
        var names = new List<string>() 
        { "sharpness_common_g0.png", "sharpness_common_g0p5.png", "sharpness_common_g1.png", "sharpness_common_g2.png",
          "sharpness_common_g3.png", "sharpness_common_g5.png", "sharpness_common_g10.png", "sharpness_common_g25.png" };

        string value = string.Empty;
        foreach (var name in names)
        {
            string imagePath = Path.Combine(Helper.GetAssemblyDir(), "TestData", name);
            var image = new ImageHandler(imagePath);

            var parameters = new StatmParameters(image);
            parameters.SharpnessCalcCannyUpThreshold = 0.7;  // Для тестового хороший: 0.7
            parameters.SharpnessCalcCannyDownThreshold = 0.65;  // Для тестового хороший: 0.65

            var sharpnessCalculator = new SharpnessCalculator(parameters);
            var sharpness = sharpnessCalculator.CalcSharpness();
            value += $"[{sharpness} for {name}]\n";
        }

        Assert.Fail("\n" + value);
    }
}
