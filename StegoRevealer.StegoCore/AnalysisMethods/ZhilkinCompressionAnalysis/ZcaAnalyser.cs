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

    private bool _verboseLog = false;

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
        _verboseLog = verboseLog;
        var timer = Stopwatch.StartNew();

        var result = new ZcaResult();
        _writeToLog = result.LogInfo;

        _writeToLog($"Started steganalysis by method '{MethodName}' for image '{Params.Image.ImgName}'");

        try
        {
            AnalyseInner(result).Wait();
        }
        catch (Exception ex)
        {
            result.LogError($"Fatal error while executing '{MethodName}': [{ex.GetType().Name}] {ex.Message}");
            result.MethodSuccessful = false;
        }

        timer.Stop();
        _writeToLog($"Steganalysis by method '{MethodName}' ended for {timer.ElapsedMilliseconds} ms");

        result.ElapsedTime = timer.ElapsedMilliseconds;
        return result;
    }

    public async Task AnalyseInner(ZcaResult result)
    {
        if (Params.UseOverallCompression)
        {
            var analyzeTask = Task.Run(() => SingleAnalyze(null, _verboseLog));
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
                    var isHided = SingleAnalyze(channel, _verboseLog);
                    lock (_lock)
                    {
                        result.IsHidedByChannels[channel] = isHided;
                        _writeToLog?.Invoke($"Is hided in channel '{channel}': {isHided}");
                    }
                }));
            }
            await Task.WhenAll(tasks);

            result.IsHidingDetected = result.IsHidedByChannels.Values.Count(v => v is true) > Params.Channels.Count / 2;
        }

        _writeToLog?.Invoke($"Hiding is {(result.IsHidingDetected ? "" : "not")} detected");
    }

    public bool SingleAnalyze(ImgChannel? channel = null, bool verboseLog = false)
    {
        var traversalOptions = Params.GetTraversalOptions();
        traversalOptions.Channels = Params.UseOverallCompression || channel is null ? Params.Channels : [channel.Value];
        var iterator = BlocksTraverseHelper.GetForLinearAccessBlocksIndexes(Params.ImgBlocks, traversalOptions);

        int blockNum = 0;  // d
        int ltThresholdBlocks = 0;

        Parallel.ForEach(
            source: iterator, 
            localInit: () => (blocks: 0, lt: 0),
            body: (block, state, local) =>
            {
                var blockCoords = Params.ImgBlocks[block.Y, block.X];

                SKBitmap? blockBitmap = null, shuffledBlockBitmap = null;
                ScPixel[,]? shuffledBlock = null;

                Parallel.Invoke(
                    () =>
                    {
                        blockBitmap = CreateBlockBitmap(blockCoords, Params.UseOverallCompression ? null : channel);
                    },
                    () =>
                    {
                        shuffledBlock = ShuffleBlockLsb(blockCoords, Params.UseOverallCompression ? null : channel);
                        shuffledBlockBitmap = shuffledBlock is null ? null : CreateBlockBitmap(shuffledBlock, channel);
                    }
                );

                try
                {
                    double fX = 0.0, fY = 0.0;  // Коэффициенты сжатия блоков f(X, n)
                    if (blockBitmap is not null && shuffledBlockBitmap is not null)
                    {
                        Parallel.Invoke(
                            () => fX = GetCompressionRatio(blockBitmap),
                            () => fY = GetCompressionRatio(shuffledBlockBitmap)
                        );
                    }
                    else
                        _writeToLog?.Invoke($"In channel '{channel}' for block {blockNum} blockBitmap or shuffledBlockBitmap is null");

                    double delta = Math.Abs(fX - fY);
                    local.blocks++;
                    if (delta <= Params.RatioThreshold)
                        local.lt++;
                }
                finally
                {
                    blockBitmap?.Dispose();
                    shuffledBlockBitmap?.Dispose();
                }

                return local;
            },
            localFinally: local =>
            {
                Interlocked.Add(ref blockNum, local.blocks);
                Interlocked.Add(ref ltThresholdBlocks, local.lt);
            }
        );

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

        var bitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);

        unsafe
        {
            IntPtr ptr = bitmap.GetPixels();
            byte* pixels = (byte*)ptr;

            for (int y = blockCoords.Lt.Y; y <= blockCoords.Rd.Y; y++)
            {
                for (int x = blockCoords.Lt.X; x <= blockCoords.Rd.X; x++)
                {
                    ScPixel px = Params.Image.ImgArray[y, x];
                    int offset = ((y - blockCoords.Lt.Y) * width + (x - blockCoords.Lt.X)) * 4;  // 4 байта на пиксель (BGRA)

                    pixels[offset + 0] = (channel is null || channel == ImgChannel.Blue) ? px.Blue : (byte)0;
                    pixels[offset + 1] = (channel is null || channel == ImgChannel.Green) ? px.Green : (byte)0;
                    pixels[offset + 2] = (channel is null || channel == ImgChannel.Red) ? px.Red : (byte)0;
                    pixels[offset + 3] = px.Alpha;
                }
            }
        }

        return bitmap;
    }

    // При передаче channel == null сохраняются данные по всем каналам
    private static SKBitmap CreateBlockBitmap(ScPixel[,] block, ImgChannel? channel = null)
    {
        int height = block.GetLength(0);
        int width = block.GetLength(1);

        var bitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);

        unsafe
        {
            IntPtr ptr = bitmap.GetPixels();
            byte* pixels = (byte*)ptr;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    ScPixel px = block[y, x];
                    int offset = (y * width + x) * 4; // 4 байта на пиксель (BGRA)

                    pixels[offset + 0] = (channel is null || channel == ImgChannel.Blue) ? px.Blue : (byte)0;
                    pixels[offset + 1] = (channel is null || channel == ImgChannel.Green) ? px.Green : (byte)0;
                    pixels[offset + 2] = (channel is null || channel == ImgChannel.Red) ? px.Red : (byte)0;
                    pixels[offset + 3] = px.Alpha;
                }
            }
        }

        return bitmap;
    }

    private static SKColor MapToSKColor(ScPixel pixel, ImgChannel? channel = null) =>
        new SKColor(red: (channel is null or ImgChannel.Red ? pixel.Red : (byte)0),
                    green: (channel is null or ImgChannel.Green ? pixel.Green : (byte)0),
                    blue: (channel is null or ImgChannel.Blue ? pixel.Blue : (byte)0));

    private static readonly ThreadLocal<Random> _rnd = new(() => new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId)));

    private ScPixel[,] ShuffleBlockLsb(BlockCoords blockCoords, ImgChannel? channel = null)
    {
        var imar = Params.Image.ImgArray;

        int height = blockCoords.Rd.Y - blockCoords.Lt.Y + 1;
        int width = blockCoords.Rd.X - blockCoords.Lt.X + 1;

        var rnd = _rnd.Value!;
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

        data.Dispose();
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
                entryStream.Flush();
                length = compressedStream.Length;
                entryStream.Close();
                zip.Dispose();
                break;
            case CompressingAlgorithm.GZIP:
                var gzip = new GZipStream(compressedStream, CompressionMode.Compress);
                gzip.Write(data, 0, data.Length);
                gzip.Flush();
                length = compressedStream.Length;
                gzip.Close();
                gzip.Dispose();
                break;
            case CompressingAlgorithm.BZIP2:
                var bzip2 = new BZip2Stream(compressedStream, SharpCompress.Compressors.CompressionMode.Compress, true);
                bzip2.Write(data, 0, data.Length);
                bzip2.Flush();
                length = compressedStream.Length;
                bzip2.Close();
                bzip2.Dispose();
                break;
        }
        compressedStream.Close();

        return length;
    }
}
