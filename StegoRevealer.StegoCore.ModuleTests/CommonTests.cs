using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.CommonLib.Entities;
using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevealer.StegoCore.ModuleTests;

[TestClass]
public class CommonTests
{
    [TestMethod]
    public void IntervalWithNeighbourhood_AllLeaksTest()
    {
        string imagePath = Path.Combine(Helper.GetAssemblyDir(), "TestData", "PixelsTestImage1.png");
        var image = new ImageHandler(imagePath);
        int rowId = 4;

        var intervalsInfo = new List<ImageHorizontalIntervalInfo>()
        {
            new ImageHorizontalIntervalInfo() { RowId = rowId, ImgChannel = ImgChannel.Red, IntervalStartId = 0, IntervalEndId = 99 },
            new ImageHorizontalIntervalInfo() { RowId = rowId, ImgChannel = ImgChannel.Green, IntervalStartId = 0, IntervalEndId = 99 },
            new ImageHorizontalIntervalInfo() { RowId = rowId, ImgChannel = ImgChannel.Blue, IntervalStartId = 0, IntervalEndId = 99 }
        };

        var startResults = new List<byte[]>()
        {
            // 0
            new byte[] { 255, 0, 0, 255, 0, 0 },
            new byte[] { 0, 0, 255, 255, 0, 0 },
            new byte[] { 0, 255, 0, 255, 0, 0 },
            // 1
            new byte[] { 0, 255, 0, 0, 255, 0, 0 },
            new byte[] { 0, 0, 0, 255, 255, 0, 0 },
            new byte[] { 255, 0, 255, 0, 255, 0, 0 },
            // 2
            new byte[] { 0, 0, 255, 0, 0, 255, 0, 0 },
            new byte[] { 255, 0, 0, 0, 255, 255, 0, 0 },
            new byte[] { 0, 255, 0, 255, 0, 255, 0, 0 },
            // 3
            new byte[] { 255, 0, 0, 255, 0, 0, 255, 0, 0 },
            new byte[] { 255, 255, 0, 0, 0, 255, 255, 0, 0 },
            new byte[] { 255, 0, 255, 0, 255, 0, 255, 0, 0 },
        };
        var endResults = new List<byte[]>()
        {
            // 0
            new byte[] { 0, 0, 255, 0, 255, 0 },
            new byte[] { 0, 0, 0, 0, 255, 255 },
            new byte[] { 0, 0, 0, 255, 255, 0 },
            // 1
            new byte[] { 0, 0, 255, 0, 255, 0, 255 },
            new byte[] { 0, 0, 0, 0, 255, 255, 255 },
            new byte[] { 0, 0, 0, 255, 255, 0, 255 },
            // 2
            new byte[] { 0, 0, 255, 0, 255, 0, 255, 0 },
            new byte[] { 0, 0, 0, 0, 255, 255, 255, 0 },
            new byte[] { 0, 0, 0, 255, 255, 0, 255, 255 },
            // 3
            new byte[] { 0, 0, 255, 0, 255, 0, 255, 0, 255 },
            new byte[] { 0, 0, 0, 0, 255, 255, 255, 0, 0 },
            new byte[] { 0, 0, 0, 255, 255, 0, 255, 255, 0 },
        };

        int resultsIndex = 0;
        for (int neighbourhoodLength = 0; neighbourhoodLength <= 3; neighbourhoodLength++)
        {
            foreach (var interval in intervalsInfo)
            {
                var result = PixelsTools.GetIntervalWithNeighbourhood(image, interval, neighbourhoodLength);
                IntervalCheck(result, startResults[resultsIndex], endResults[resultsIndex], neighbourhoodLength, interval);
                resultsIndex++;
            }
        }
    }

