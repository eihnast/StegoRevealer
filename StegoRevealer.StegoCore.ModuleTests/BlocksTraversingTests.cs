using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.ImageHandlerLib.Blocks;
using StegoRevealer.StegoCore.StegoMethods;

namespace StegoRevealer.StegoCore.ModuleTests;

[TestClass]
public class BlocksTraversingTests
{
    // Общее для тестов изображение: загружаем 1 раз
    private static string TestImagePath = Path.Combine(Helper.GetAssemblyDir(), "TestData", "FreqBlocksTraversingTest.png");
    private static ImageHandler TestImage = new ImageHandler(TestImagePath);
    private const int BlockSize = 8;

    private const byte Blue0 = 255;
    private const byte Blue1 = 150;
    private const byte Blue2 = 60;
    private const byte Blue3 = 10;

    [TestMethod]
    public void CheckForDirectTraveseHorizontal_ValuesInterator()
    {
        var blocks = new ImageBlocks(new ImageBlocksParameters(TestImage, BlockSize));
        var traversalOptions = GetTraversalOptionsForKzha(TraverseType.Horizontal);

        var expectedColors = new Dictionary<int, byte>()
        {
            { 0, Blue0 }, { 1, Blue1 }, { 2, Blue2 }, { 3, Blue3 },
            { 4, Blue3 }, { 5, Blue1 }, { 6, Blue3 }, { 7, Blue0 },
            { 8, Blue1 }, { 9, Blue2 }, { 10, Blue0 }, { 11, Blue2 }
        };

        // Данный итератор применяется в стегоанализе
        var iterator = BlocksTraverseHelper.GetForLinearAccessOneChannelBlocks(blocks, traversalOptions);
        var iteratedBlocks = new List<byte[,]>();
        foreach (var block in iterator)
            iteratedBlocks.Add(block);

        Assert.AreEqual(12, iteratedBlocks.Count);
        foreach (var expectedColor in expectedColors)
            Assert.AreEqual(expectedColor.Value, iteratedBlocks[expectedColor.Key][0, 0]);
    }

    [TestMethod]
    public void CheckForDirectTraveseVertical_ValuesInterator()
    {
        var blocks = new ImageBlocks(new ImageBlocksParameters(TestImage, BlockSize));
        var traversalOptions = GetTraversalOptionsForKzha(TraverseType.Vertical);

        var expectedColors = new Dictionary<int, byte>()
        {
            { 0, Blue0 }, { 1, Blue3 }, { 2, Blue1 },
            { 3, Blue1 }, { 4, Blue1 }, { 5, Blue2 },
            { 6, Blue2 }, { 7, Blue3 }, { 8, Blue0 },
            { 9, Blue3 }, { 10, Blue0 }, { 11, Blue2 }
        };

        // Данный итератор применяется в стегоанализе
        var iterator = BlocksTraverseHelper.GetForLinearAccessOneChannelBlocks(blocks, traversalOptions);
        var iteratedBlocks = new List<byte[,]>();
        foreach (var block in iterator)
            iteratedBlocks.Add(block);

        Assert.AreEqual(12, iteratedBlocks.Count);
        foreach (var expectedColor in expectedColors)
            Assert.AreEqual(expectedColor.Value, iteratedBlocks[expectedColor.Key][0, 0]);
    }

    [TestMethod]
    public void CheckForDirectTraveseHorizontal_IndexesInterator()
    {
        var blocks = new ImageBlocks(new ImageBlocksParameters(TestImage, BlockSize));
        var traversalOptions = GetTraversalOptionsForKzha(TraverseType.Horizontal);
        const int BlueChannelIndex = (int)ImgChannel.Blue;

        var expectedIndexes = new List<ScPointCoords>()
        { 
            new ScPointCoords(0, 0, BlueChannelIndex), new ScPointCoords(0, 1, BlueChannelIndex), new ScPointCoords(0, 2, BlueChannelIndex),
            new ScPointCoords(0, 3, BlueChannelIndex), new ScPointCoords(1, 0, BlueChannelIndex), new ScPointCoords(1, 1, BlueChannelIndex),
            new ScPointCoords(1, 2, BlueChannelIndex), new ScPointCoords(1, 3, BlueChannelIndex), new ScPointCoords(2, 0, BlueChannelIndex),
            new ScPointCoords(2, 1, BlueChannelIndex), new ScPointCoords(2, 2, BlueChannelIndex), new ScPointCoords(2, 3, BlueChannelIndex)
        };
        var expectedColors = new Dictionary<int, byte>()
        {
            { 0, Blue0 }, { 1, Blue1 }, { 2, Blue2 }, { 3, Blue3 },
            { 4, Blue3 }, { 5, Blue1 }, { 6, Blue3 }, { 7, Blue0 },
            { 8, Blue1 }, { 9, Blue2 }, { 10, Blue0 }, { 11, Blue2 }
        };

        // Данный итератор применяется в стегоанализе
        var iterator = BlocksTraverseHelper.GetForLinearAccessOneChannelBlocksIndexes(blocks, traversalOptions);
        var iteratedBlocksIndexes = new List<ScPointCoords>();
        foreach (var blockIndex in iterator)
            iteratedBlocksIndexes.Add(blockIndex);

        Assert.AreEqual(12, iteratedBlocksIndexes.Count);
        for (int i = 0; i < 12; i++)
            Assert.IsTrue(IsEqual(expectedIndexes[i], iteratedBlocksIndexes[i]));
        for (int i = 0; i < 12; i++)
            Assert.AreEqual(expectedColors[i], BlocksTraverseHelper.GetOneChannelBlockByIndexes(iteratedBlocksIndexes[i], blocks)[0, 0]);
    }

