using MathNet.Numerics;
using Newtonsoft.Json;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.CommonLib.Exceptions;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.ScMath;
using StegoRevealer.StegoCore.StegoMethods;
using StegoRevealer.StegoCore.StegoMethods.KochZhao;
using StegoRevealer.StegoCore.StegoMethods.Lsb;
using System.Collections;
using System.CommandLine;

namespace StegoRevealer.StegoCore.ModuleTests;

[TestClass]
public class HidingExtractionTests
{
    #region LSB

    [TestMethod]
    public void HidingExtractionLsb_DefaultParams()
    {
        string imagePath = Path.Combine(Helper.GetAssemblyDir(), "TestData", "image0.png");
        var lsbHider = new LsbHider(new ImageHandler(imagePath));

        string data = string.Empty;
        for (int i = 0; i < 1000; i++)
            data += $"Data for hiding {i}.\t";

        var resultPath = lsbHider.Hide(data).GetResultPath();
        Assert.IsFalse(string.IsNullOrEmpty(resultPath));

        var lsbExtractor = new LsbExtractor(new ImageHandler(resultPath));
        lsbExtractor.Params.ToExtractBitLength = lsbHider.Params.DataBitLength;
        // Либо нужно отдельно сконвертировать текст и посчитать длину, чтобы не брать из параметров Hider-а

        var extractedData = lsbExtractor.Extract().GetResultData();
        Assert.AreEqual(data, extractedData);
    }

    [TestMethod]
    public void HidingExtractionLsb_WhithFewData()
    {
        string imagePath = Path.Combine(Helper.GetAssemblyDir(), "TestData", "image0a.png");
        var lsbHider = new LsbHider(new ImageHandler(imagePath));

        string data = "Data";

        var resultPath = lsbHider.Hide(data).GetResultPath();
        Assert.IsFalse(string.IsNullOrEmpty(resultPath));

        var lsbExtractor = new LsbExtractor(new ImageHandler(resultPath));

        var extractedData = lsbExtractor.Extract().GetResultData();
        Assert.IsTrue(extractedData?.StartsWith(data), $"Extracted data must starts with '{data}', but was '{extractedData?[..10]}'");
    }

    [TestMethod]
    public void HidingExtractionLsb_Random()
    {
        string imagePath = Path.Combine(Helper.GetAssemblyDir(), "TestData", "image1.png");
        const int seed = 13378;
        var lsbHider = new LsbHider(new ImageHandler(imagePath));
        lsbHider.Params.Seed = seed;

        string data = string.Empty;
        for (int i = 0; i < 1000; i++)
            data += $"Data for hiding {i}.\t";

        var resultPath = lsbHider.Hide(data, "customNameLsb").GetResultPath();
        Assert.IsFalse(string.IsNullOrEmpty(resultPath));

        var lsbExtractor = new LsbExtractor(new ImageHandler(resultPath));
        lsbExtractor.Params.ToExtractBitLength = lsbHider.Params.DataBitLength;
        // Либо нужно отдельно сконвертировать текст и посчитать длину, чтобы не брать из параметров Hider-а
        lsbExtractor.Params.Seed = seed;

        var extractedData = lsbExtractor.Extract().GetResultData();
        Assert.AreEqual(data, extractedData);
    }

    [TestMethod]
    public void HidingExtractionLsb_CustomStartPixels()
    {
        string imagePath = Path.Combine(Helper.GetAssemblyDir(), "TestData", "image2.png");
        var customStartPixels = new StegoMethods.StartValues(
            (ImgChannel.Red, 100), (ImgChannel.Green, 299), (ImgChannel.Blue, 665));
        var lsbHider = new LsbHider(new ImageHandler(imagePath));
        lsbHider.Params.StartPixels = customStartPixels;

        string data = string.Empty;
        for (int i = 0; i < 1000; i++)
            data += $"Data for hiding {i}.\t";

        var resultPath = lsbHider.Hide(data).GetResultPath();
        Assert.IsFalse(string.IsNullOrEmpty(resultPath));

        var lsbExtractor = new LsbExtractor(new ImageHandler(resultPath));
        lsbExtractor.Params.ToExtractBitLength = lsbHider.Params.DataBitLength;
        // Либо нужно отдельно сконвертировать текст и посчитать длину, чтобы не брать из параметров Hider-а
        lsbExtractor.Params.StartPixels = customStartPixels;

        var extractedData = lsbExtractor.Extract().GetResultData();
        Assert.AreEqual(data, extractedData);
    }

