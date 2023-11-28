using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.StegoMethods.KochZhao;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StegoRevealer.StegoCore.ModuleTests
{
    [TestClass]
    public class CommonTests
    {
        //[TestMethod]
        //public void RsTest()
        //{
        //    string imagePath = Path.Combine(Helper.GetAssemblyDir(), "TestData", "rsHigher100.png");  // Var 45, img9. RandomKz. pre036.
        //    //string imagePath = Path.Combine(Helper.GetAssemblyDir(), "TestData", "pre036.png");
        //    //string imagePath = Path.Combine(Helper.GetAssemblyDir(), "TestData", "rs9953.png");  // Var 42, img7. LinearKZ.
        //    //string imagePath = Path.Combine(Helper.GetAssemblyDir(), "TestData", "rs77_real65.png");  // Var 8, img8.
        //    var image = new ImageHandler(imagePath);

        //    var rsAnalyzer = new RsAnalyser(image);
        //    var result = rsAnalyzer.Analyse(verboseLog: true);
        //    Assert.IsNotNull(result);

        //    var log = string.Empty;
        //    foreach (var logEntry in result.LogRecords)
        //        log += logEntry.Message + "\n";
        //    Assert.IsTrue(result.MessageRelativeVolume is < 1.0 and >= 0.0);
        //}

        //[TestMethod]
        //public void RsTest2()
        //{
        //    string imagePath = Path.Combine(Helper.GetAssemblyDir(), "TestData", "pre036.png");
        //    var image = new ImageHandler(imagePath);

        //    string fullDataPath = Path.Combine(Helper.GetAssemblyDir(), "TestData", "DuneBook.txt");
        //    var allData = File.ReadAllText(fullDataPath);

        //    var kzHider = new KochZhaoHider(image);
        //    kzHider.Params.Threshold = 120;
        //    kzHider.Params.Seed = 52415;
        //    var hidingResult = kzHider.Hide(GetDataByBitLength(allData, 422449, 28200));

        //    Assert.IsNotNull(hidingResult.GetResultPath());
        //}

        //public string GetDataByBitLength(string allData, int startIndex, int bitLength)
        //{
        //    var remainingDataBitLength = StringBitsTools.StringToBitArray(allData[startIndex..]).Length;

        //    // Сдвигаем startIndex влево, если не хватает (условно считаем, что символ = 8 бит, т.е. сдвигаем на возможный максимум символов)
        //    if (remainingDataBitLength < bitLength)
        //        startIndex -= (bitLength - remainingDataBitLength) / 8 - 1;
        //    startIndex = Math.Max(startIndex, 0);

        //    int actualBitLength = 0;
        //    string resultData = string.Empty;

        //    int k = startIndex;
        //    while (actualBitLength < bitLength)
        //    {
        //        actualBitLength += StringBitsTools.StringToBitArray(allData[k].ToString()).Length;
        //        if (actualBitLength <= bitLength)  // Результат может быть меньше заданного bitLength, если символ "не влезает"
        //            resultData += allData[k];
        //        k++;
        //    }

        //    return resultData;
        //}
    }
}