    [TestMethod]
    public void CheckForDirectTraveseVertical_IndexesInterator()
    {
        var blocks = new ImageBlocks(new ImageBlocksParameters(TestImage, BlockSize));
        var traversalOptions = GetTraversalOptionsForKzha(TraverseType.Vertical);
        const int BlueChannelIndex = (int)ImgChannel.Blue;

        var expectedIndexes = new List<ScPointCoords>()
        {
            new ScPointCoords(0, 0, BlueChannelIndex), new ScPointCoords(1, 0, BlueChannelIndex), new ScPointCoords(2, 0, BlueChannelIndex),
            new ScPointCoords(0, 1, BlueChannelIndex), new ScPointCoords(1, 1, BlueChannelIndex), new ScPointCoords(2, 1, BlueChannelIndex),
            new ScPointCoords(0, 2, BlueChannelIndex), new ScPointCoords(1, 2, BlueChannelIndex), new ScPointCoords(2, 2, BlueChannelIndex),
            new ScPointCoords(0, 3, BlueChannelIndex), new ScPointCoords(1, 3, BlueChannelIndex), new ScPointCoords(2, 3, BlueChannelIndex)
        };
        var expectedColors = new Dictionary<int, byte>()
        {
            { 0, Blue0 }, { 1, Blue3 }, { 2, Blue1 },
            { 3, Blue1 }, { 4, Blue1 }, { 5, Blue2 },
            { 6, Blue2 }, { 7, Blue3 }, { 8, Blue0 },
            { 9, Blue3 }, { 10, Blue0 }, { 11, Blue2 }
        };

        // Данный итератор применяется в стегоанализе
        var iterator = BlocksTraverseHelper.GetForLinearAccessOneChannelBlocksIndexes(blocks, traversalOptions);
        var iteratedBlocksIndexes = new List<ScPointCoords>();
        foreach (var blockIndex in iterator)
            iteratedBlocksIndexes.Add(blockIndex);

        Assert.AreEqual(12, iteratedBlocksIndexes.Count);
        for (int i = 0; i < 12; i++)
            Assert.IsTrue(IsEqual(expectedIndexes[i], iteratedBlocksIndexes[i]));
        for (int i = 0; i < 12; i++)
            Assert.AreEqual(expectedColors[i], BlocksTraverseHelper.GetOneChannelBlockByIndexes(iteratedBlocksIndexes[i], blocks)[0, 0]);
    }


    #region Helper

    private BlocksTraverseOptions GetTraversalOptionsForKzha(TraverseType traverseType)
    {
        var traverseOptions = new BlocksTraverseOptions();
        var startBlocks = new StartValues();
        traverseOptions.Channels.Clear();
        foreach (var channel in new UniqueList<ImgChannel>(new List<ImgChannel>() { ImgChannel.Blue }))
        {
            traverseOptions.Channels.Add(channel);
            startBlocks[channel] = 0;
        }
        traverseOptions.StartBlocks = startBlocks;
        traverseOptions.InterlaceChannels = false;
        traverseOptions.TraverseType = traverseType;

        return traverseOptions;
    }

    private bool IsEqual(ScPointCoords first, ScPointCoords second)
    {
        if (first.Y != second.Y || first.X != second.X || first.ChannelId != second.ChannelId)
            return false;
        return true;
    }

    #endregion
}