    [TestMethod]
    public void HidingExtractionLsb_WithInterlacingChannels()
    {
        string imagePath = Path.Combine(Helper.GetAssemblyDir(), "TestData", "image3.png");
        var lsbHider = new LsbHider(new ImageHandler(imagePath));
        lsbHider.Params.InterlaceChannels = true;

        string data = string.Empty;
        for (int i = 0; i < 1000; i++)
            data += $"Data for hiding {i}.\t";

        var resultPath = lsbHider.Hide(data).GetResultPath();
        Assert.IsFalse(string.IsNullOrEmpty(resultPath));

        var lsbExtractor = new LsbExtractor(new ImageHandler(resultPath));
        lsbExtractor.Params.ToExtractBitLength = lsbHider.Params.DataBitLength;
        // Либо нужно отдельно сконвертировать текст и посчитать длину, чтобы не брать из параметров Hider-а
        lsbExtractor.Params.InterlaceChannels = true;

        var extractedData = lsbExtractor.Extract().GetResultData();
        Assert.AreEqual(data, extractedData);
    }

    [TestMethod]
    public void HidingExtractionLsb_WithVerticalTraversing()
    {
        string imagePath = Path.Combine(Helper.GetAssemblyDir(), "TestData", "image4.png");
        var lsbHider = new LsbHider(new ImageHandler(imagePath));
        lsbHider.Params.TraverseType = CommonLib.TraverseType.Vertical;

        string data = string.Empty;
        for (int i = 0; i < 1000; i++)
            data += $"Data for hiding {i}.\t";

        var resultPath = lsbHider.Hide(data).GetResultPath();
        Assert.IsFalse(string.IsNullOrEmpty(resultPath));

        var lsbExtractor = new LsbExtractor(new ImageHandler(resultPath));
        lsbExtractor.Params.ToExtractBitLength = lsbHider.Params.DataBitLength;
        // Либо нужно отдельно сконвертировать текст и посчитать длину, чтобы не брать из параметров Hider-а
        lsbExtractor.Params.TraverseType = CommonLib.TraverseType.Vertical;

        var extractedData = lsbExtractor.Extract().GetResultData();
        Assert.AreEqual(data, extractedData);
    }

    [TestMethod]
    public void HidingExtractionLsb_RandomVerticalNotInterlace()
    {
        string imagePath = Path.Combine(Helper.GetAssemblyDir(), "TestData", "image5.png");
        const int seed = 155760;
        var lsbHider = new LsbHider(new ImageHandler(imagePath));
        lsbHider.Params.Seed = seed;
        lsbHider.Params.InterlaceChannels = false;
        lsbHider.Params.TraverseType = CommonLib.TraverseType.Vertical;

        string data = string.Empty;
        for (int i = 0; i < 1000; i++)
            data += $"Data for hiding {i}.\t";

        var resultPath = lsbHider.Hide(data).GetResultPath();
        Assert.IsFalse(string.IsNullOrEmpty(resultPath));

        var lsbExtractor = new LsbExtractor(new ImageHandler(resultPath));
        lsbExtractor.Params.ToExtractBitLength = lsbHider.Params.DataBitLength;
        // Либо нужно отдельно сконвертировать текст и посчитать длину, чтобы не брать из параметров Hider-а
        lsbExtractor.Params.Seed = seed;
        lsbExtractor.Params.InterlaceChannels = false;
        lsbExtractor.Params.TraverseType = CommonLib.TraverseType.Vertical;

        var extractedData = lsbExtractor.Extract().GetResultData();
        Assert.AreEqual(data, extractedData);
    }

