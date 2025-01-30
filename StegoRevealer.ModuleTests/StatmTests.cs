using MathNet.Numerics;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Calculators;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Entities;
using StegoRevealer.StegoCore.ImageHandlerLib;
using System.Xml.Linq;

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

    [TestMethod]
    public void EntropyTest()
    {
        // var names = new List<string>() { "entropy1.png", "entropy2.png", "entropy3.png", "entropy4.png", "entropy5.png", "entropy6.png" };
        var names = new List<string>() { "entropyNew_1.png", "entropyNew_2.png", "entropyNew_3.png", "entropyNew_4.png" };

        var entropies = new List<EntropyData>();
        foreach (var name in names)
        {
            string imagePath = Path.Combine(Helper.GetAssemblyDir(), "TestData", name);
            var image = new ImageHandler(imagePath);

            var parameters = new StatmParameters(image);
            parameters.EntropyMethods = EntropyMethods.All;

            var entropyCalculator = new EntropyCalculator(parameters);
            var entropy = entropyCalculator.CalcEntropy();
            entropies.Add(entropy);
        }

        Console.WriteLine("Entropy calculation results:");
        for (int i = 0; i < names.Count; i++)
            Console.WriteLine($"{names[i]}: Tsallis = {entropies[i].Tsallis:0.0000}; Vaida = {entropies[i].Vaida:0.0000}; " +
                $"Shennon = {entropies[i].Shennon:0.0000}; Renyi = {entropies[i].Renyi:0.0000}; Havard = {entropies[i].Havard:0.0000};");

        for (int i = 1; i < names.Count; i++)
        {
            Assert.IsTrue(entropies[i].Tsallis >= entropies[i - 1].Tsallis, $"Error with {names[i]}. Current '{names[i]}' : {entropies[i].Tsallis}. Previous '{names[i - 1]}': {entropies[i - 1].Tsallis}");
            Assert.IsTrue(entropies[i].Vaida >= entropies[i - 1].Vaida, $"Error with {names[i]}. Current '{names[i]}' : {entropies[i].Vaida}. Previous '{names[i - 1]}': {entropies[i - 1].Vaida}");
            Assert.IsTrue(entropies[i].Shennon >= entropies[i - 1].Shennon, $"Error with {names[i]}. Current '{names[i]}' : {entropies[i].Shennon}. Previous '{names[i - 1]}': {entropies[i - 1].Shennon}");
            Assert.IsTrue(entropies[i].Renyi >= entropies[i - 1].Renyi, $"Error with {names[i]}. Current '{names[i]}' : {entropies[i].Renyi}. Previous '{names[i - 1]}': {entropies[i - 1].Renyi}");
            Assert.IsTrue(entropies[i].Havard >= entropies[i - 1].Havard, $"Error with {names[i]}. Current '{names[i]}' : {entropies[i].Havard}. Previous '{names[i - 1]}': {entropies[i - 1].Havard}");
        }
    }

    [TestMethod]
    public void ShennonSemiEqualsRenyi()
    {
        string path = Path.Combine(Helper.GetAssemblyDir(), "TestData", "ShennonRenyiTest.png");
        var image = new ImageHandler(path);
        var parameters = new StatmParameters(image);
        parameters.EntropyCalcSensitivity = 1.0000001;
        var entropyCalculator = new EntropyCalculator(parameters);
        var entropy = entropyCalculator.CalcEntropy();
        Assert.AreEqual(entropy.Shennon.Round(6), entropy.Renyi.Round(6));
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
        cannyImage.SaveNear("sharpness_canny_gpt_applied");
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
        cannyImage.SaveNear("sharpness_sobel_applied");
    }
    #endregion
}
