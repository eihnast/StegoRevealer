using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.StegoMethods;
using StegoRevealer.StegoCore.StegoMethods.Lsb;
using System.Collections;

namespace StegoRevealer.StegoCore.ModuleTests
{
    [TestClass]
    public class PixelsTraversingTests
    {
        // Фактически, тесты класса LsbCommon

        // Общее для тестов изображение: загружаем 1 раз
        private static string TestImagePath = Path.Combine(Helper.GetAssemblyDir(), "TestData", "TraverseTestImage.png");
        private static ImageHandler TestImage = new ImageHandler(TestImagePath);

        private const int TestImageWidth = 20;
        private const int TestImageHeight = 10;
        private const int TestImageColorBytes = TestImageWidth * TestImageHeight * 3;

        // Тестовое изображение:
        // Слева направо и сверху вниз идут друг за другом пиксели одного из следующих цветов:
        //     чёрный,  красный,   синий,     зелёный,   белый,         жёлтый,      розовый,     лазоревый
        //     (0,0,0), (255,0,0), (0,0,255), (0,255,0), (255,255,255), (255,255,0), (255,0,255), (0,255,255)


        [TestMethod]
        public void CheckDirectTraverse_Horizontal_Interlacing()
        {
            var parameters = new LsbParameters(TestImage);
            parameters.InterlaceChannels = true;
            parameters.TraverseType = CommonLib.TraverseType.Horizontal;

            // Храним значения всех цветовых байт в порядке их получения итератором
            var allColorBytes = new byte[TestImageColorBytes];
            int k = 0;

            Func<int, LsbParameters, IEnumerable<ScPointCoords>> iterator = LsbCommon.GetForLinearAccessIndex;
            foreach (var colorByteIndexes in iterator(TestImageColorBytes, parameters))
            {
                var pixel = TestImage.GetPixelValue(colorByteIndexes.X, colorByteIndexes.Y);
                allColorBytes[k] = pixel[colorByteIndexes.ChannelId];
                k++;
            }

            // Сверяем избранные значения для проверки последовательности итератора (горизонтальный, чересканально)
            // Индекс красного (первого по порядку) цветового байта пикселя = (строка - 1) * 60 + (столбец - 1) * 3
            var expectedValues = new Dictionary<int, byte>()  // key-тый полученный цветовой байт - это байт со значением value
            {
                { 0, 0 }, { 1, 0 }, { 2, 0 },  // 1-й пиксель (чёрный)
                { 3, 0 }, { 4, 0 }, { 5, 255 },  // 2-й пиксель (синий)
                { 6, 255 }, { 7, 0 }, { 8, 0 },  // 3-й пиксель (красный)
                { 60, 0 }, { 61, 255 }, { 62, 0 },  // 21-й пиксель (1 во 2 строке) (зелёный)
                { 120, 255 }, { 121, 255 }, { 122, 255 },  // 31-й пиксель (1 в 3 строке) (белый)
                { 180, 255 }, { 181, 0 }, { 182, 0 },  // 41-й пиксель (1 в 4 строке) (красный)
                { 387, 0 }, { 388, 255 }, { 389, 255 },  // N-й пиксель (10 в 7 строке) (лазоревый)
                { 213, 255 }, { 214, 0 }, { 215, 255 },  // N-й пиксель (12 в 4 строке) (розовый)
                { 417, 255 }, { 418, 255 }, { 419, 0 },  // N-й пиксель (20 в 7 строке) (жёлтый)
                { 597, 255 }, { 598, 0 }, { 599, 0 },  // N-й пиксель (20 в 10 строке) (красный)
                { 537, 0 }, { 538, 255 }, { 539, 255 },  // N-й пиксель (20 в 9 строке) (лазоревый)
                { 594, 0 }, { 595, 255 }, { 596, 255 },  // N-й пиксель (19 в 10 строке) (лазоревый)
            };

            foreach (var expectedValue in expectedValues)
                Assert.AreEqual(allColorBytes[expectedValue.Key], expectedValue.Value, 
                    $"Для цветового байта под индексом {expectedValue.Key} не получен ожидаемый результат");
        }

