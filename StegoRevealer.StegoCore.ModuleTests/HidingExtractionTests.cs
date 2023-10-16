using Accord.Math;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.ScMath;
using StegoRevealer.StegoCore.StegoMethods;
using StegoRevealer.StegoCore.StegoMethods.KochZhao;
using StegoRevealer.StegoCore.StegoMethods.Lsb;
using static System.Net.Mime.MediaTypeNames;

namespace StegoRevealer.StegoCore.ModuleTests
{
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
            Assert.AreEqual(extractedData, data);
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

            var resultPath = lsbHider.Hide(data).GetResultPath();
            Assert.IsFalse(string.IsNullOrEmpty(resultPath));

            var lsbExtractor = new LsbExtractor(new ImageHandler(resultPath));
            lsbExtractor.Params.ToExtractBitLength = lsbHider.Params.DataBitLength;
            // Либо нужно отдельно сконвертировать текст и посчитать длину, чтобы не брать из параметров Hider-а
            lsbExtractor.Params.Seed = seed;

            var extractedData = lsbExtractor.Extract().GetResultData();
            Assert.AreEqual(extractedData, data);
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
            Assert.AreEqual(extractedData, data);
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
            Assert.AreEqual(extractedData, data);
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
            Assert.AreEqual(extractedData, data);
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
            Assert.AreEqual(extractedData, data);
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
            Assert.AreEqual(extractedData, data);
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
            Assert.AreEqual(extractedData, data);
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
            var hidingResult = kzHider.Hide(data);

            var newImage = new ImageHandler(hidingResult.GetResultPath() ?? throw new Exception("hidingResult.Path is null"));

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

        [TestMethod]
        public void CheckRsMethod()
        {
            var imagePath = Path.Combine(Helper.GetAssemblyDir(), "TestData", "DangerousRsImage.png");
            var image = new ImageHandler(imagePath);

            var rsAnalyse = new RsAnalyser(image);
            var saResult = rsAnalyse.Analyse();

            Assert.Fail($"{saResult.MessageRelativeVolume}");
        }

        #endregion
    }
}
