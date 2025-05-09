﻿using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Entities;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.CommonLib.Exceptions;
using StegoRevealer.StegoCore.ScMath;
using static Plotly.NET.StyleParam.Range;

namespace StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Calculators;

public class SharpnessCalculator
{
    private readonly StatmParameters _params;

    private readonly object _lock = new object();

    public SharpnessCalculator(StatmParameters parameters)
    {
        _params = parameters;
    }


    // Вычисление оценки резкости
    public double CalcSharpness()
    {
        var canny = CannyEdgeDetection();
        var edgesImar = canny.EdgePixelsArray;  // Матрица граничных пискелей по методу Канни
        var directionsImar = canny.SobelOperatorResult.Direction;  // Матрица направлений в радиантах
        var anglesImar = GetAngles(directionsImar, repairNegativeAngles: true);  // Получаем матрицу углов из значений направлений в радиантах
        var gimar = canny.GrayscaledPixelsArray;

        int height = edgesImar.GetLength(0);
        int width = edgesImar.GetLength(1);

        double maxSharpness = 0.0;

        Parallel.For(0, height, y =>  // Обходим матрицу граничных пикселей
        {
            double localSharpness = 0.0;

            for (int x = 0; x < width; x++)
            {
                var currentPixel = edgesImar[y, x];
                if (currentPixel != 0)  // Если это граничный пиксель т.е. - пиксель weak или strong
                {
                    List<PixelInfo> pixelsOnLine = new();
                    int maxOffset = _params.SharpnessCalcExtremumsNeighborhoodSize;  // Максимальное смещение = размеру окрестности, в пределах которой ищем экстремумы

                    // Находим крайние пиксели прямой
                    double k = Math.Tan(anglesImar[y, x]);  // y = kx, условно, с центром в центре текущего пикселя, k = тангенс угла из матрицы углов

                    // Вырожденный случай горизонтали
                    if (k.Equals(0))
                    {
                        for (int localX = Math.Max(x - maxOffset, 0); localX < Math.Min(x + maxOffset, width - 1); localX++)
                            pixelsOnLine.Add(new PixelInfo { Y = y, X = localX });
                    }
                    else
                    {
                        double maxX = maxOffset + 0.5;
                        double maxY = maxOffset + 0.5;
                        double yForMaxX = k * maxX;
                        double xForMaxY = maxY / k;

                        PixelInfo pixelOffset;
                        if ((yForMaxX > 0 && yForMaxX <= maxY) || (yForMaxX < 0 && yForMaxX >= -maxY))
                            pixelOffset = new PixelInfo() { Y = (int)Math.Ceiling(yForMaxX + 0.5) - 1, X = maxOffset };
                        else if ((xForMaxY > 0 && xForMaxY <= maxX) || (xForMaxY < 0 && xForMaxY >= -maxX))
                            pixelOffset = new PixelInfo() { Y = maxOffset, X = (int)Math.Ceiling(xForMaxY + 0.5) - 1 };
                        else
                            throw new CalculationException($"Line coordinates error with: k = {k}, yForMaxX = {yForMaxX}, xForMaxY = {xForMaxY}");

                        var firstPixel = new PixelInfo() { Y = y + pixelOffset.Y, X = x + pixelOffset.X };
                        var secondPixel = new PixelInfo() { Y = y - pixelOffset.Y, X = x - pixelOffset.X };

                        if (firstPixel.Y > height - 1 || firstPixel.Y < 0 || firstPixel.X > width - 1 || firstPixel.X < 0)
                            firstPixel = new PixelInfo { Y = y, X = x };
                        if (secondPixel.Y > height - 1 || secondPixel.Y < 0 || secondPixel.X > width - 1 || secondPixel.X < 0)
                            secondPixel = new PixelInfo { Y = y, X = x };

                        // Применяем алгоритм Брезенхэма
                        pixelsOnLine = PixelsTools.GetPixelsOnLine(firstPixel.Y, firstPixel.X, secondPixel.Y, secondPixel.X);
                    }

                    // Обогащаем значениями пикселей
                    for (int i = 0; i < pixelsOnLine.Count; i++)
                        pixelsOnLine[i].Value = gimar[pixelsOnLine[i].Y, pixelsOnLine[i].X];

                    // Вычисления
                    PixelInfo? minPixel = null, maxPixel = null;
                    byte min = 255;  // a
                    byte max = 0;  // b

                    foreach (var p in pixelsOnLine)
                    {
                        if (p.Value < min)
                        {
                            min = p.Value;
                            minPixel = p;
                        }
                        if (p.Value > max)
                        {
                            max = p.Value;
                            maxPixel = p;
                        }
                    }
                    if (minPixel == null || maxPixel == null)
                        continue;

                    double distance = Math.Sqrt(Math.Pow(minPixel.Y - maxPixel.Y, 2) + Math.Pow(minPixel.X - maxPixel.X, 2));  // w

                    localSharpness = Math.Abs(max - min) / distance;
                }
            }

            lock (_lock)
            {
                if (localSharpness > maxSharpness)
                    maxSharpness = localSharpness;
            }
        });

        return maxSharpness;
    }

