using StegoRevealer.StegoCore.ImageHandlerLib;
using System.Collections;

namespace StegoRevealer.StegoCore.StegoMethods.KochZhao
{
    public class KochZhaoExtractor : IExtractor
    {
        public KochZhaoParameters Params { get; set; }

        public KochZhaoExtractor(ImageHandler img)
        {
            Params = new KochZhaoParameters(img);
            Params.HidingOperation = false;
        }

        public KochZhaoExtractor(ImageHandler img, int? seed = null, bool verticalHiding = false, int threshold = 120)
            : this(img)
        {
            Params.Seed = seed;
            Params.VerticalHiding = verticalHiding;
            Params.Threshold = threshold;
        }

        public IExtractResult Extract()
        {
            return Extract(Params);
        }

        public IExtractResult Extract(IParams parameters)
        {
            KochZhaoExtractResult result = new();
            result.Log($"Запущен процесс извлечения из {Params.Image.ImgName}");

            KochZhaoParameters? kzParams = parameters as KochZhaoParameters;
            if (kzParams is null)  // Не удалось привести к KochZhaoParameters
            {
                result.Error("kzParams является null");
                return result;
            }

            List<bool> dataBitArray = new();  // Массив извлечённых данных

            // Доопределение параметров извлечения
            bool isRandomHiding = kzParams.Seed is not null;  // Вид скрытия: последовательный или псевдослучайный
            int usedBlocksNum = kzParams.ToExtractBitLength;  // Количество извлекаемых бит => блоков
            result.Log($"Установлены параметры:\n\t" +
                $"isRandomHiding = {isRandomHiding}\n\tusedBlocksNum = {usedBlocksNum}");

            // Выбор типа итерации в зависимости от метода скрытия (последовательное / псевдослучайное)
            Func<KochZhaoParameters, int?, IEnumerable<byte[,]>> iterator
                = isRandomHiding ? KochZhaoCommon.GetForRandomAccessBlock : KochZhaoCommon.GetForLinearAccessBlock;

            int brokenBitsNum = 0;

            // Осуществление извлечения
            result.Log("Запущен цикл извлечения");
            // foreach (var block in iterator(kzParams, usedBlocksNum))
            foreach (var block in iterator(kzParams, usedBlocksNum))
            {
                var dctBlock = KochZhaoCommon.DctBlock(block, Params.GetBlockSize());
                bool? extractedBit = ExtractBitFromDctBlock(dctBlock);
                if (extractedBit.HasValue)
                    dataBitArray.Add(extractedBit.Value);
                else
                    brokenBitsNum++;
            }
            result.Log("Завершён цикл извлечения");

            if (brokenBitsNum > 0)
            {
                result.Log($"Блоков, из которых не удалось извлечь бит: {brokenBitsNum}");
                result.Warning("Не из всех блоков удалось успешно извлечь информацию");
            }

            // Преобразование извлечённых бит в текст
            result.ResultData = StringBitsTools.BitArrayToString(new BitArray(dataBitArray.ToArray()));
            result.Log($"Объём извлечённой информации: {result.ResultData.Length} символов");

            result.Log("Процесс извлечения завершён");
            return result;
        }

        private bool? ExtractBitFromDctBlock(double[,] dctBlock)
        {
            var coefValues = KochZhaoCommon.GetBlockCoeffs(dctBlock, Params.HidingCoeffs);  // Значения коэффициентов
            var difference = MathMethods.GetModulesDiff(coefValues);  // Разница коэффициентов

            // Извлечение бита
            bool? bit = null;  // Извлечение бита может быть неудачным
            if (difference > Params.Threshold)
                bit = false;
            else if (difference < -Params.Threshold)
                bit = true;

            return bit;
        }
    }
}