    [TestMethod]
    public void HidingExtractionLsb_ThreeLsbs()
    {
        string imagePath = Path.Combine(Helper.GetAssemblyDir(), "TestData", "image6.png");
        var lsbHider = new LsbHider(new ImageHandler(imagePath));
        lsbHider.Params.LsbNum = 3;

        string data = string.Empty;
        for (int i = 0; i < 1000; i++)
            data += $"Data for hiding {i}.\t";

        var resultPath = lsbHider.Hide(data).GetResultPath();
        Assert.IsFalse(string.IsNullOrEmpty(resultPath));

        var lsbExtractor = new LsbExtractor(new ImageHandler(resultPath));
        lsbExtractor.Params.ToExtractBitLength = lsbHider.Params.DataBitLength;
        // Либо нужно отдельно сконвертировать текст и посчитать длину, чтобы не брать из параметров Hider-а
        lsbExtractor.Params.LsbNum = 3;

        var extractedData = lsbExtractor.Extract().GetResultData();
        Assert.AreEqual(data, extractedData);
    }

    [TestMethod]
    public void HidingExtractionLsb_ManyCustomParams()
    {
        string imagePath = Path.Combine(Helper.GetAssemblyDir(), "TestData", "image7.png");
        var customStartPixels = new StegoMethods.StartValues(
            (ImgChannel.Red, 100), (ImgChannel.Green, 299), (ImgChannel.Blue, 665));
        var lsbHider = new LsbHider(new ImageHandler(imagePath));
        lsbHider.Params.StartPixels = customStartPixels;
        lsbHider.Params.TraverseType = CommonLib.TraverseType.Vertical;
        lsbHider.Params.InterlaceChannels = false;
        lsbHider.Params.LsbNum = 2;

        string data = string.Empty;
        for (int i = 0; i < 1000; i++)
            data += $"Data for hiding {i}.\t";

        var resultPath = lsbHider.Hide(data).GetResultPath();
        Assert.IsFalse(string.IsNullOrEmpty(resultPath));

        var lsbExtractor = new LsbExtractor(new ImageHandler(resultPath));
        lsbExtractor.Params.ToExtractBitLength = lsbHider.Params.DataBitLength;
        // Либо нужно отдельно сконвертировать текст и посчитать длину, чтобы не брать из параметров Hider-а
        lsbExtractor.Params.StartPixels = customStartPixels;
        lsbExtractor.Params.TraverseType = CommonLib.TraverseType.Vertical;
        lsbExtractor.Params.InterlaceChannels = false;
        lsbExtractor.Params.LsbNum = 2;

        var extractedData = lsbExtractor.Extract().GetResultData();
        Assert.AreEqual(data, extractedData);
    }

    #endregion

    #region Koch-Zhao

    [TestMethod]
    public void KochZhaoHidingExtractionCommonTest()
    {
        var imagePath = Path.Combine(Helper.GetAssemblyDir(), "TestData", "imgForKz1.png");
        var image = new ImageHandler(imagePath);
        var kzHider = new KochZhaoHider(image);

        kzHider.Params.Threshold = 120;
        kzHider.Params.TraverseType = TraverseType.Horizontal;

        string data = "Данные для скрытия по методу Коха-Жао. Горизонтальный обход. Порог = 120.";
        var hidingResult = kzHider.Hide(data, "customNameKzh.png");

        var newImage = new ImageHandler(hidingResult.GetResultPath() ?? throw new OperationException("hidingResult.Path is null"));

        var kzExtractor = new KochZhaoExtractor(newImage);
        kzExtractor.Params.Threshold = 20;

        var extractionResult = kzExtractor.Extract();
        var extractedData = extractionResult.GetResultData();
        Assert.IsTrue(extractedData?.StartsWith(data), $"extractedData = {extractedData}");

        var kzAnalayser = new KzhaAnalyser(newImage);
        var saResult = kzAnalayser.Analyse();
        string str = string.Empty;
        foreach (var logEntry in saResult.LogRecords)
            str += logEntry.ToString() + "\n";

        Assert.IsTrue(saResult.ExtractedData?.StartsWith(data), str + $"data = {saResult.ExtractedData}");
    }

