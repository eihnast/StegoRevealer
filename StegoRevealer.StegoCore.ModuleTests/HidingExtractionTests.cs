using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.StegoMethods.Lsb;

namespace StegoRevealer.StegoCore.ModuleTests
{
    [TestClass]
    public class HidingExtractionTests
    {
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
            string imagePath = Path.Combine(Helper.GetAssemblyDir(), "TestData", "image1.png");
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
    }
}