        [TestMethod]
        public void CheckDirectTraverse_Horizontal_NotInterlacing()
        {
            var parameters = new LsbParameters(TestImage);
            parameters.InterlaceChannels = false;
            parameters.TraverseType = CommonLib.TraverseType.Horizontal;

            // Храним значения всех цветовых байт в порядке их получения итератором
            var allColorBytes = new byte[TestImageColorBytes];
            int k = 0;

            Func<int, LsbParameters, IEnumerable<ScPointCoords>> iterator = LsbCommon.GetForLinearAccessIndex;
            foreach (var colorByteIndexes in iterator(TestImageColorBytes, parameters))
            {
                var pixel = TestImage.GetPixelValue(colorByteIndexes.X, colorByteIndexes.Y);
                allColorBytes[k] = pixel[colorByteIndexes.ChannelId];
                k++;
            }

            // Сверяем избранные значения для проверки последовательности итератора (горизонтальный, поканально)
            // Индекс для красного цветового байта: (строка - 1) * 20 + (столбец - 1)
            // Индекс для зелёного цветового байта: индекс для красного + 200
            // Индекс для синего цветового байта: индекс для красного + 400
            var expectedValues = new Dictionary<int, byte>()  // key-тый полученный цветовой байт - это байт со значением value
            {
                { 0, 0 }, { 200, 0 }, { 400, 0 },  // 1-й пиксель (чёрный)
                { 1, 0 }, { 201, 0 }, { 401, 255 },  // 2-й пиксель (синий)
                { 2, 255 }, { 202, 0 }, { 402, 0 },  // 3-й пиксель (красный)
                { 20, 0 }, { 220, 255 }, { 420, 0 },  // 21-й пиксель (1 во 2 строке) (зелёный)
                { 40, 255 }, { 240, 255 }, { 440, 255 },  // 31-й пиксель (1 в 3 строке) (белый)
                { 60, 255 }, { 260, 0 }, { 460, 0 },  // 41-й пиксель (1 в 4 строке) (красный)
                { 129, 0 }, { 329, 255 }, { 529, 255 },  // N-й пиксель (10 в 7 строке) (лазоревый)
                { 71, 255 }, { 271, 0 }, { 471, 255 },  // N-й пиксель (12 в 4 строке) (розовый)
                { 139, 255 }, { 339, 255 }, { 539, 0 },  // N-й пиксель (20 в 7 строке) (жёлтый)
                { 199, 255 }, { 399, 0 }, { 599, 0 },  // N-й пиксель (20 в 10 строке) (красный)
                { 179, 0 }, { 379, 255 }, { 579, 255 },  // N-й пиксель (20 в 9 строке) (лазоревый)
                { 198, 0 }, { 398, 255 }, { 598, 255 },  // N-й пиксель (19 в 10 строке) (лазоревый)
            };

            foreach (var expectedValue in expectedValues)
                Assert.AreEqual(allColorBytes[expectedValue.Key], expectedValue.Value,
                    $"Для цветового байта под индексом {expectedValue.Key} не получен ожидаемый результат");
        }

        [TestMethod]
        public void CheckDirectTraverse_Vertical_Interlacing()
        {
            var parameters = new LsbParameters(TestImage);
            parameters.InterlaceChannels = true;
            parameters.TraverseType = CommonLib.TraverseType.Vertical;

            // Храним значения всех цветовых байт в порядке их получения итератором
            var allColorBytes = new byte[TestImageColorBytes];
            int k = 0;

            Func<int, LsbParameters, IEnumerable<ScPointCoords>> iterator = LsbCommon.GetForLinearAccessIndex;
            foreach (var colorByteIndexes in iterator(TestImageColorBytes, parameters))
            {
                var pixel = TestImage.GetPixelValue(colorByteIndexes.X, colorByteIndexes.Y);
                allColorBytes[k] = pixel[colorByteIndexes.ChannelId];
                k++;
            }

            // Сверяем избранные значения для проверки последовательности итератора (вертикальный, чересканально)
            // Индекс красного (первого по порядку) цветового байта пикселя = (столбец - 1) * 30 + (строка - 1) * 3
            var expectedValues = new Dictionary<int, byte>()  // key-тый полученный цветовой байт - это байт со значением value
            {
                { 0, 0 }, { 1, 0 }, { 2, 0 },  // 1-й пиксель (чёрный)
                { 3, 0 }, { 4, 255 }, { 5, 0 },  // 2-й пиксель (зелёный)
                { 6, 255 }, { 7, 255 }, { 8, 255 },  // 3-й пиксель (белый)
                { 30, 0 }, { 31, 0 }, { 32, 255 },  // 21-й пиксель (1 во 2 столбце) (синий)
                { 60, 255 }, { 61, 0 }, { 62, 0 },  // 21-й пиксель (1 в 3 столбце) (красный)
                { 90, 255 }, { 91, 0 }, { 92, 255 },  // 31-й пиксель (1 в 4 столбце) (розовый)
                { 288, 0 }, { 289, 255 }, { 290, 255 },  // N-й пиксель (10 столбец, 7 строка) (лазоревый)
                { 339, 255 }, { 340, 0 }, { 341, 255 },  // N-й пиксель (12 столбец, 4 строка) (розовый)
                { 588, 255 }, { 589, 255 }, { 590, 0 },  // N-й пиксель (20 столбец, 7 строка) (жёлтый)
                { 597, 255 }, { 598, 0 }, { 599, 0 },  // N-й пиксель (20 столбец, 10 строка) (красный)
                { 594, 0 }, { 595, 255 }, { 596, 255 },  // N-й пиксель (20 столбец, 9 строка) (лазоревый)
                { 567, 0 }, { 568, 255 }, { 569, 255 },  // N-й пиксель (19 столбец, 10 строка) (лазоревый)
            };

            foreach (var expectedValue in expectedValues)
                Assert.AreEqual(allColorBytes[expectedValue.Key], expectedValue.Value,
                    $"Для цветового байта под индексом {expectedValue.Key} не получен ожидаемый результат");
        }

