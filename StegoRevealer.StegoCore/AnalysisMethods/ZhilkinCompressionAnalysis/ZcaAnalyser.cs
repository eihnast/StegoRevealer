using System.Diagnostics;
using System.IO.Compression;
using SkiaSharp;
using SharpCompress.Compressors.BZip2;
using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.ImageHandlerLib.Blocks;
namespace StegoRevealer.StegoCore.AnalysisMethods.ZhilkinCompressionAnalysis;

public class ZcaAnalyser
{
    private const string MethodName = "ZCA (Zhilkin Compression Analysis)";

    private static readonly object _lock = new object();

    /// <summary>
    /// Параметры метода
    /// </summary>
    public ZcaParameters Params { get; set; }

    /// <summary>
    /// Внутренний метод-прослойка для записи в лог
    /// </summary>
    private Action<string>? _writeToLog = null;

    public ZcaAnalyser(ImageHandler image)
    {
        Params = new ZcaParameters(image);
    }

    public ZcaAnalyser(ZcaParameters parameters)
    {
        Params = parameters;
    }


    /// <summary>
    /// Запуск стегоанализа
    /// </summary>
    /// <param name="verboseLog">Вести подробный лог</param>
    public ZcaResult Analyse(bool verboseLog = false)
    {
        var timer = Stopwatch.StartNew();

        var result = new ZcaResult();
        _writeToLog = result.Log;

        _writeToLog($"Started steganalysis by method '{MethodName}' for image '{Params.Image.ImgName}'");

        if (Params.UseOverallCompression)
        {
            var analyzeTask = Task.Run(() => SingleAnalyze(null, verboseLog));
            var isHided = analyzeTask.Result;
            result.IsHidingDetected = isHided;
        }
        else
        {
            var tasks = new List<Task>();
            foreach (var channel in Params.Channels)
            {
                tasks.Add(Task.Run(() =>
                {
                    var isHided = SingleAnalyze(channel, verboseLog);
                    lock (_lock)
                    {
                        result.IsHidedByChannels[channel] = isHided;
                        _writeToLog($"Is hided in channel '{channel}': {isHided}");
                    }
                }));
            }
            Task.WaitAll(tasks);

            result.IsHidingDetected = result.IsHidedByChannels.Values.Count(v => v is true) > Params.Channels.Count / 2;
        }

        _writeToLog($"Hiding is {(result.IsHidingDetected ? "" : "not")} detected");

        timer.Stop();
        _writeToLog($"Steganalysis by method '{MethodName}' ended for {timer.ElapsedMilliseconds} ms");

        result.ElapsedTime = timer.ElapsedMilliseconds;
        return result;
    }

    public bool SingleAnalyze(ImgChannel? channel = null, bool verboseLog = false)
    {
        var traversalOptions = Params.GetTraversalOptions();
        traversalOptions.Channels = Params.UseOverallCompression || channel is null ? Params.Channels : [channel.Value];
        var iterator = BlocksTraverseHelper.GetForLinearAccessBlocksIndexes(Params.ImgBlocks, traversalOptions);

        double deltaSum = 0;
        int blockNum = 0;  // d
        int ltThresholdBlocks = 0;

        Parallel.ForEach(iterator, block =>
        {
            var blockCoords = Params.ImgBlocks[block.Y, block.X];

            SKBitmap? blockBitmap = null, shuffledBlockBitmap = null;
            ScPixel[,]? shuffledBlock = null;

            var blockBitmapTask = Task.Run(() => blockBitmap = CreateBlockBitmap(blockCoords, Params.UseOverallCompression ? null : channel));
            var shuffledBlockTask = Task.Run(() => shuffledBlock = ShuffleBlockLsb(blockCoords, Params.UseOverallCompression ? null : channel));

            shuffledBlockTask.Wait();
            var shuffledBlockBitmapTask = Task.Run(() => shuffledBlockBitmap = shuffledBlock is null ? null : CreateBlockBitmap(shuffledBlock, channel));

            blockBitmapTask.Wait();
            shuffledBlockBitmapTask.Wait();

            double fX = 0.0, fY = 0.0;  // Коэффициенты сжатия блоков f(X, n)
            if (blockBitmap is not null && shuffledBlockBitmap is not null)
            {
                var compressionRatioTasks = new List<Task>
                {
                    Task.Run(() => fX = GetCompressionRatio(blockBitmap)),
                    Task.Run(() => fY = GetCompressionRatio(shuffledBlockBitmap))
                };
                Task.WaitAll(compressionRatioTasks);
            }
            else
                _writeToLog?.Invoke($"In channel '{channel}' for block {blockNum} blockBitmap or shuffledBlockBitmap is null, deltaSum will be 0");

            double delta = Math.Abs(fX - fY);
            lock (_lock)
            {
                deltaSum += delta;
                if (delta <= Params.RatioThreshold)
                    ltThresholdBlocks++;
                blockNum++;
            }
        });

        int halfBlocksNum = blockNum / 2;
        bool isHidingDetected = ltThresholdBlocks > halfBlocksNum;

        _writeToLog?.Invoke($"ZCA method results {(channel is not null || !Params.UseOverallCompression ? $"in channel '{channel}'" : "")}: " +
            $"blocks number = {blockNum}; blocks with compressing ratio less than threshold ('{Params.RatioThreshold}') = {ltThresholdBlocks}" +
            $"{(isHidingDetected ? ">" : "<")} {halfBlocksNum} => hiding is {(isHidingDetected ? "" : "not")} detected");

        return isHidingDetected;
    }