    [TestMethod]
    public void IntervalWithNeighbourhood_CustomLeaksTest()
    {
        string imagePath = Path.Combine(Helper.GetAssemblyDir(), "TestData", "PixelsTestImage1.png");
        var image = new ImageHandler(imagePath);
        int rowId = 4;

        var interval1 = new ImageHorizontalIntervalInfo() { RowId = rowId, ImgChannel = ImgChannel.Red, IntervalStartId = 0, IntervalEndId = 9 };
        var startResults1 = new List<byte[]>()
        {
            new byte[] { 255, 0, 0, 255, 0, 0 },
            new byte[] { 0, 255, 0, 0, 255, 0, 0 },
            new byte[] { 0, 0, 255, 0, 0, 255, 0, 0 },
            new byte[] { 255, 0, 0, 255, 0, 0, 255, 0, 0 },
        };
        var endResults1 = new List<byte[]>()
        {
            new byte[] { 0, 0, 0, 0, 0 },
            new byte[] { 0, 0, 0, 0, 0, 255 },
            new byte[] { 0, 0, 0, 0, 0, 255, 255 },
            new byte[] { 0, 0, 0, 0, 0, 255, 255, 0 },
        };

        var interval2 = new ImageHorizontalIntervalInfo() { RowId = rowId, ImgChannel = ImgChannel.Blue, IntervalStartId = 0, IntervalEndId = 11 };
        var startResults2 = new List<byte[]>()
        {
            new byte[] { 0, 255, 0, 255, 0, 0 },
            new byte[] { 255, 0, 255, 0, 255, 0, 0 },
            new byte[] { 0, 255, 0, 255, 0, 255, 0, 0 },
            new byte[] { 255, 0, 255, 0, 255, 0, 255, 0, 0 },
        };
        var endResults2 = new List<byte[]>()
        {
            new byte[] { 0, 0, 0, 0, 255 },
            new byte[] { 0, 0, 0, 0, 255, 0 },
            new byte[] { 0, 0, 0, 0, 255, 0, 255 },
            new byte[] { 0, 0, 0, 0, 255, 0, 255, 0 },
        };

        var interval3 = new ImageHorizontalIntervalInfo() { RowId = rowId, ImgChannel = ImgChannel.Green, IntervalStartId = 89, IntervalEndId = 99 };
        var startResults3 = new List<byte[]>()
        {
            new byte[] { 0, 0, 0 },
            new byte[] { 0, 0, 0, 0 },
            new byte[] { 255, 0, 0, 0, 0 },
            new byte[] { 0, 255, 0, 0, 0, 0 },
        };
        var endResults3 = new List<byte[]>()
        {
            new byte[] { 0, 0, 0, 0, 255, 255 },
            new byte[] { 0, 0, 0, 0, 255, 255, 255 },
            new byte[] { 0, 0, 0, 0, 255, 255, 255, 0 },
            new byte[] { 0, 0, 0, 0, 255, 255, 255, 0, 0 },
        };

        var interval4 = new ImageHorizontalIntervalInfo() { RowId = rowId, ImgChannel = ImgChannel.Red, IntervalStartId = 88, IntervalEndId = 99 };
        var startResults4 = new List<byte[]>()
        {
            new byte[] { 0, 0, 0 },
            new byte[] { 0, 0, 0, 0 },
            new byte[] { 255, 0, 0, 0, 0 },
            new byte[] { 255, 255, 0, 0, 0, 0 },
        };
        var endResults4 = new List<byte[]>()
        {
            new byte[] { 0, 0, 255, 0, 255, 0 },
            new byte[] { 0, 0, 255, 0, 255, 0, 255 },
            new byte[] { 0, 0, 255, 0, 255, 0, 255, 0 },
            new byte[] { 0, 0, 255, 0, 255, 0, 255, 0, 255 },
        };

        var interval5 = new ImageHorizontalIntervalInfo() { RowId = rowId, ImgChannel = ImgChannel.Blue, IntervalStartId = 14, IntervalEndId = 84 };
        var startResults5 = new List<byte[]>()
        {
            new byte[] { 0, 0, 0 },
            new byte[] { 255, 0, 0, 0 },
            new byte[] { 0, 255, 0, 0, 0 },
            new byte[] { 255, 0, 255, 0, 0, 0 },
        };
        var endResults5 = new List<byte[]>()
        {
            new byte[] { 0, 0, 0 },
            new byte[] { 0, 0, 0, 255 },
            new byte[] { 0, 0, 0, 255, 0 },
            new byte[] { 0, 0, 0, 255, 0, 0 },
        };

        var interval6 = new ImageHorizontalIntervalInfo() { RowId = rowId, ImgChannel = ImgChannel.Green, IntervalStartId = 13, IntervalEndId = 85 };
        var startResults6 = new List<byte[]>()
        {
            new byte[] { 0, 0, 0 },
            new byte[] { 255, 0, 0, 0 },
            new byte[] { 255, 255, 0, 0, 0 },
            new byte[] { 0, 255, 255, 0, 0, 0 },
        };
        var endResults6 = new List<byte[]>()
        {
            new byte[] { 0, 0, 255 },
            new byte[] { 0, 0, 255, 0 },
            new byte[] { 0, 0, 255, 0, 255 },
            new byte[] { 0, 0, 255, 0, 255, 0 },
        };

        int resultsIndex = 0;
        for (int neighbourhoodLength = 0; neighbourhoodLength <= 3; neighbourhoodLength++)
        {
            Console.WriteLine($"Checking neighbourhoodLength = {neighbourhoodLength}, interval1");
            var result = PixelsTools.GetIntervalWithNeighbourhood(image, interval1, neighbourhoodLength);
            IntervalCheck(result, startResults1[resultsIndex], endResults1[resultsIndex], neighbourhoodLength, interval1);

            Console.WriteLine($"Checking neighbourhoodLength = {neighbourhoodLength}, interval2");
            result = PixelsTools.GetIntervalWithNeighbourhood(image, interval2, neighbourhoodLength);
            IntervalCheck(result, startResults2[resultsIndex], endResults2[resultsIndex], neighbourhoodLength, interval2);

            Console.WriteLine($"Checking neighbourhoodLength = {neighbourhoodLength}, interval3");
            result = PixelsTools.GetIntervalWithNeighbourhood(image, interval3, neighbourhoodLength);
            IntervalCheck(result, startResults3[resultsIndex], endResults3[resultsIndex], neighbourhoodLength, interval3);

            Console.WriteLine($"Checking neighbourhoodLength = {neighbourhoodLength}, interval4");
            result = PixelsTools.GetIntervalWithNeighbourhood(image, interval4, neighbourhoodLength);
            IntervalCheck(result, startResults4[resultsIndex], endResults4[resultsIndex], neighbourhoodLength, interval4);

            Console.WriteLine($"Checking neighbourhoodLength = {neighbourhoodLength}, interval5");
            result = PixelsTools.GetIntervalWithNeighbourhood(image, interval5, neighbourhoodLength);
            IntervalCheck(result, startResults5[resultsIndex], endResults5[resultsIndex], neighbourhoodLength, interval5);

            Console.WriteLine($"Checking neighbourhoodLength = {neighbourhoodLength}, interval6");
            result = PixelsTools.GetIntervalWithNeighbourhood(image, interval6, neighbourhoodLength);
            IntervalCheck(result, startResults6[resultsIndex], endResults6[resultsIndex], neighbourhoodLength, interval6);
            resultsIndex++;
        }
    }


    private void IntervalCheck(byte[] result, byte[] startResult, byte[] endResult, int neighbourhoodLength, ImageHorizontalIntervalInfo interval)
    {
        for (int i = 0; i < startResult.Length; i++)
            Assert.AreEqual(startResult[i], result[i],
                $"NeighbourhoodLength = {neighbourhoodLength}, channel = {interval.ImgChannel}. " +
                $"Expected start: '{string.Join(",", startResult)}', but actual start: '{string.Join(",", result[0..startResult.Length])}'");
        for (int i = 0; i < endResult.Length; i++)
            Assert.AreEqual(endResult[i], result[result.Length - endResult.Length + i],
                $"NeighbourhoodLength = {neighbourhoodLength}, channel = {interval.ImgChannel}. " +
                $"Expected end: '{string.Join(",", endResult)}', but actual end: '{string.Join(",", result[(result.Length - endResult.Length)..])}'");

    }
}