    [TestMethod]
    public void KochZhaoHidingExtractionLowThresholdTest()
    {
        int threshold = 15;

        var imagePath = Path.Combine(Helper.GetAssemblyDir(), "TestData", "imgForKz2.png");
        var image = new ImageHandler(imagePath);
        var kzHider = new KochZhaoHider(image);

        kzHider.Params.Threshold = threshold;

        string data = $"Данные для скрытия по методу Коха-Жао. Горизонтальный обход. Порог = {threshold}.";
        var hidingResult = kzHider.Hide(data, "customNameKzh2.png");

        var newImage = new ImageHandler(hidingResult.GetResultPath() ?? throw new OperationException("hidingResult.Path is null"));

        var kzExtractor = new KochZhaoExtractor(newImage);

        var extractionResult = kzExtractor.Extract();
        var extractedData = extractionResult.GetResultData();
        Assert.IsTrue(extractedData?.StartsWith(data), $"extractedData = {extractedData}");
    }

    [TestMethod]
    public void KochZhaoHidingExtractionOneBitHidingTest()
    {
        var coeffs = HidingCoefficients.Coeff45;
        byte[,] block = new byte[8, 8]
        {
                { 157, 154, 153, 154, 157, 157, 155, 153 },
                { 159, 157, 154, 152, 153, 155, 157, 158 },
                { 159, 159, 158, 157, 155, 155, 158, 159 },
                { 159, 160, 162, 162, 160, 158, 157, 157 },
                { 159, 161, 164, 164, 160, 158, 157, 157 },
                { 161, 161, 159, 159, 157, 157, 158, 159 },
                { 161, 159, 158, 157, 158, 159, 161, 162 },
                { 159, 159, 159, 160, 164, 165, 165, 164 },
        };
        Console.WriteLine($"Коэффициенты: {coeffs}");


        // Проверка корректности обратного ДКП
        var dct = FrequencyViewTools.DctBlock(block);
        var idct = FrequencyViewTools.IDctBlock(dct);
        Assert.AreEqual(JsonConvert.SerializeObject(block), JsonConvert.SerializeObject(FrequencyViewTools.NormalizeBlock(idct)));


        // Проверка встраиваний бита
        var dctForHide = FrequencyViewTools.DctBlock(block);
        int threshold = 25;

        var coefValues = FrequencyViewTools.GetBlockCoeffs(dctForHide, coeffs);  // Значения коэффициентов
        Console.WriteLine($"Значения коэффициентов оригинального блока ДКП: {coefValues}");

        var difference = MathMethods.GetModulesDiff(coefValues);  // Разница коэффициентов
        Console.WriteLine($"Разница значений коэффициентов оригинального блока ДКП: {difference}");

        var newCoeffValues = FrequencyViewTools.GetModifiedCoeffsOriginal(coefValues, threshold, false);
        Console.WriteLine($"Новые значения коэффициентов коэффициентов оригинального блока ДКП: {newCoeffValues}");

        (int coefInd1, int coefInd2) = coeffs.AsTuple();
        dctForHide[coefInd1, coefInd2] = newCoeffValues.val1;
        dctForHide[coefInd2, coefInd1] = newCoeffValues.val2;

        var checkCoefValues = FrequencyViewTools.GetBlockCoeffs(dctForHide, coeffs);  // Значения коэффициентов
        Console.WriteLine($"Значения коэффициентов изменённого блока ДКП: {checkCoefValues}");

        var newDifference = MathMethods.GetModulesDiff(coefValues);  // Разница коэффициентов
        Console.WriteLine($"Новая величина разницы значений коэффициентов изменённого блока ДКП: {newDifference}");

        Assert.IsTrue(newDifference < threshold);

        var hidedIdct = FrequencyViewTools.IDctBlock(dctForHide);
        var hidedBlock = FrequencyViewTools.NormalizeBlock(hidedIdct);

        var hidedBlockDct = FrequencyViewTools.DctBlock(hidedBlock);

        var extractionCoefValues = FrequencyViewTools.GetBlockCoeffs(hidedBlockDct, coeffs);  // Значения коэффициентов
        Console.WriteLine($"Значения коэффициентов при извлечении измённого блока ДКП: {extractionCoefValues}");

        var extractionDifference = MathMethods.GetModulesDiff(extractionCoefValues);  // Разница коэффициентов
        Console.WriteLine($"Разница значений коэффициентов при извлечении измённого блока ДКП: {extractionDifference}");

        bool? bit = null;  // Извлечение бита может быть неудачным
        if (difference > 0)
            bit = true;
        else if (difference < 0)
            bit = false;

        Assert.IsTrue(bit == false);
    }

