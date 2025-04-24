using System.Diagnostics;
using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevealer.StegoCore.AnalysisMethods.SamplePairAnalysis;

public class SpaAnalyser
{
    private const string MethodName = "SPA (Sample Pair Analysis)";

    private static readonly object _lock = new object();

    /// <summary>
    /// Параметры метода
    /// </summary>
    public SpaParameters Params { get; set; }

    /// <summary>
    /// Внутренний метод-прослойка для записи в лог
    /// </summary>
    private Action<string>? _writeToLog = null;

    public SpaAnalyser(ImageHandler image)
    {
        Params = new SpaParameters(image);
    }

    public SpaAnalyser(SpaParameters parameters)
    {
        Params = parameters;
    }


    /// <summary>
    /// Запуск стегоанализа
    /// </summary>
    /// <param name="verboseLog">Вести подробный лог</param>
    public SpaResult Analyse(bool verboseLog = false)
    {
        var timer = Stopwatch.StartNew();

        var result = new SpaResult();
        _writeToLog = result.Log;

        _writeToLog($"Started steganalysis by method '{MethodName}' for image '{Params.Image.ImgName}'");

        var analysisTasks = new List<Task>();
        foreach (var channel in Params.Channels)
        {
            analysisTasks.Add(Task.Run(() =>
            {
                double volume = 0.0;

                switch (Params.MethodVersion)
                {
                    case SpaVersion.Original:
                        volume = AnalyzeInOneChannel((int)channel);
                        break;
                    case SpaVersion.StegExpose:
                        volume = AnalyzeInOneChannelStegExpose((int)channel);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(Params.MethodVersion), "Unknown SPA version");
                }

                lock (_lock)
                {
                    result.MessageRelativeVolumesByChannels[channel] = volume;
                    _writeToLog($"Relative message volume at channel '{channel}': {volume}");
                }
            }));
        }
        Task.WaitAll(analysisTasks);

        result.MessageRelativeVolume = result.MessageRelativeVolumesByChannels.Values.Average();
        _writeToLog($"Average relative message volume = {result.MessageRelativeVolume}");

        timer.Stop();
        _writeToLog($"Steganalysis by method '{MethodName}' ended for {timer.ElapsedMilliseconds} ms");

        result.ElapsedTime = timer.ElapsedMilliseconds;
        return result;
    }

    // Классическая версия метода
    private double AnalyzeInOneChannel(int channgelId)
    {
        double P = 0.0, Q = 0.0;
        if (!Params.UseDoubleDirection)
            (P, Q) = CalcPairsValues(channgelId, Params.Direction);
        else
        {
            double horizP = 0.0, horizQ = 0.0, vertP = 0.0, vertQ = 0.0;
            var tasks = new List<Task>
            {
                Task.Run(() => (horizP, horizQ) = CalcPairsValues(channgelId, PairDirection.Horizontal)),
                Task.Run(() => (vertP, vertQ) = CalcPairsValues(channgelId, PairDirection.Vertical))
            };
            Task.WaitAll(tasks);

            P = horizP + vertP;
            Q = horizQ + vertQ;
        }

        // Вычисление статистики и вероятности наличия стегоданных
        double p = P + Q == 0 ? 0 : Math.Abs((double)(P - Q) / (P + Q));
        p = Math.Max(0, Math.Min(1, p));
        return p;
    }

    private (double P, double Q) CalcPairsValues(int channgelId, PairDirection pairDirection)
    {
        int P = 0, Q = 0;
        var imar = Params.Image.ImgArray;

        int localWidth = Params.Image.Width;
        if (pairDirection is not PairDirection.Vertical)
            localWidth--;
        int localHeight = Params.Image.Height;
        if (pairDirection is not PairDirection.Horizontal)
            localHeight--;

        Parallel.For(0, localHeight, y =>
        {
            for (int x = 0; x < localWidth; x++)
            {
                (byte x1, byte x2) = GetPairValues(imar, y, x, channgelId, pairDirection);
                int diff = x1 - x2;

                if (Math.Abs(diff) == 1)
                {
                    if ((x1 & 1) == 0 && diff == 1)
                        Interlocked.Increment(ref P);
                    else if ((x1 & 1) == 1 && diff == -1)
                        Interlocked.Increment(ref P);
                    else if ((x1 & 1) == 0 && diff == -1)
                        Interlocked.Increment(ref Q);
                    else if ((x1 & 1) == 1 && diff == 1)
                        Interlocked.Increment(ref Q);
                }
            }
        });

        return (P, Q);
    }

    /// <summary>
    /// Получает значения пары пикселей в зависимости от направления
    /// </summary>
    private (byte p1, byte p2) GetPairValues(ImageArray imgArray, int y, int x, int channelId, PairDirection? pairDirection = null)
    {
        byte p1 = imgArray[y, x, channelId];

        pairDirection ??= Params.Direction;
        byte p2 = pairDirection switch
        {
            PairDirection.Horizontal => imgArray[y, x + 1, channelId],
            PairDirection.Vertical => imgArray[y + 1, x, channelId],
            PairDirection.Diagonal => imgArray[y + 1, x + 1, channelId],
            _ => (byte)0
        };
        return (p1, p2);
    }

    // Реализация StegExpose-версии
    private double AnalyzeInOneChannelStegExpose(int channgelId)
    {
        int[] histogram = new int[256];
        var imar = Params.Image.ImgArray;

        int P = 0, X = 0, Y = 0, Z = 0, W = 0;

        void calcValues(byte u, byte v)
        {
            if ((u >> 1 == v >> 1) && (v & 0x1) != (u & 0x1))
                Interlocked.Increment(ref W);
            if (u == v)
                Interlocked.Increment(ref Z);
            if ((v == (v >> 1) << 1) && (u < v) || (v != (v >> 1) << 1) && (u > v))
                Interlocked.Increment(ref X);
            if ((v == (v >> 1) << 1) && (u > v) || (v != (v >> 1) << 1) && (u < v))
                Interlocked.Increment(ref Y);
            Interlocked.Increment(ref P);
        }

        Task[] tasks = [
            Task.Run(() =>
                Parallel.For(0, Params.Image.Height, y =>
                {
                    for (int x = 0; x < Params.Image.Width - 1; x += 2)
                        calcValues(imar[y, x, channgelId], imar[y, x + 1, channgelId]);
                })
            ),
            Task.Run(() =>
                Parallel.For(0, Params.Image.Width, x =>
                {
                    for (int y = 0; y < Params.Image.Height - 1; y += 2)
                        calcValues(imar[y, x, channgelId], imar[y + 1, x, channgelId]);
                })
            )
        ];
        Task.WaitAll(tasks);

        double a = 0.5 * (W + Z);
        double b = 2 * X - P;
        double c = Y - X;
        double x;

        if (a == 0)
            x = c / b;
        double discriminant = Math.Pow(b, 2) - (4 * a * c);

        if (discriminant >= 0)
        {
            double rootpos = ((-1 * b) + Math.Sqrt(discriminant)) / (2 * a);
            double rootneg = ((-1 * b) - Math.Sqrt(discriminant)) / (2 * a);

            if (Math.Abs(rootpos) <= Math.Abs(rootneg))
                x = rootpos;
            else
                x = rootneg;
        }
        else
            x = c / b;

        if (x == 0)
            x = c / b;
        return x;
    }
}