    // При передаче channel == null сохраняются данные по всем каналам
    private SKBitmap CreateBlockBitmap(BlockCoords blockCoords, ImgChannel? channel = null)
    {
        int height = blockCoords.Rd.Y - blockCoords.Lt.Y + 1;
        int width = blockCoords.Rd.X - blockCoords.Lt.X + 1;

        var bitmap = new SKBitmap(width, height);

        for (int y = blockCoords.Lt.Y; y <= blockCoords.Rd.Y; y++)
        {
            for (int x = blockCoords.Lt.X; x <= blockCoords.Rd.X; x++)
            {
                var pixel = Params.Image.ImgArray[y, x];
                bitmap.SetPixel(x - blockCoords.Lt.X, y - blockCoords.Lt.Y, MapToSKColor(pixel, channel));
            }
        }

        return bitmap;
    }

    // При передаче channel == null сохраняются данные по всем каналам
    private static SKBitmap CreateBlockBitmap(ScPixel[,] block, ImgChannel? channel = null)
    {
        int height = block.GetLength(0);
        int width = block.GetLength(1);

        var bitmap = new SKBitmap(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var pixel = block[y, x];
                bitmap.SetPixel(x, y, MapToSKColor(pixel, channel));
            }
        }

        return bitmap;
    }

    private static SKColor MapToSKColor(ScPixel pixel, ImgChannel? channel = null) =>
        new SKColor(red: (channel is null or ImgChannel.Red ? pixel.Red : (byte)0),
                    green: (channel is null or ImgChannel.Green ? pixel.Green : (byte)0),
                    blue: (channel is null or ImgChannel.Blue ? pixel.Blue : (byte)0));

    private ScPixel[,] ShuffleBlockLsb(BlockCoords blockCoords, ImgChannel? channel = null)
    {
        var imar = Params.Image.ImgArray;

        int height = blockCoords.Rd.Y - blockCoords.Lt.Y + 1;
        int width = blockCoords.Rd.X - blockCoords.Lt.X + 1;

        var rnd = new Random();
        var channels = channel is null ? Params.Channels : new UniqueList<ImgChannel> { channel.Value };

        var shuffledBlock = new ScPixel[height, width];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var pixel = new ScPixel(0, 0, 0);
                foreach (var ch in channels)
                {
                    var value = imar[blockCoords.Lt.Y + y, blockCoords.Lt.X + x][(int)ch];
                    if (rnd.Next(2) == 1)
                        value ^= 0b0000_0001; // Invert LSB
                    pixel[(int)ch] = value;
                }
                shuffledBlock[y, x] = pixel;
            }
        }

        return shuffledBlock;
    }

    private double GetCompressionRatio(SKBitmap bitmap)
    {
        SKData data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
        byte[] raw = data.ToArray();

        long compressedLength = GetCompressedLength(raw, Params.CompressingAlgorithm);
        double ratio = (double)compressedLength / raw.Length;

        return ratio;
    }

    private static long GetCompressedLength(byte[] data, CompressingAlgorithm algo)
    {
        long length = 0;
        var compressedStream = new MemoryStream();

        switch (algo)
        {
            case CompressingAlgorithm.ZIP:
                var zip = new ZipArchive(compressedStream, ZipArchiveMode.Create, true);
                var entry = zip.CreateEntry("image");
                var entryStream = entry.Open();
                entryStream.Write(data, 0, data.Length);
                length = compressedStream.Length;
                entryStream.Close();
                break;
            case CompressingAlgorithm.GZIP:
                var gzip = new GZipStream(compressedStream, CompressionMode.Compress);
                gzip.Write(data, 0, data.Length);
                length = compressedStream.Length;
                gzip.Close();
                break;
            case CompressingAlgorithm.BZIP2:
                var bzip2 = new BZip2Stream(compressedStream, SharpCompress.Compressors.CompressionMode.Compress, true);
                bzip2.Write(data, 0, data.Length);
                length = compressedStream.Length;
                bzip2.Close();
                break;
        }
        compressedStream.Close();

        return length;
    }
}
