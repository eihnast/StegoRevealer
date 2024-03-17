using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Entities;
using StegoRevealer.StegoCore.CommonLib;

namespace StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Calculators;

public class SharpnessCalculator
{
    private StatmParameters _params;

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

        for (int y = 0; y < height; y++)  // Обходим матрицу граничных пикселей
        {
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
                    if (k == 0)
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
                            throw new Exception($"Line coordinates error with: k = {k}, yForMaxX = {yForMaxX}, xForMaxY = {xForMaxY}");

                        var firstPixel = new PixelInfo() { Y = y + pixelOffset.Y, X = x + pixelOffset.X };
                        var secondPixel = new PixelInfo() { Y = y - pixelOffset.Y, X = x - pixelOffset.X };

                        if (firstPixel.Y > height - 1 || firstPixel.Y < 0 || firstPixel.X > width - 1 || firstPixel.X < 0)
                            firstPixel = new PixelInfo { Y = 0, X = 0 };
                        if (secondPixel.Y > height - 1 || secondPixel.Y < 0 || secondPixel.X > width - 1 || secondPixel.X < 0)
                            secondPixel = new PixelInfo { Y = 0, X = 0 };

                        // Применяем алгоритм Брезенхэма
                        pixelsOnLine = PixelsTools.GetPixelsOnLine(firstPixel.Y, firstPixel.X, secondPixel.Y, secondPixel.X);
                    }

                    // Убираем центральный краевой пиксель из последовательности
                    pixelsOnLine = pixelsOnLine.Where(p => !(p.Y == y && p.X == x)).ToList();

                    // Обогащаем значениями пикселей
                    for (int i = 0; i < pixelsOnLine.Count; i++)
                        pixelsOnLine[i].Value = gimar[pixelsOnLine[i].Y, pixelsOnLine[i].X];

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

    // Детектор определения границ Канни
    public CannyEdgeDetectionResult CannyEdgeDetection()
    {
        var imar = _params.Image.ImgArray;
        var gimar = PixelsTools.ToGrayscale(imar, _params.SharpnessCalcUseAveragedGrayscale);

        // Noise Reduction. Применение фильтра Гаусса
        var guassKernel = GenerateGuassianKernel(_params.SharpnessCalcGuassianKernelSize, _params.SharpnessCalcGuassianKernelSigma);
        var blurredImar = PixelsTools.DoubleToByteMatrix(Convolution(PixelsTools.ByteToDoubleMatrix(gimar), guassKernel));

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

        for (int y = 0; y < height; y++)
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
        }