        [TestMethod]
        public void CheckDirectTraverse_Vertical_NotInterlacing()
        {
            var parameters = new LsbParameters(TestImage);
            parameters.InterlaceChannels = false;
            parameters.TraverseType = CommonLib.TraverseType.Vertical;

            // Храним значения всех цветовых байт в порядке их получения итератором
            var allColorBytes = new byte[TestImageColorBytes];
            int k = 0;

            Func<int, LsbParameters, IEnumerable<ScPointCoords>> iterator = LsbCommon.GetForLinearAccessIndex;
            foreach (var colorByteIndexes in iterator(TestImageColorBytes, parameters))
            {
                var pixel = TestImage.GetPixelValue(colorByteIndexes.X, colorByteIndexes.Y);
                allColorBytes[k] = pixel[colorByteIndexes.ChannelId];
                k++;
            }

            // Сверяем избранные значения для проверки последовательности итератора (вертикальный, поканально)
            // Индекс для красного цветового байта: (столбец - 1) * 10 + (строка - 1)
            // Индекс для зелёного цветового байта: индекс для красного + 200
            // Индекс для синего цветового байта: индекс для красного + 400
            var expectedValues = new Dictionary<int, byte>()  // key-тый полученный цветовой байт - это байт со значением value
            {
                { 0, 0 }, { 200, 0 }, { 400, 0 },  // 1-й пиксель (чёрный)
                { 1, 0 }, { 201, 255 }, { 401, 0 },  // 2-й пиксель (зелёный)
                { 2, 255 }, { 202, 255 }, { 402, 255 },  // 3-й пиксель (белый)
                { 10, 0 }, { 210, 0 }, { 410, 255 },  // 21-й пиксель (1 во 2 столбце) (синий)
                { 20, 255 }, { 220, 0 }, { 420, 0 },  // 21-й пиксель (1 в 3 столбце) (красный)
                { 30, 255 }, { 230, 0 }, { 430, 255 },  // 31-й пиксель (1 в 4 столбце) (розовый)
                { 96, 0 }, { 296, 255 }, { 496, 255 },  // N-й пиксель (10 столбец, 7 строка) (лазоревый)
                { 113, 255 }, { 313, 0 }, { 513, 255 },  // N-й пиксель (12 столбец, 4 строка) (розовый)
                { 196, 255 }, { 396, 255 }, { 596, 0 },  // N-й пиксель (20 столбец, 7 строка) (жёлтый)
                { 199, 255 }, { 399, 0 }, { 599, 0 },  // N-й пиксель (20 столбец, 10 строка) (красный)
                { 198, 0 }, { 398, 255 }, { 598, 255 },  // N-й пиксель (20 столбец, 9 строка) (лазоревый)
                { 189, 0 }, { 389, 255 }, { 589, 255 },  // N-й пиксель (19 столбец, 10 строка) (лазоревый)
            };

            foreach (var expectedValue in expectedValues)
                Assert.AreEqual(allColorBytes[expectedValue.Key], expectedValue.Value,
                    $"Для цветового байта под индексом {expectedValue.Key} не получен ожидаемый результат");
        }