    //[TestMethod]
    //public void KochZhaoHidingExtractionCommonTest2()
    //{
    //    var str = "\n";

    //    byte[,] block = new byte[8, 8]
    //    {
    //        { 157, 154, 153, 154, 157, 157, 155, 153 },
    //        { 159, 157, 154, 152, 153, 155, 157, 158 },
    //        { 159, 159, 158, 157, 155, 155, 158, 159 },
    //        { 159, 160, 162, 162, 160, 158, 157, 157 },
    //        { 159, 161, 164, 164, 160, 158, 157, 157 },
    //        { 161, 161, 159, 159, 157, 157, 158, 159 },
    //        { 161, 159, 158, 157, 158, 159, 161, 162 },
    //        { 159, 159, 159, 160, 164, 165, 165, 164 },
    //    };

    //    for (int i = 0; i < 8; i++)
    //    {
    //        for (int j = 0; j < 8; j++)
    //        {
    //            str += string.Format("{0,4:000} ", block[i, j]);
    //            //str += $"{block[i, j]:000} ";
    //        }
    //        str += "\n";
    //    }
    //    str += "\n";


    //    //var dctBlock = MathMethods.Dct(block);
    //    var dctBlock = FrequencyViewTools.DctBlock(block, 8);

    //    for (int i = 0; i < 8; i++)
    //    {
    //        for (int j = 0; j < 8; j++)
    //        {
    //            str += string.Format("{0,4:000} ", dctBlock[i, j]);
    //            //str += $"{dctBlock[i, j]:000} ";
    //        }
    //        str += "\n";
    //    }
    //    str += "\n";


    //    var coefValues = FrequencyViewTools.GetBlockCoeffs(dctBlock, HidingCoefficients.Coeff45);  // Значения коэффициентов
    //    var difference = MathMethods.GetModulesDiff(coefValues);  // Разница коэффициентов
    //    var newCoeffValues = coefValues;

    //    str += $"old: {coefValues:000}, {difference:000}\n";

    //    // Получение модифицированных значений коэффициентов
    //    newCoeffValues = FrequencyViewTools.GetModifiedCoeffs(newCoeffValues, -120, false);
    //    str += $"new: {newCoeffValues:000}\n\n";

    //    // Изменение значений на новые в блоке
    //    (int coefInd1, int coefInd2) = HidingCoefficients.Coeff45.AsTuple();
    //    str += $"old: {dctBlock[coefInd1, coefInd2]}, {dctBlock[coefInd2, coefInd1]}\n";
    //    dctBlock[coefInd1, coefInd2] = newCoeffValues.val1;
    //    dctBlock[coefInd2, coefInd1] = newCoeffValues.val2;
    //    str += $"new: {dctBlock[coefInd1, coefInd2]}, {dctBlock[coefInd2, coefInd1]}\n\n";


