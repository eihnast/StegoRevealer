using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevealer.StegoCore.StegoMethods.Lsb
{
    /*
     * Случайный обход не учитывает стартовые индексы цветовых байт.
     *   Однако отличается от чрезканальности. Если включена чрезканальность, линейный массив индексов выглядит так:
     *     Px1-R, Px1-G, Px1-B, Px2-R, Px2-G, Px2-B, ..., PxN-R, PxN-G, PxN-B
     *   Если выключена чрезканальность, линейный массив индексов выглядит так:
     *     Px1-R, Px2-R, ..., PxN-R, Px1-G, Px2-G, ..., PxN-G, Px1-B, Px2-B, ..., PxN-B
     *   Важно отметить: сам по себе линейный массив индексов всегда одинаков (массив длины = кол-ву всех цветовых байт),
     *     однако различна обработка выбранного из этого массива индекса цветового байта.
     * Последовательный обход использует такое же представление линейного массива индексов, однако учитывает
     *   стартовые индексы.
     * Общий линейный индекс: полностью уникальный индекс цветового байта (о.л.и. - у цветового байта)
     * Линейный индекс: индекс пикселя в сведённой к линейному списку матрице пикселей одного канала (л.и. - у пикселя)
     */

    public static class LsbCommon
    {
        // Общие методы

        // Возвращает координаты (индексы) пикселя в матрице Width x Heigth по его линейному индексу
        public static Sc2DPoint GetPixelCoordsByIndex(int index, LsbParameters parameters)
        {
            if (!parameters.VerticalHiding)
            {
                int line = index / parameters.Image.ImgArray.Width;
                int column = index % parameters.Image.ImgArray.Width;
                return new Sc2DPoint(line, column);
            }
            else
            {
                int line = index / parameters.Image.ImgArray.Height;
                int column = index % parameters.Image.ImgArray.Height;
                return new Sc2DPoint(line, column);
            }
        }


        // Последовательная итерация

        // Возвращает следующий набор индексов для доступа к байту цвета пикселя определённого канала
        //   при последовательном скрытии
        public static IEnumerable<ScPointCoords> GetForLinearAccessIndex(int usingColorBytesNum, LsbParameters parameters)
        {
            int overallCount = 0;

            // Стартовые линейные индексы пикселей
            int[] indexes = new int[parameters.Channels.Count];
            for (int i = 0; i < parameters.Channels.Count; i++)
                indexes[i] = parameters.StartPixels[i];

            if (!parameters.InterlaceChannels)  // Поканально
            {
                for (int k = 0; k < parameters.Channels.Count && overallCount <= usingColorBytesNum; k++)
                {
                    var (line, column) = GetPixelCoordsByIndex(indexes[k], parameters).AsTuple();
                    overallCount++;
                    yield return new ScPointCoords(line, column, (int)parameters.Channels[k]);
                    indexes[k]++;
                }

                yield break;
            }
            else  // Чересканально
            {
                while (overallCount <= usingColorBytesNum)
                {
                    for (int k = 0; k < parameters.Channels.Count && overallCount <= usingColorBytesNum; k++)
                    {
                        var (line, column) = GetPixelCoordsByIndex(indexes[k], parameters).AsTuple();
                        overallCount++;
                        yield return new ScPointCoords(line, column, (int)parameters.Channels[k]);
                        indexes[k]++;
                    }
                }

                yield break;
            }
        }


        // Псевдослучайная итерация

        // Возвращает индексы доступа к конкретному цветовому байту пикселя по общему линейному индексу
        public static (int, int, int) GetImgByteIndexesFromLinearIndex(int index, LsbParameters parameters)
        {
            // Вычисление происходит в зависимости от: обхода по матрице, чересканальности

            // Определение индекса канала и линейного индекса пикселя
            int channel;
            int pixelLinearIndex;
            if (parameters.InterlaceChannels)  // Чередование каналов (Pixel1 {R;G;B} --> 0,1,2)
            {
                channel = (int)parameters.Channels[index % parameters.Channels.Count];
                pixelLinearIndex = Convert
                    .ToInt32(Math.Round((decimal)index / parameters.Channels.Count, MidpointRounding.ToPositiveInfinity));
            }
            else  // Поканально (R: Pixel1, Pixel2, Pixel3 --> 0,1,2)
            {
                var (w, h, d) = parameters.Image.GetImgSizes();
                int channelInnerIndex = index / (w * h);  // Индекс канала в списке используемых каналов
                channel = (int)parameters.Channels[channelInnerIndex];  // Реальный индекс канала (Red - 0 и т.д.)
                pixelLinearIndex = index - channelInnerIndex * (w * h);  // Получаем линейный индекс пикселя
            }

            var (line, column) = GetPixelCoordsByIndex(pixelLinearIndex, parameters).AsTuple();
            return (line, column, channel);
        }

        // Возвращает следующий набор индексов для доступа к байту цвета пикселя определённого канала
        //   при псевдослучайном скрытии
        public static IEnumerable<ScPointCoords> GetForRandomAccessIndex(int usingColorBytesNum, LsbParameters parameters)
        {
            var rnd = parameters.Seed.HasValue ? new Random(parameters.Seed.Value) : new Random();

            var (width, height, depth) = parameters.Image.GetImgSizes();
            int imgLinearLength = width * height * parameters.Channels.Count;  // Общее число цветовых байт изображения
            if (usingColorBytesNum < 0)
                usingColorBytesNum = imgLinearLength;

            // Массив общих линейных индексов цветовых байт
            var allLinearIndexes = Enumerable.Range(0, imgLinearLength).ToArray();  // Формирование
            allLinearIndexes = allLinearIndexes.OrderBy(e => rnd.Next()).ToArray();  // Перемешивание

            for (int i = 0; i < Math.Min(usingColorBytesNum, imgLinearLength); i++)
            {
                var (y, x, channel) = GetImgByteIndexesFromLinearIndex(allLinearIndexes[i], parameters);
                yield return new ScPointCoords(y, x, channel);
            }

            yield break;
        }
    }
}