    // Детектор определения границ Канни
    public CannyEdgeDetectionResult CannyEdgeDetection()
    {
        var imar = _params.Image.ImgArray;
        var gimar = PixelsTools.ToGrayscale(imar, _params.SharpnessCalcUseAveragedGrayscale);

        // Noise Reduction. Применение фильтра Гаусса
        var guassKernel = MathMethods.GenerateGuassianKernel(_params.SharpnessCalcGuassianKernelSize, _params.SharpnessCalcGuassianKernelSigma);
        var blurredImar = PixelsTools.DoubleToByteMatrix(MathMethods.Convolution(PixelsTools.ByteToDoubleMatrix(gimar), guassKernel));

        // Gradient Calculation
        var sobelResult = ApplySobelOperator(blurredImar, _params.SharpnessCalcUseScharrOperator);

        // Non-Maximum Suppression
        var supressedImar = NonMaximumSuppression(sobelResult.Intensity, sobelResult.Direction);

        // Double threshold
        var processedImar = DoubleThreshold(supressedImar, _params.SharpnessCalcCannyDownThreshold, _params.SharpnessCalcCannyUpThreshold);

        // Edge Tracking by Hysteresis
        var finalImar = ApplyHysteresis(processedImar);

        return new CannyEdgeDetectionResult
        {
            EdgePixelsArray = finalImar,
            GrayscaledPixelsArray = gimar,
            SobelOperatorResult = sobelResult
        };
    }

    // Детектор определения границ Собеля
    public byte[,] SobelEdgeDetection()
    {
        var imar = _params.Image.ImgArray;
        var gimar = PixelsTools.ToGrayscale(imar, _params.SharpnessCalcUseAveragedGrayscale);

        var sobelResult = ApplySobelOperator(gimar, _params.SharpnessCalcUseScharrOperator);
        return sobelResult.Intensity;
    }


    #region Operator kernels

    // Ядро оператора Собеля для вертикальных изменений
    private static double[,] ySobel
    {
        get
        {
            return new double[,] {
                {  1,  2,  1 },
                {  0,  0,  0 },
                { -1, -2, -1 } };
        }
    }
    // Ядро оператора Собеля для горизонтальных изменений
    private static double[,] xSobel
    {
        get
        {
            return new double[,] {
                { 1, 0, -1 },
                { 2, 0, -2 },
                { 1, 0, -1 } };
        }
    }

    // Ядро оператора Щарра для вертикальных изменений
    private static double[,] yScharr
    {
        get
        {
            return new double[,] {
                {  3,  10,  3 },
                {  0,   0,  0 },
                { -3, -10, -3 } };
        }
    }
    // Ядро оператора Щарра для горизонтальных изменений
    private static double[,] xScharr
    {
        get
        {
            return new double[,] {
                { 3,  0, -3  },
                { 10, 0, -10 },
                { 3,  0, -3  } };
        }
    }