        [TestMethod]
        public void CheckRandomTraverse_Horizontal_Interlacing()
        {
            var parameters = new LsbParameters(TestImage);
            parameters.InterlaceChannels = true;
            parameters.TraverseType = CommonLib.TraverseType.Horizontal;
            parameters.Seed = 13378;  // О.л.и. после перемешивания: 133, 233, 336, 6, 267, 523, 536, 499, 212, 453, 307, 485, 442, 184, 582

            // Храним значения всех цветовых байт в порядке их получения итератором
            var allColorBytes = new byte[TestImageColorBytes];
            int k = 0;

            Func<int, LsbParameters, IEnumerable<ScPointCoords>> iterator = LsbCommon.GetForRandomAccessIndex;
            foreach (var colorByteIndexes in iterator(TestImageColorBytes, parameters))
            {
                var pixel = TestImage.GetPixelValue(colorByteIndexes.X, colorByteIndexes.Y);
                allColorBytes[k] = pixel[colorByteIndexes.ChannelId];
                k++;
            }

            // Сверяем избранные значения для проверки последовательности итератора (горизонтальный, чересканально)
            // Индекс красного (первого по порядку) цветового байта пикселя = (строка - 1) * 60 + (столбец - 1) * 3
            // Определение координат по ОЛИ:
            // канал = ОЛИ % 3
            // строка = ОЛИ / 60 + 1 (индекс строки без +1);
            // столбец = (ОЛИ - (строка - 1) * 60) / 3 + 1 (индекс столбца без +1).
            // Пример: 133 - это Green(index: 1), 3 строка, 5 столбец
            var expectedValues = new int[] { 0, 255, 0, 255, 0, 255, 0, 0, 255, 0, 0, 0, 255, 255, 255 };

            // Преобразование: общий линейный индекс (ОЛИ) --> линейный индекс (ЛИ):
            // *Чересканально: ОЛИ / 3
            // Поканально: ОЛИ - (ОЛИ / (кол-во пикселей)) * (кол-во пикселей)

            for (int i = 0; i < expectedValues.Length; i++)
                Assert.AreEqual(allColorBytes[i], expectedValues[i],
                    $"Для цветового байта под индексом {i} не получен ожидаемый результат");
        }

        [TestMethod]
        public void CheckRandomTraverse_Horizontal_NotInterlacing()
        {
            var parameters = new LsbParameters(TestImage);
            parameters.InterlaceChannels = false;
            parameters.TraverseType = CommonLib.TraverseType.Horizontal;
            parameters.Seed = 13378;  // О.л.и. после перемешивания: 133, 233, 336, 6, 267, 523, 536, 499, 212, 453, 307, 485, 442, 184, 582

            // Храним значения всех цветовых байт в порядке их получения итератором
            var allColorBytes = new byte[TestImageColorBytes];
            int k = 0;

            Func<int, LsbParameters, IEnumerable<ScPointCoords>> iterator = LsbCommon.GetForRandomAccessIndex;
            foreach (var colorByteIndexes in iterator(TestImageColorBytes, parameters))
            {
                var pixel = TestImage.GetPixelValue(colorByteIndexes.X, colorByteIndexes.Y);
                allColorBytes[k] = pixel[colorByteIndexes.ChannelId];
                k++;
            }

            // Сверяем избранные значения для проверки последовательности итератора (горизонтальный, поканально)
            // Индекс для красного цветового байта: (строка - 1) * 20 + (столбец - 1)
            // Индекс для зелёного цветового байта: индекс для красного + 200
            // Индекс для синего цветового байта: индекс для красного + 400
            // Определение координат по ОЛИ:
            // канал = ОЛИ / 200
            // строка = (ОЛИ - канал * 200) / 20 + 1 (индекс строки без +1);
            // столбец = (ОЛИ - канал * 200) - (строка - 1) * 20 + 1 (индекс столбца без +1).
            // Пример: 133 - это Green(index: 1), 3 строка, 5 столбец
            var expectedValues = new int[] { 0, 255, 0, 255, 255, 255, 0, 0, 0, 0, 0, 255, 255, 0, 255 };

            // Преобразование: общий линейный индекс (ОЛИ) --> линейный индекс (ЛИ):
            // Чересканально: ОЛИ / 3
            // *Поканально: ОЛИ - (ОЛИ / (кол-во пикселей)) * (кол-во пикселей)

            for (int i = 0; i < expectedValues.Length; i++)
                Assert.AreEqual(allColorBytes[i], expectedValues[i],
                    $"Для цветового байта под индексом {i} не получен ожидаемый результат");
        }
    }
}
