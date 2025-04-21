using StegoRevealer.StegoCore.ImageHandlerLib;
using System.Diagnostics;
using System.Threading.Channels;

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

        _writeToLog($"Выполняется стегоанализ методом {MethodName} для файла изображения {Params.Image.ImgName}");

        var analysisTasks = new List<Task>();
        foreach (var channel in Params.Channels)
        {
            analysisTasks.Add(Task.Run(() =>
            {
                double probability = AnalyzeInOneChannel((int)channel);

                lock (_lock)
                {
                    result.HidedDataProbabilities[channel] = probability;
                    _writeToLog($"Вероятность наличия скрытого сообщения в канале {channel}: {probability}");
                }
            }));
        }
        Task.WaitAll(analysisTasks);

        result.AvgHidedDataProbability = result.HidedDataProbabilities.Values.Average();
        _writeToLog($"Усреднённая вероятность наличия скрытого сообщения: {result.AvgHidedDataProbability}");

        timer.Stop();
        _writeToLog($"Стегоанализ методом {MethodName} завершён за {timer.ElapsedMilliseconds} мс");
        return result;
    }

    private double AnalyzeInOneChannel(int channgelId)
    {
        int u = 0, v = 0;

        Parallel.For(0, Params.Image.Height, () => (0, 0), (y, state, counts) =>
        {
            var imar = Params.Image.ImgArray;
            int localU = counts.Item1;
            int localV = counts.Item2;

            for (int x = 0; x < Params.Image.Width - 1; x += 2)
            {
                byte x1 = imar[y, x, channgelId];
                byte x2 = imar[y, x + 1, channgelId];

                if (x1 / 2 == x2 / 2)
                    localU++;
                else if (Math.Abs(x1 - x2) == 1)
                    localV++;
            }

            return (localU, localV);
        },
        counts =>
        {
            lock (this)
            {
                u += counts.Item1;
                v += counts.Item2;
            }
        });

        // Вычисление статистики и вероятности наличия стегоданных
        double totalPairs = u + v;
        if (totalPairs == 0)
            return 0.0;

        double P = v / totalPairs;
        return P;
    }
}
