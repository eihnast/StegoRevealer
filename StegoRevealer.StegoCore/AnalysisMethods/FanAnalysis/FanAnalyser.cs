using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using Accord.Math;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;
using MessagePack;
using MessagePack.Resolvers;
using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevealer.StegoCore.AnalysisMethods.FanAnalysis;

/// <summary>
/// Стегоанализатор метода Коха-Жао
/// </summary>
public class FanAnalyser
{
    private const string MethodName = "FAN (Fast Additive Noise)";

    /// <summary>
    /// Параметры метода
    /// </summary>
    public FanParameters Params { get; set; }

    /// <summary>
    /// Внутренний метод-прослойка для записи в лог
    /// </summary>
    private Action<string> _writeToLog = (string str) => new string(str);

    // Пары каналов: RG, GB, BR
    private static readonly (Func<ScPixel, byte>, Func<ScPixel, byte>)[] ChannelPairs = new[]
    {
        (new Func<ScPixel, byte>(c => c.Red), new Func<ScPixel, byte>(c => c.Green)),
        (new Func<ScPixel, byte>(c => c.Green), new Func<ScPixel, byte>(c => c.Blue)),
        (new Func<ScPixel, byte>(c => c.Blue), new Func<ScPixel, byte>(c => c.Red))
    };

    // Размер гистограммы (по одному каналу от 0 до 255)
    private const int BinSize = 256;

    private static double[][]? _trainingComs = null;


    static FanAnalyser()
    {
        _trainingComs = LoadTrainingComs();
    }

    public FanAnalyser(ImageHandler image)
    {
        Params = new FanParameters(image);
    }

    public FanAnalyser(FanParameters parameters)
    {
        Params = parameters;
    }


    /// <summary>
    /// Запуск стегоанализа
    /// </summary>
    /// <param name="verboseLog">Вести подробный лог</param>
    public FanResult Analyse(bool verboseLog = false)
    {
        var timer = Stopwatch.StartNew();

        var result = new FanResult();
        _writeToLog = result.Log;
        _writeToLog($"Started steganalysis by method '{MethodName}' for image '{Params.Image.ImgName}'");

        var comList = ComputeCompositeCom(Params.Image);
        result.ComsList = comList;

        if (_trainingComs is not null)
        {
            // var threshold = CalcAdaptiveThreshold(_trainingComs, 1.0);

            double distance = ComputeMahalanobisDistance(comList, _trainingComs);
            result.MahalanobisDistance = distance;
            _writeToLog($"Mahanalobis distance: {distance}");

            result.IsHidingDetected = distance < Params.Threshold;
        }
        else
        {
            result.Error("Training COMs array is null. Method result is false by default.");
        }

        timer.Stop();
        _writeToLog($"Steganalysis by method '{MethodName}' ended for {timer.ElapsedMilliseconds} ms");

        result.ElapsedTime = timer.ElapsedMilliseconds;
        return result;
    }

    private static double[][]? LoadTrainingComs()
    {
        double[][]? trainingComs = null;

        var assembly = Assembly.GetExecutingAssembly();
        string resourceName = "StegoRevealer.StegoCore.AnalysisMethods.FanAnalysis.FanComsTrainingSet.msgpack";

        using (var stream = assembly.GetManifestResourceStream(resourceName))
        using (MemoryStream ms = new MemoryStream())
        {
            if (stream is not null)
            {
                stream.CopyTo(ms);
                trainingComs = MessagePackSerializer.Deserialize<double[][]>(ms.ToArray(), ContractlessStandardResolver.Options);
            }
        }

        return trainingComs;
    }

    public static double[] ComputeCompositeCom(ImageHandler image)
    {
        var comList = new double[6];  // 3 пары × 2 координаты центра масс (x, y)
        int pairCount = ChannelPairs.Length;

        Parallel.For(0, pairCount, i =>
        {
            var (c1Func, c2Func) = ChannelPairs[i];

            // Построение двумерной гистограммы
            var hist = new double[BinSize, BinSize];

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    var pixel = image.ImgArray[y, x];
                    byte v1 = c1Func(pixel);
                    byte v2 = c2Func(pixel);
                    hist[v1, v2]++;
                }
            }

            // Преобразуем гистограмму в комплексную матрицу
            var fftInput = new Complex[BinSize, BinSize];
            Parallel.For(0, BinSize, y =>
            {
                for (int x = 0; x < BinSize; x++)
                    fftInput[y, x] = new Complex((float)hist[y, x], 0);
            });

            // Прямое 2D FFT
            FourierTransform.FFT2(fftInput, FourierTransform.Direction.Forward);

            // Сдвигаем FFT, чтобы центр спектра был в середине
            var shiftedFFT = FFTShift2D(fftInput);

            // Расчёт центра масс по модулю спектра
            double totalMagnitude = 0;
            double sumX = 0, sumY = 0;

            for (int y = 0; y < BinSize / 2; y++) // Первая половина по каждой оси
            {
                for (int x = 0; x < BinSize / 2; x++)
                {
                    double mag = shiftedFFT[y, x].Magnitude;
                    totalMagnitude += mag;
                    sumX += x * mag;
                    sumY += y * mag;
                }
            }

            comList[i * 2] = sumX / totalMagnitude;
            comList[i * 2 + 1] = sumY / totalMagnitude;
        });

        return comList;
    }

    private static Complex[,] FFTShift2D(Complex[,] input)
    {
        int width = input.GetLength(0);
        int height = input.GetLength(1);
        var output = new Complex[width, height];

        int halfWidth = width / 2;
        int halfHeight = height / 2;

        for (int y = 0; y < height; y++)
        {
            int shiftedY = (y + halfHeight) % height;
            for (int x = 0; x < width; x++)
            {
                int shiftedX = (x + halfWidth) % width;
                output[shiftedY, shiftedX] = input[y, x];
            }
        }

        return output;
    }

    public static double ComputeMahalanobisDistance(double[] vector, double[][] trainingSet)
    {
        var matrix = Matrix<double>.Build;
        var vec = MathNet.Numerics.LinearAlgebra.Vector<double>.Build;

        var X = matrix.DenseOfRowArrays(trainingSet);
        var mean = vec.DenseOfEnumerable(X.ColumnSums() / X.RowCount);
        var cov = CovarianceMatrix(X);
        var diff = vec.DenseOfArray(vector) - mean;
        var invCov = cov.Inverse();

        return Math.Sqrt(diff * invCov * diff);
    }

    private static Matrix<double> CovarianceMatrix(Matrix<double> data)
    {
        int n = data.RowCount;
        var mean = data.ColumnSums() / n;
        var meanMatrix = Matrix<double>.Build.Dense(n, data.ColumnCount, (i, j) => mean[j]);

        var centered = data - meanMatrix;
        return (centered.TransposeThisAndMultiply(centered)) / (n - 1);
    }

    private static double CalcAdaptiveThreshold(double[][] trainingComs, double k = 2.5)
    {
        int trainCount = trainingComs.Length;
        var distances = new double[trainCount];

        for (int i = 0; i < trainCount; i++)
        {
            var currentPoint = trainingComs[i];
            var remainingPoints = trainingComs
                .Where((_, idx) => idx != i)
                .ToArray();
            distances[i] = ComputeMahalanobisDistance(currentPoint, remainingPoints);
        }

        double mean = distances.Average();
        double stddev = distances.StandardDeviation();
        double adaptiveThreshold = mean + k * stddev;
        //Console.WriteLine($"Adaptive threshold: {adaptiveThreshold}");

        return adaptiveThreshold;
    }
}
