using StegoRevealer.StegoCore.CommonLib;
using System.ComponentModel;

namespace StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Calculators;

public class BlurCalculator
{
    private readonly StatmParameters _params;

    private readonly object _locker = new object();

    public BlurCalculator(StatmParameters parameters)
    {
        _params = parameters;
    }

    // Вычисление оценки размытости
    public double CalcBlur()
    {
        var imar = _params.Image.ImgArray;
        var gimar = PixelsTools.ToGrayscale(imar, _params.BlurCalcUseAveragedGrayscale);

        var blurredImarB1 = ApplyAverageFilter(gimar, _params.BlurCalcFilterSizeK1);
        var blurredImarB2 = ApplyAverageFilter(gimar, _params.BlurCalcFilterSizeK2);

        int MN = imar.Height * imar.Width;
        double C = 0.0;
        Parallel.For(0, imar.Height, () => 0.0, (y, state, localSum) =>
        {
            for (int x = 0; x < imar.Width; x++)
                localSum += Math.Abs((double)(blurredImarB1[y, x] - blurredImarB2[y, x]) / MN);
            return localSum;
        },
        localSum =>
        {
            lock (_locker)
            {
                C += localSum;
            }
        });

        return 1 / C;  // Возвращаем обратное: чем выше число, тем больше размытость
    }

    private static byte[,] ApplyAverageFilter(byte[,] pixelsArray, int filterSize)
    {
        int height = pixelsArray.GetLength(1);
        int width = pixelsArray.GetLength(0);
        byte[,] result = new byte[width, height];

        int filterOffset = filterSize / 2;

        Parallel.For(0, height, y =>
        {
            for (int x = 0; x < width; x++)
            {
                int averageSum = 0;
                int pixelCount = 0;

                // Проходим по окну фильтра
                for (int filterX = -filterOffset; filterX <= filterOffset; filterX++)
                {
                    for (int filterY = -filterOffset; filterY <= filterOffset; filterY++)
                    {
                        int imageX = x + filterX;
                        int imageY = y + filterY;

                        // Проверяем, находится ли точка в пределах изображения
                        if (imageX >= 0 && imageX < width && imageY >= 0 && imageY < height)
                        {
                            averageSum += pixelsArray[imageX, imageY];
                            pixelCount++;
                        }
                    }
                }

                // Вычисляем среднее значение и присваиваем его текущему пикселю в результирующем изображении
                byte averageValue = (byte)(averageSum / pixelCount);
                result[x, y] = averageValue;
            }
        });

        return result;
    }
}