        return new SobelOperatorResult
        {
            Intensity = sharpnessImar,
            Direction = gradientDirections
        };
    }


    // Формирует ядро фильтра Гаусса
    private double[,] GenerateGuassianKernel(int size, double sigma = 1.0)
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
                kernel[i, j] = n * Math.Exp(-(up / down));
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

        var angles = GetAngles(directionsArray, repairNegativeAngles: false);  // Не исправляем отрицательные углы!

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
                    else if (angles[y, x] is >= 22.5 and < 67.5)  // Angle 45
                    {
                        q = pixelsArray[y + 1, x - 1];
                        r = pixelsArray[y - 1, x + 1];
                    }
                    else if (angles[y, x] is >= 67.5 and < 112.5)  // Angle 90
                    {
                        q = pixelsArray[y + 1, x];
                        r = pixelsArray[y - 1, x];
                    }
                    else if (angles[y, x] is >= 112.5 and < 157.5)  // Angle 135
                    {
                        q = pixelsArray[y - 1, x - 1];
                        r = pixelsArray[y + 1, x + 1];
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
    private byte[,] DoubleThreshold(byte[,] pixelsArray, double downThreshold = 0.09, double upThreshold = 0.15)
    {
        int height = pixelsArray.GetLength(0);
        int width = pixelsArray.GetLength(1);
        var result = new byte[height, width];

        byte maxPixelValue = pixelsArray.Cast<byte>().Max();
        double highThresholdValue = maxPixelValue * upThreshold;
        double lowThresholdValue = highThresholdValue * downThreshold;

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
        }

        return result;
    }

    #region Helper

    [Obsolete("Рекурсивный метод поиска соединённых с центром пикселей, не используется")]
    private bool HasEdgeLineConnectionWithCenter(List<PixelInfo> pixels, PixelInfo pixel, int centerY, int centerX, byte[,] edges, List<PixelInfo>? pixelsFrom = null)
    {
        var neib = pixels.Where(np => Math.Abs(np.X - pixel.X) <= 1 && Math.Abs(np.Y - pixel.Y) <= 1 && np != pixel && (pixelsFrom is null || !pixelsFrom.Contains(np)))
            .Where(p => edges[p.Y, p.X] == _params.SharpnessCalcStrongPixel || edges[p.Y, p.X] == _params.SharpnessCalcWeakPixel).ToList();

        if (neib.Any(p => p.Y == centerY && p.X == centerX))
            return true;

        if (pixelsFrom is null)
            pixelsFrom = new List<PixelInfo>() { pixel };
        else
            pixelsFrom.Add(pixel);

        bool result = false;
        foreach (var npixel in neib)
            result |= HasEdgeLineConnectionWithCenter(pixels, npixel, centerY, centerX, edges, pixelsFrom);

        return result;
    }

    #endregion

    #region ChatGPT realisation

    [Obsolete]
    public class ChatGPTCannyEdgeDetector
    {
        // Параметры фильтра Гаусса
        private const int GaussKernelSize = 5;
        private const double Sigma = 1.4;

        public byte[,] DetectEdges(byte[,] image)
        {
            int width = image.GetLength(0);
            int height = image.GetLength(1);

            // Этап 1: Сглаживание изображения фильтром Гаусса
            double[,] gaussKernel = CreateGaussianKernel(GaussKernelSize, Sigma);
            byte[,] smoothed = Convolve(image, gaussKernel);

            // Этап 2: Нахождение градиентов изображения
            (double[,] gradientMagnitude, double[,] gradientDirection) = FindGradients(smoothed);

            // Этап 3: Немаксимальное подавление
            byte[,] nonMaxSuppressed = NonMaximumSuppression(gradientMagnitude, gradientDirection);

            // Этап 4: Двойная пороговая фильтрация и трассировка границ по гистерезису
            byte[,] edges = HysteresisThresholding(nonMaxSuppressed);

            return edges;
        }

        private double[,] CreateGaussianKernel(int size, double sigma)
        {
            double[,] kernel = new double[size, size];
            double mean = size / 2;
            double sum = 0.0; // Аккумуляция всех весов элементов ядра
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    kernel[x, y] = Math.Exp(-0.5 * (Math.Pow((x - mean) / sigma, 2.0) + Math.Pow((y - mean) / sigma, 2.0))) / (2 * Math.PI * sigma * sigma);
                    sum += kernel[x, y];
                }
            }
            // Нормализация ядра
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    kernel[x, y] /= sum;
                }
            }
            return kernel;
        }

        // Данный метод должен быть реализован для выполнения свертки изображения с ядром Гаусса
        private byte[,] Convolve(byte[,] image, double[,] kernel)
        {
            int width = image.GetLength(0);
            int height = image.GetLength(1);
            int kernelRadius = kernel.GetLength(0) / 2;
            byte[,] result = new byte[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    double sum = 0.0;

                    for (int kx = -kernelRadius; kx <= kernelRadius; kx++)
                    {
                        for (int ky = -kernelRadius; ky <= kernelRadius; ky++)
                        {
                            int px = Math.Min(Math.Max(x + kx, 0), width - 1);
                            int py = Math.Min(Math.Max(y + ky, 0), height - 1);
                            sum += image[px, py] * kernel[kx + kernelRadius, ky + kernelRadius];
                        }
                    }

                    result[x, y] = (byte)Math.Max(0, Math.Min(255, sum));
                }
            }

            return result;
        }

        // Реализуйте этот метод для нахождения градиента и его направления
        private (double[,], double[,]) FindGradients(byte[,] image)
        {
            int width = image.GetLength(0);
            int height = image.GetLength(1);
            double[,] gradientMagnitude = new double[width, height];
            double[,] gradientDirection = new double[width, height];

            // Операторы Собеля
            int[,] gx = new int[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            int[,] gy = new int[,] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };

            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    double gradX = 0.0;
                    double gradY = 0.0;

                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            gradX += image[x + i, y + j] * gx[i + 1, j + 1];
                            gradY += image[x + i, y + j] * gy[i + 1, j + 1];
                        }
                    }

                    gradientMagnitude[x, y] = Math.Sqrt(gradX * gradX + gradY * gradY);
                    gradientDirection[x, y] = Math.Atan2(gradY, gradX);
                }
            }

            return (gradientMagnitude, gradientDirection);
        }

        // Реализуйте этот метод для немаксимального подавления
        private byte[,] NonMaximumSuppression(double[,] gradientMagnitude, double[,] gradientDirection)
        {
            int width = gradientMagnitude.GetLength(0);
            int height = gradientMagnitude.GetLength(1);
            byte[,] result = new byte[width, height];

            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    double direction = gradientDirection[x, y] * (180 / Math.PI);
                    direction = (direction < 0) ? direction + 180 : direction;

                    double q = 255;
                    double r = 255;

                    if ((0 <= direction && direction < 22.5) || (157.5 <= direction && direction <= 180))
                    {
                        q = gradientMagnitude[x, y - 1];
                        r = gradientMagnitude[x, y + 1];
                    }
                    else if (22.5 <= direction && direction < 67.5)
                    {
                        q = gradientMagnitude[x + 1, y - 1];
                        r = gradientMagnitude[x - 1, y + 1];
                    }
                    else if (67.5 <= direction && direction < 112.5)
                    {
                        q = gradientMagnitude[x + 1, y];
                        r = gradientMagnitude[x - 1, y];
                    }
                    else if (112.5 <= direction && direction < 157.5)
                    {
                        q = gradientMagnitude[x - 1, y - 1];
                        r = gradientMagnitude[x + 1, y + 1];
                    }

                    if (gradientMagnitude[x, y] >= q && gradientMagnitude[x, y] >= r)
                    {
                        result[x, y] = (byte)Math.Min(255, gradientMagnitude[x, y]);
                    }
                    else
                    {
                        result[x, y] = 0;
                    }
                }
            }

            return result;
        }

        // Реализуйте этот метод для двойной пороговой фильтрации и трассировки границ по гистерезису
        private byte[,] HysteresisThresholding(byte[,] image)
        {
            int width = image.GetLength(0);
            int height = image.GetLength(1);
            byte[,] result = new byte[width, height];
            const byte strongEdge = 255;
            const byte weakEdge = 25;
            double highThreshold = 75;
            double lowThreshold = 30;

            // Определение сильных и слабых границ
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (image[x, y] >= highThreshold)
                    {
                        result[x, y] = strongEdge;
                    }
                    else if (image[x, y] >= lowThreshold)
                    {
                        result[x, y] = weakEdge;
                    }
                }
            }

            // Преобразование слабых границ в сильные, если они соединены с сильными
            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    if (result[x, y] == weakEdge)
                    {
                        if (result[x + 1, y] == strongEdge || result[x - 1, y] == strongEdge ||
                            result[x, y + 1] == strongEdge || result[x, y - 1] == strongEdge ||
                            result[x + 1, y + 1] == strongEdge || result[x - 1, y - 1] == strongEdge ||
                            result[x + 1, y - 1] == strongEdge || result[x - 1, y + 1] == strongEdge)
                        {
                            result[x, y] = strongEdge;
                        }
                        else
                        {
                            result[x, y] = 0;
                        }
                    }
                }
            }

            return result;
        }
    }

    #endregion
}