    #endregion

    // Применяет оператор Собеля для выделения краевых пикселей и расчёта направлений градиента
    private SobelOperatorResult ApplySobelOperator(byte[,] pixelsMatrix, bool useScharrOperator = false)
    {
        int height = pixelsMatrix.GetLength(0);
        int width = pixelsMatrix.GetLength(1);

        // Применение оператора
        int kernelOffset = 1;  // Ширина окрестности пикселя для применения оператора
        var sharpnessImar = new byte[height, width];
        var gradientDirections = new double[height, width];

        double[,] yKernel = useScharrOperator ? yScharr : ySobel;
        double[,] xKernel = useScharrOperator ? xScharr : xSobel;

        Parallel.For(0, height, y =>
        {
            for (int x = 0; x < width; x++)
            {
                double Gy = 0.0;
                double Gx = 0.0;

                for (int kernelY = -kernelOffset; kernelY <= kernelOffset; kernelY++)
                {
                    for (int kernelX = -kernelOffset; kernelX <= kernelOffset; kernelX++)
                    {
                        int pixelY = y + kernelY;
                        int pixelX = x + kernelX;
                        if (pixelY >= height || pixelY < 0 || pixelX >= width || pixelX < 0)
                            continue;

                        Gy += pixelsMatrix[pixelY, pixelX] * yKernel[kernelY + kernelOffset, kernelX + kernelOffset];
                        Gx += pixelsMatrix[pixelY, pixelX] * xKernel[kernelY + kernelOffset, kernelX + kernelOffset];
                    }
                }

                byte newByte = PixelsTools.ToLimitedByte(Math.Sqrt(Math.Pow(Gy, 2) + Math.Pow(Gx, 2)));  // Sqrt(Gy * Gy + Gx * Gx)
                sharpnessImar[y, x] = newByte;

                gradientDirections[y, x] = Math.Atan2(Gy, Gx);  //  * (180 / Math.PI)
            }
        });

        return new SobelOperatorResult
        {
            Intensity = sharpnessImar,
            Direction = gradientDirections
        };
    }

    // Формирует матрицу углов в градусов из матрицы значений углов в радиантах
    private static double[,] GetAngles(double[,] directions, bool repairNegativeAngles = true)
    {
        const double angleConst = 180 / Math.PI;

        var angles = new double[directions.GetLength(0), directions.GetLength(1)];
        for (int i = 0; i < angles.GetLength(0); i++)
            for (int j = 0; j < angles.GetLength(1); j++)
                angles[i, j] = directions[i, j] * angleConst;

        if (repairNegativeAngles)
        {
            for (int i = 0; i < angles.GetLength(0); i++)
                for (int j = 0; j < angles.GetLength(1); j++)
                    if (angles[i, j] < 0)
                        angles[i, j] += 360;
        }

        return angles;
    }

