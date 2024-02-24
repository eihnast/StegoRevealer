using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Entities;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.ScMath;

namespace StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Calculators;

public class SharpnessCalculator
{
    private StatmParameters _params;

    public SharpnessCalculator(StatmParameters parameters)
    {
        _params = parameters;
    }


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
                { -1, 0, 1 },
                { -2, 0, 2 },
                { -1, 0, 1 } };
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

    // Переводит цветовой байт (RGB) в grayscale
    private byte ToGrayscaleByte(byte[] rgb, bool asAverage = true)
    {
        if (rgb.Length != 3)
            throw new Exception("RGB array must contains only 3 byte values!");

        byte result = 0;

        if (asAverage)
            result = MathMethods.Average(rgb);
        else
            result = PixelsTools.ToLimitedByte(rgb[0] * .21f + rgb[1] * .71f + rgb[2] * .071f);
        return result;
    }

    // Применяет оператор Собеля для выделения краевых пикселей и расчёта направлений градиента
    private SobelOperatorResult ApplySobelOperator(byte[,] pixelsMatrix, bool useScharrOperator = false)
    {
        int width = pixelsMatrix.GetLength(0);
        int height = pixelsMatrix.GetLength(1);

        // Применение оператора
        int kernelOffset = 1;  // Ширина окрестности пикселя для применения оператора
        var sharpnessImar = new byte[height, width];
        var gradientDirections = new double[height, width];

        double[,] yKernel = useScharrOperator ? yScharr : ySobel;
        double[,] xKernel = useScharrOperator ? xScharr : xSobel;

        for (int y = kernelOffset; y < height - kernelOffset; y++)
        {
            for (int x = kernelOffset; x < width - kernelOffset; x++)
            {
                double Gy = 0.0;
                double Gx = 0.0;

                for (int kernelY = -kernelOffset; kernelY <= kernelOffset; kernelY++)
                {
                    for (int kernelX = -kernelOffset; kernelX <= kernelOffset; kernelX++)
                    {
                        Gy += pixelsMatrix[y + kernelY, x + kernelX] * yKernel[kernelY + kernelOffset, kernelX + kernelOffset];
                        Gx += pixelsMatrix[y + kernelY, x + kernelX] * xKernel[kernelY + kernelOffset, kernelX + kernelOffset];
                    }
                }

                byte newByte = PixelsTools.ToLimitedByte(Math.Sqrt(Math.Pow(Gy, 2) + Math.Pow(Gx, 2)));  // Sqrt(Gy * Gy + Gx * Gx)
                sharpnessImar[y, x] = newByte;

                gradientDirections[y, x] = Math.Atan2(Gy, Gx);  //  * (180 / Math.PI)
            }
        }

        return new SobelOperatorResult
        {
            Intensity = sharpnessImar,
            Direction = gradientDirections
        };
    }


    // Формирует ядро фильтра Гаусса
    private double[,] GenerateGuassianKernel(int size, int sigma = 1)
    {
        var kernel = new double[size, size];

        int k = size / 2;  // size == 2k + 1
        double n = 1 / (2 * Math.PI * Math.Pow(sigma, 2));
        double down = 2 * Math.Pow(sigma, 2);

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                double up = Math.Pow((i + 1) - (k + 1), 2) + Math.Pow((j + 1) - (k + 1), 2);
                kernel[i, j] = n * Math.Exp(- (up / down));
            }
        }

        return kernel;
    }

    // Convolution
    private double[,] Convolution(double[,] values, double[,] kernel)
    {
        int height = values.GetLength(0);
        int width = values.GetLength(1);

        var result = new double[height, width];

        int kernelOffset = kernel.GetLength(0) / 2;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                double val = 0.0;

                for (int kernelY = 0; kernelY < kernel.GetLength(0); kernelY++) 
                {
                    for (int kernelX = 0; kernelX < kernel.GetLength(1); kernelX++)
                    {
                        int pixelY = y + kernelY - kernelOffset;
                        int pixelX = x + kernelX - kernelOffset;
                        if (pixelY >= height || pixelY < 0 || pixelX >= width || pixelX < 0)
                            continue;

                        val += values[y, x] * kernel[kernelY, kernelX];
                    }
                }

                result[y, x] = val;
            }
        }

        return result;
    }

    // Формирует матрицу углов в градусов из матрицы значений углов в радиантах
    private double[,] GetAngles(double[,] directions, bool repairNegativeAngles = true)
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
    private byte[,] NonMaximumSuppression(byte[,] pixelsArray, double[,] directionsArray)
    {
        int height = pixelsArray.GetLength(0);
        int width = pixelsArray.GetLength(1);

        var supressedArray = new byte[height, width];
        for (int i = 0; i < height; i++)
            for (int j = 0; j < width; j++)
                supressedArray[i, j] = 0;

        var angles = GetAngles(directionsArray, repairNegativeAngles: true);  // Не исправляем отрицательные углы!

        // Здесь добавляем 180, т.к. направление тут не важно - важнен сам угол (превращаем обработку 8 углов в 4)
        // (через какие пиксели проходит условная линия, далее учитываем 4 варианта: вертикаль, горизонталь и две диагонали)
        for (int i = 0; i < angles.GetLength(0); i++)
            for (int j = 0; j < angles.GetLength(1); j++)
                if (angles[i, j] < 0)
                    angles[i, j] += 180;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                try
                {
                    int q = 255;
                    int r = 255;

                    if ((angles[y, x] is >= 0 and < 22.5) || (angles[y, x] is >= 157.5 and <= 180))  // Angle 0
                    {
                        q = pixelsArray[y, x + 1];
                        r = pixelsArray[y, x - 1];
                    }
                    if (angles[y, x] is >= 22.5 and < 67.5)  // Angle 45
                    {
                        q = pixelsArray[y, x + 1];
                        r = pixelsArray[y, x - 1];
                    }
                    if (angles[y, x] is >= 67.5 and < 112.5)  // Angle 90
                    {
                        q = pixelsArray[y, x + 1];
                        r = pixelsArray[y, x - 1];
                    }
                    if (angles[y, x] is >= 112.5 and < 157.5)  // Angle 135
                    {
                        q = pixelsArray[y, x + 1];
                        r = pixelsArray[y, x - 1];
                    }

                    if (pixelsArray[y, x] >= q && pixelsArray[y, x] >= r)
                        supressedArray[y, x] = pixelsArray[y, x];
                }
                catch { }
            }
        }

        return supressedArray;
    }

    // Прогон через двойной порог: выравнивает значения пикселей до 3 уровней (сильный, слабый, 0), где границы - преимущественно сильные пиксели
    private byte[,] DoubleThreshold(byte[,] pixelsArray, double lowThreshold = 0.05, double highThreshold = 0.15)
    {
        int height = pixelsArray.GetLength(0);
        int width = pixelsArray.GetLength(1);
        var result = new byte[height, width];

        byte maxPixelValue = pixelsArray.Cast<byte>().Max();
        double highThresholdValue = maxPixelValue * highThreshold;
        double lowThresholdValue = highThresholdValue * lowThreshold;

        for (int y = 0; y < height; y++)
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
        }

        return result;
    }

    // Гистерезис: попытка избавиться от слабых пикселей, переведя их в сильные или 0 в зависимости от пиксельного контекста в окрестности
    private byte[,] ApplyHysteresis(byte[,] pixelsArray)
    {
        int height = pixelsArray.GetLength(0);
        int width = pixelsArray.GetLength(1);
        var result = new byte[height, width];

        byte strongPixel = _params.SharpnessCalcStrongPixel;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                result[y, x] = pixelsArray[y, x];
                if (pixelsArray[y, x] == _params.SharpnessCalcWeakPixel)
                {
                    try
                    {
                        if (pixelsArray[y + 1, x - 1] == strongPixel || pixelsArray[y + 1, x] == strongPixel || pixelsArray[y + 1, x + 1] == strongPixel ||
                            pixelsArray[y, x - 1] == strongPixel || pixelsArray[y, x + 1] == strongPixel || pixelsArray[y - 1, x - 1] == strongPixel ||
                            pixelsArray[y - 1, x] == strongPixel || pixelsArray[y - 1, x + 1] == strongPixel)
                            result[y, x] = strongPixel;
                        else
                            result[y, x] = 0;
                    }
                    catch { }
                }
            }
        }

        return result;
    }

    // Детектор определения границ Канни
    private CannyEdgeDetectionResult CannyEdgeDetection()
    {
        var imar = _params.Image.ImgArray;
        int width = imar.Width;
        int height = imar.Height;

        // Перевод в grayscale
        var gimar = new byte[height, width];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var fullByte = imar[y, x];
                var rgb = new byte[] { fullByte.Red, fullByte.Green, fullByte.Blue };
                var gbyte = ToGrayscaleByte(rgb, _params.SharpnessCalcUseAveragedGrayscale);
                gimar[y, x] = gbyte;
            }
        }

        // Noise Reduction. Применение фильтра Гаусса
        var guassKernel = GenerateGuassianKernel(_params.SharpnessCalcGuassianKernelSize, _params.SharpnessCalcGuassianKernelSigma);
        var blurredImar = PixelsTools.DoubleToByteMatrix(Convolution(PixelsTools.ByteToDoubleMatrix(gimar), guassKernel));

        // Gradient Calculation
        var sobelResult = ApplySobelOperator(blurredImar, _params.SharpnessCalcUseScharrOperator);

        // Non-Maximum Suppression
        var supressedImar = NonMaximumSuppression(sobelResult.Intensity, sobelResult.Direction);

        // Double threshold
        var processedImar = DoubleThreshold(supressedImar);

        // Edge Tracking by Hysteresis
        var finalImar = ApplyHysteresis(processedImar);


        return new CannyEdgeDetectionResult
        {
            EdgePixelsArray = finalImar,
            GrayscaledPixelsArray = gimar,
            SobelOperatorResult = sobelResult
        };
    }

    public double CalcSharpness()
    {
        var canny = CannyEdgeDetection();
        var edgesImar = canny.EdgePixelsArray;  // Матрица граничных пискелей по методу Канни
        var directionsImar = canny.SobelOperatorResult.Direction;  // Матрица направлений в радиантах
        var anglesImar = GetAngles(directionsImar, repairNegativeAngles: true);  // Получаем матрицу углов из значений направлений в радиантах
        var valuesArray = canny.GrayscaledPixelsArray;

        int height = edgesImar.GetLength(0);
        int width = edgesImar.GetLength(1);

        double maxSharpness = 0.0;

        for (int y = 0; y < height; y++)  // Обходим матрицу граничных пикселей
        {
            for (int x = 0; x < width; x++)
            {
                var currentPixel = edgesImar[y, x];
                if (currentPixel != 0)  // Если это граничный пиксель т.е. - пиксель weak или strong
                {
                    double k = Math.Tan(anglesImar[y, x]);  // y = kx, условно, с центром в центре текущего пикселя, k = тангенс угла из матрицы углов
                    var pixelsOnLine = new List<PixelInfo>();  // Какие пиксели попадают на прямую в направлении градиента (и обратном)

                    int maxOffset = _params.SharpnessCalcExtremumsNeighborhoodSize;  // Максимальное смещение = размеру окрестности, в пределах которой ищем экстремумы

                    // Сокращаем область поиска до прямоугольника, включающего пиксели прямой
                    double maxLocalY = k * (maxOffset + 0.5);
                    maxLocalY = maxLocalY > 0 ? Math.Min(maxLocalY, maxOffset + 0.5) : Math.Max(maxLocalY, - (maxOffset + 0.5));
                    double maxLocalX = maxLocalY / k;  // Реальное значение максимального x для прямой в рамках окна поиска

                    int maxOffsetX = (int)Math.Floor(maxLocalX - 0.5);
                    int maxOffsetY = (int)Math.Floor(Math.Abs(maxLocalY) - 0.5);

                    // Ищем пересечение прямой с границами пикселя
                    for (int localY = - maxOffsetY; localY <= maxOffsetY; localY++)
                    {
                        for (int localX = - maxOffsetX; localX <= maxOffsetX; localX++)
                        {
                            if (y + localY < 0 || y + localY >= height || x + localX < 0 || x + localX >= width)
                                continue;

                            double downCrossingX = (localY - 0.5) / k;
                            double upCrossingX = (localY + 0.5) / k;
                            double leftCrossingY = (localX - 0.5) * k;
                            double rightCrossingY = (localX + 0.5) * k;

                            // Если есть пересечение граней пикселя
                            if (localY - 0.5 <= leftCrossingY  && leftCrossingY  <= localY + 0.5 ||  // Левая грань
                                localY - 0.5 <= rightCrossingY && rightCrossingY <= localY + 0.5 ||  // Правая грань
                                localX - 0.5 <= downCrossingX  && downCrossingX  <= localX + 0.5 ||  // Нижняя грань
                                localX - 0.5 <= upCrossingX    && upCrossingX    <= localX + 0.5)    // Верхняя грань
                                pixelsOnLine.Add(new PixelInfo { Y = y + localY, X = x + localX, Value = valuesArray[y + localY, x + localX] });
                        }
                    }


                    // Ищем пиксели с минимальным и максимальным значением яркости на прямой и расстояние между ними
                    // Удаляем пиксели на линии, пересекающие другие границы
                    var anotherEdgePixels = pixelsOnLine.Where(p => p.Value == _params.SharpnessCalcStrongPixel && p.Y != y && p.X != x).ToList();
                    if (anotherEdgePixels.Count > 0)
                    {
                        var toRemove = new List<PixelInfo>();
                        foreach (var pixel in anotherEdgePixels)
                        {
                            if (!HasEdgeLineConnectionWithCenter(pixelsOnLine, pixel, y, x))
                                toRemove.AddRange(
                                    pixelsOnLine.Where(p => p == pixel || y - pixel.Y > 0 ? p.Y < pixel.Y : p.Y > pixel.Y || x - pixel.X > 0 ? p.X < pixel.X : p.X > pixel.X));
                        }

                        pixelsOnLine = pixelsOnLine.Where(p => !toRemove.Contains(p)).ToList();
                    }

                    // Убираем центральный краевой пиксель из последовательности
                    pixelsOnLine = pixelsOnLine.Where(p => p.Y != y && p.X != x).ToList();

                    // Вычисления
                    byte min = pixelsOnLine.Min(p => p.Value);  // a
                    byte max = pixelsOnLine.Max(p => p.Value);  // b

                    var minPixel = pixelsOnLine.Where(p => p.Value == min).OrderBy(p => Math.Sqrt(Math.Pow(p.Y - y, 2) + Math.Pow(p.X - x, 2))).First();
                    var maxPixel = pixelsOnLine.Where(p => p.Value == max).OrderBy(p => Math.Sqrt(Math.Pow(p.Y - y, 2) + Math.Pow(p.X - x, 2))).First();

                    double distance = Math.Sqrt(Math.Pow(minPixel.Y - maxPixel.Y, 2) + Math.Pow(minPixel.X - maxPixel.X, 2));  // w

                    double sharpness = Math.Abs(max - min) / distance;
                    if (sharpness > maxSharpness)
                        maxSharpness = sharpness;
                }
            }
        }

        return maxSharpness;
    }

    private bool HasEdgeLineConnectionWithCenter(List<PixelInfo> pixels, PixelInfo pixel, int centerY, int centerX, List<PixelInfo>? pixelsFrom = null)
    {
        var neib = pixels.Where(np => Math.Abs(np.X - pixel.X) <= 1 && Math.Abs(np.Y - pixel.Y) <= 1 && np != pixel && (pixelsFrom is null || !pixelsFrom.Contains(np)))
            .Where(p => p.Value == _params.SharpnessCalcStrongPixel || p.Value == _params.SharpnessCalcWeakPixel).ToList();

        if (neib.Any(p => p.Y == centerY && p.X == centerX))
            return true;

        if (pixelsFrom is null)
            pixelsFrom = new List<PixelInfo>() { pixel };
        else
            pixelsFrom.Add(pixel);

        bool result = false;
        foreach (var npixel in neib)
            result |= HasEdgeLineConnectionWithCenter(pixels, npixel, centerY, centerX, pixelsFrom);

        return result;
    }
}
