using StegoRevealer.StegoCore.CommonLib;

namespace StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Calculators;

public class BlurCalculator
{
    private StatmParameters _params;

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
        for (int y = 0; y < imar.Height; y++)
            for (int x = 0; x < imar.Width; x++)
                C += Math.Abs((double)(blurredImarB1[y, x] - blurredImarB2[y, x]) / MN);

        return 1 / C;  // Возвращаем обратное: чем выше число, тем больше размытость
    }

    private static byte[,] ApplyAverageFilter(byte[,] pixelsArray, int filterSize)
    {
        int width = pixelsArray.GetLength(0);
        int height = pixelsArray.GetLength(1);
        byte[,] result = new byte[width, height];

        int filterOffset = filterSize / 2;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
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
        }

        return result;
    }
}
