using System.Collections;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.CommonLib.TypeExtensions;
using StegoRevealer.StegoCore.CommonLib;

namespace StegoRevealer.StegoCore.StegoMethods.Lsb
{
    public class LsbExtractor : IExtractor
    {
        public LsbParameters Params { get; set; }

        public LsbExtractor(ImageHandler img)
        {
            Params = new LsbParameters(img);
            Params.HidingOperation = false;
        }

        public LsbExtractor(ImageHandler img, 
            int? seed = null, bool interlaceChannels = true, bool verticalHiding = false, int lsbNum = 1)
            : this(img)
        {
            Params.Seed = seed;
            Params.InterlaceChannels = interlaceChannels;
            Params.VerticalHiding = verticalHiding;
            Params.LsbNum = lsbNum;
        }

        public IExtractResult Extract()
        {
            return Extract(Params);
        }

        public IExtractResult Extract(IParams parameters)
        {
            LsbExtractResult result = new();
            result.Log($"Запущен процесс извлечения из {Params.Image.ImgName}");

            LsbParameters? lsbParams = parameters as LsbParameters;
            if (lsbParams is null)  // Не удалось привести к LsbParameters
            {
                result.Error("lsbParams является null");
                return result;
            }

            List<bool> dataBitArray = new();  // Массив извлечённых данных

            // Доопределение параметров извлечения
            bool isRandomHiding = lsbParams.Seed is not null;  // Вид скрытия: последовательный или псевдослучайный
            int usedColorBytesNum = lsbParams.ToExtractColorBytesNum;  // Количество извлекаемых байт цвета
            result.Log($"Установлены параметры:\n\t" +
                $"isRandomHiding = {isRandomHiding}\n\tusedColorBytesNum = {usedColorBytesNum}");

            // Выбор типа итерации в зависимости от метода скрытия (последовательное / псевдослучайное)
            Func<int, LsbParameters, IEnumerable<ScPointCoords>> iterator
                = isRandomHiding ? LsbCommon.GetForRandomAccessIndex : LsbCommon.GetForLinearAccessIndex;

            // Осуществление извлечения
            result.Log("Запущен цикл извлечения");
            foreach (var blockCoords in iterator(usedColorBytesNum, lsbParams))
            {
                (int line, int column, int channel) = blockCoords.AsTuple();
                byte colorByte = lsbParams.Image.ImgArray[line, column, channel];
                BitArray extractedBits = ExtractBitsFromColorByte(colorByte, lsbParams.LsbNum);
                for (int i = 0; i < extractedBits.Length; i++)
                    dataBitArray.Add(extractedBits[i]);
            }
            result.Log("Завершён цикл извлечения");

            // Преобразование извлечённых бит в текст
            result.ResultData = StringBitsTools.BitArrayToString(new BitArray(dataBitArray.ToArray()));
            result.Log($"Объём извлечённой информации: {result.ResultData.Length} символов");

            result.Log("Процесс извлечения завершён");
            return result;
        }

        private BitArray ExtractBitsFromColorByte(byte colorByte, int lsbNum)
        {
            var bits = new BitArray(lsbNum);
            var colorByteAsBits = BitArrayExtensions.NewFromByte(colorByte);
            for (int i = 0; i < lsbNum; i++)
                bits[i] = colorByteAsBits[8 - lsbNum + i];
            return bits;
        }
    }
}