    //    for (int i = 0; i < 8; i++)
    //    {
    //        for (int j = 0; j < 8; j++)
    //        {
    //            str += string.Format("{0,4:000} ", dctBlock[i, j]);
    //            //str += $"{dctBlock[i, j]:000} ";
    //        }
    //        str += "\n";
    //    }
    //    str += "\n";

    //    //var idctBlock = MathMethods.Idct(dctBlock);
    //    var idctBlock = FrequencyViewTools.NormalizeBlock(FrequencyViewTools.IDctBlock(dctBlock));

    //    for (int i = 0; i < 8; i++)
    //    {
    //        for (int j = 0; j < 8; j++)
    //        {
    //            str += string.Format("{0,4:000} ", idctBlock[i, j]);
    //            //str += $"{idctBlock[i, j]:000} ";
    //        }
    //        str += "\n";
    //    }
    //    str += "\n";

    //    //var newDct = MathMethods.Dct(idctBlock);
    //    var newDct = FrequencyViewTools.DctBlock(idctBlock);
    //    for (int i = 0; i < 8; i++)
    //    {
    //        for (int j = 0; j < 8; j++)
    //        {
    //            str += string.Format("{0,4:000} ", newDct[i, j]);
    //            //str += $"{idctBlock[i, j]:000} ";
    //        }
    //        str += "\n";
    //    }
    //    str += "\n";



    //    //Assert.Fail(str);
    //}

    #endregion


    [TestMethod]
    public void HidingExtractionBoth_DefaultParamsBigImage()
    {
        string imagePath = Path.Combine(Helper.GetAssemblyDir(), "TestData", "imageBig.png");

        // Скрываем по НЗБ
        var lsbHandler = new ImageHandler(imagePath);
        var lsbHider = new LsbHider(lsbHandler);

        string lsbData = string.Empty;
        for (int i = 0; i < 1000; i++)
            lsbData += $"Data for hiding {i}.\t";

        var lsbResultPath = lsbHider.Hide(lsbData).GetResultPath();
        lsbHandler.CloseHandler();
        Assert.IsFalse(string.IsNullOrEmpty(lsbResultPath));

        // Извлекаем и проверяем извлечённое по НЗБ
        var lsbHidedHandler = new ImageHandler(lsbResultPath);
        var lsbExtractor = new LsbExtractor(lsbHidedHandler);
        lsbExtractor.Params.ToExtractBitLength = lsbHider.Params.DataBitLength;

        var lsbExtractedData = lsbExtractor.Extract().GetResultData();
        lsbHidedHandler.CloseHandler();
        Assert.AreEqual(lsbData, lsbExtractedData);

        // Скрываем по Коха-Жао
        var image = new ImageHandler(imagePath);
        var kzHider = new KochZhaoHider(image);
        kzHider.Params.Threshold = 120;
        kzHider.Params.TraverseType = TraverseType.Horizontal;

        string kzhData = "Данные для скрытия по методу Коха-Жао. Горизонтальный обход. Порог = 120.";
        var kzhResultPath = kzHider.Hide(kzhData).GetResultPath();
        Assert.IsFalse(string.IsNullOrEmpty(kzhResultPath));

        // Извлекаем и проверяем извлечённое по Коха-Жао
        var kzhHidedHandler = new ImageHandler(kzhResultPath);

        var kzExtractor = new KochZhaoExtractor(kzhHidedHandler);
        kzExtractor.Params.Threshold = 20;

        var kzhExtractedData = kzExtractor.Extract().GetResultData();
        Assert.IsTrue(kzhExtractedData?.StartsWith(kzhData), $"extractedData = {kzhExtractedData}");
    }
}