    // "Сжатие границ": оставляем минимум значимых для границы перепада пикселей
    private static byte[,] NonMaximumSuppression(byte[,] pixelsArray, double[,] directionsArray)
    {
        int height = pixelsArray.GetLength(0);
        int width = pixelsArray.GetLength(1);

        var supressedArray = new byte[height, width];
        for (int i = 0; i < height; i++)
            for (int j = 0; j < width; j++)
                supressedArray[i, j] = 0;

        var angles = GetAngles(directionsArray, repairNegativeAngles: false);  // Не исправляем отрицательные углы!

        // Здесь добавляем 180, т.к. направление тут не важно - важнен сам угол (превращаем обработку 8 углов в 4)
        // (через какие пиксели проходит условная линия, далее учитываем 4 варианта: вертикаль, горизонталь и две диагонали)
        for (int i = 0; i < angles.GetLength(0); i++)
            for (int j = 0; j < angles.GetLength(1); j++)
                if (angles[i, j] < 0)
                    angles[i, j] += 180;

        Parallel.For(0, height, y =>
        {
            for (int x = 0; x < width; x++)
            {
                int q = 255;
                int r = 255;

                if ((angles[y, x] is >= 0 and < 22.5) || (angles[y, x] is >= 157.5 and <= 180))  // Angle 0
                {
                    if (x + 1 >= width || x - 1 < 0)
                        continue;
                    q = pixelsArray[y, x + 1];
                    r = pixelsArray[y, x - 1];
                }
                else if (angles[y, x] is >= 22.5 and < 67.5)  // Angle 45
                {
                    if (y + 1 >= height || y - 1 < 0 || x + 1 >= width || x - 1 < 0)
                        continue;
                    q = pixelsArray[y + 1, x - 1];
                    r = pixelsArray[y - 1, x + 1];
                }
                else if (angles[y, x] is >= 67.5 and < 112.5)  // Angle 90
                {
                    if (y + 1 >= height || y - 1 < 0)
                        continue;
                    q = pixelsArray[y + 1, x];
                    r = pixelsArray[y - 1, x];
                }
                else if (angles[y, x] is >= 112.5 and < 157.5)  // Angle 135
                {
                    if (y + 1 >= height || y - 1 < 0 || x + 1 >= width || x - 1 < 0)
                        continue;
                    q = pixelsArray[y - 1, x - 1];
                    r = pixelsArray[y + 1, x + 1];
                }

                if (pixelsArray[y, x] >= q && pixelsArray[y, x] >= r)
                    supressedArray[y, x] = pixelsArray[y, x];
            }
        });

        return supressedArray;
    }

    // Прогон через двойной порог: выравнивает значения пикселей до 3 уровней (сильный, слабый, 0), где границы - преимущественно сильные пиксели
    private byte[,] DoubleThreshold(byte[,] pixelsArray, double downThreshold = 0.09, double upThreshold = 0.15)
    {
        int height = pixelsArray.GetLength(0);
        int width = pixelsArray.GetLength(1);
        var result = new byte[height, width];

        byte maxPixelValue = pixelsArray.Cast<byte>().Max();
        double highThresholdValue = maxPixelValue * upThreshold;
        double lowThresholdValue = highThresholdValue * downThreshold;

        Parallel.For(0, height, y =>
        {
            for (int x = 0; x < width; x++)
            {
                if (pixelsArray[y, x] >= highThresholdValue)
                    result[y, x] = _params.SharpnessCalcStrongPixel;
                else if (pixelsArray[y, x] < lowThresholdValue)
                    result[y, x] = 0;
                else
                    result[y, x] = _params.SharpnessCalcWeakPixel;
            }
        });

        return result;
    }

    // Гистерезис: попытка избавиться от слабых пикселей, переведя их в сильные или 0 в зависимости от пиксельного контекста в окрестности
    private byte[,] ApplyHysteresis(byte[,] pixelsArray)
    {
        int height = pixelsArray.GetLength(0);
        int width = pixelsArray.GetLength(1);
        var result = new byte[height, width];

        byte strongPixel = _params.SharpnessCalcStrongPixel;

        Parallel.For(0, height, y =>
        {
            for (int x = 0; x < width; x++)
            {
                result[y, x] = pixelsArray[y, x];
                if (pixelsArray[y, x] == _params.SharpnessCalcWeakPixel)
                {
                    bool strong = false;
                    for (int localY = Math.Max(y - 1, 0); localY <= Math.Min(y + 1, height - 1); localY++)
                        for (int localX = Math.Max(x - 1, 0); localX <= Math.Min(x + 1, width - 1); localX++)
                            if (pixelsArray[localY, localX] == strongPixel && !(localY == y && localX == x))
                            {
                                strong = true;
                                break;
                            }

                    if (strong)
                        result[y, x] = strongPixel;
                    else
                        result[y, x] = 0;
                }
            }
        });

        return result;
    }
}
