using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Calculators;
using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevealer.StegoCore.ModuleTests;

[TestClass]
public class StatmTests
{
    [TestMethod]
    public void NoiseTest()
    {
        var names = new List<string>() { "noise_common_g0.png", "noise_common_g1.png", "noise_common_g2.png", "noise_common_g4.png", "noise_common_g7.png" };

        var noises = new List<double>();
        foreach (var name in names)
        {
            string imagePath = Path.Combine(Helper.GetAssemblyDir(), "TestData", name);
            var image = new ImageHandler(imagePath);

            var parameters = new StatmParameters(image);

            var noiseCalculator = new NoiseCalculator(parameters);
            var noise = noiseCalculator.CalcNoiseLevel(NoiseCalculator.NoiseCalculationMethod.Method2);
            noises.Add(noise);
        }

        Console.WriteLine("Noise calculation results:");
        for (int i = 0; i < names.Count; i++)
            Console.WriteLine($"{names[i]}: {noises[i]}");

        for (int i = 1; i < names.Count; i++)
            Assert.IsTrue(noises[i] > noises[i - 1], $"Error with {names[i]}. Current '{names[i]}' : {noises[i]}. Previous '{names[i - 1]}': {noises[i - 1]}");
    }

    [TestMethod]
    public void SharpnessTest()
    {
        var names = new List<string>()
        { 
            "sharpness_common_g0.png", "sharpness_common_g0p5.png", "sharpness_common_g1.png", "sharpness_common_g2.png",
            "sharpness_common_g3.png", "sharpness_common_g5.png", "sharpness_common_g10.png", "sharpness_common_g25.png" 
        };

        var sharpnesses = new List<double>();
        foreach (var name in names)
        {
            string imagePath = Path.Combine(Helper.GetAssemblyDir(), "TestData", name);
            var image = new ImageHandler(imagePath);

            var parameters = new StatmParameters(image);
            parameters.SharpnessCalcCannyUpThreshold = 0.7;  // Для тестового хороший: 0.7
            parameters.SharpnessCalcCannyDownThreshold = 0.65;  // Для тестового хороший: 0.65

            var sharpnessCalculator = new SharpnessCalculator(parameters);
            var sharpness = sharpnessCalculator.CalcSharpness();
            sharpnesses.Add(sharpness);
        }

        Console.WriteLine("Sharpness calculation results:");
        for (int i = 0; i < names.Count; i++)
            Console.WriteLine($"{names[i]}: {sharpnesses[i]}");

        for (int i = 1; i < names.Count; i++)
            Assert.IsTrue(sharpnesses[i] < sharpnesses[i - 1], $"Error with {names[i]}. Current '{names[i]}' : {sharpnesses[i]}. Previous '{names[i - 1]}': {sharpnesses[i - 1]}");
    }

    [TestMethod]
    public void BlurTest()
    {
        var names = new List<string>() { "blur_common_g0.png", "blur_common_g0p5.png", "blur_common_g1.png", "blur_common_g5.png", "blur_common_g25.png" };

        var blurs = new List<double>();
        foreach (var name in names)
        {
            string imagePath = Path.Combine(Helper.GetAssemblyDir(), "TestData", name);
            var image = new ImageHandler(imagePath);

            var parameters = new StatmParameters(image);
            parameters.BlurCalcFilterSizeK1 = 5;
            parameters.BlurCalcFilterSizeK2 = 7;

            var blurCalculator = new BlurCalculator(parameters);
            var blur = blurCalculator.CalcBlur();
            blurs.Add(blur);
        }

        Console.WriteLine("Blur calculation results:");
        for (int i = 0; i < names.Count; i++)
            Console.WriteLine($"{names[i]}: {blurs[i]}");

        for (int i = 1; i < names.Count; i++)
            Assert.IsTrue(blurs[i] > blurs[i - 1], $"Error with {names[i]}. Current '{names[i]}' : {blurs[i]}. Previous '{names[i - 1]}': {blurs[i - 1]}");
    }

    [TestMethod]
    public void ContrastTest()
    {
        var names = new List<string>() 
        { 
            "contrast_common_gm50.png", "contrast_common_gm20.png", "contrast_common_g0.png", "contrast_common_g15.png",
            "contrast_common_g40.png", "contrast_common_g65.png", "contrast_common_g100.png"
        };

        var contrasts = new List<double>();
        foreach (var name in names)
        {
            string imagePath = Path.Combine(Helper.GetAssemblyDir(), "TestData", name);
            var image = new ImageHandler(imagePath);

            var parameters = new StatmParameters(image);
            parameters.ContrastCalcWindowCenterSize = 3;

            var contrastCalculator = new ContrastCalculator(parameters);
            var contrast = contrastCalculator.CalcContrast();
            contrasts.Add(contrast);
        }

        Console.WriteLine("Contrast calculation results:");
        for (int i = 0; i < names.Count; i++)
            Console.WriteLine($"{names[i]}: {contrasts[i]}");

        for (int i = 1; i < names.Count; i++)
            Assert.IsTrue(contrasts[i] > contrasts[i - 1], $"Error with {names[i]}. Current '{names[i]}' : {contrasts[i]}. Previous '{names[i - 1]}': {contrasts[i - 1]}");
    }


    #region Helper
    // [TestMethod]
    public void SaveCannyDetection()
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
        cannyImage.Save("sharpness_canny_gpt_applied");
    }

    private void SaveSobelDetection()
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
    #endregion
}
