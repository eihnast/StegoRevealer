using System.Collections;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.ImageHandlerLib.Blocks;
using StegoRevealer.StegoCore.ScMath;

namespace StegoRevealer.StegoCore.StegoMethods.KochZhao
{
    /// <summary>
    /// Извлечение информации, скрытой по методу Коха-Жао
    /// </summary>
    public class KochZhaoExtractor : IExtractor
    {
        /// <summary>
        /// Параметры метода Коха-Жао
        /// </summary>
        public KochZhaoParameters Params { get; set; }


        public KochZhaoExtractor(ImageHandler img)
        {
            Params = new KochZhaoParameters(img);
            Params.StegoOperation = StegoOperationType.Extracting;
        }

        public KochZhaoExtractor(KochZhaoParameters kzParams)
        {
            Params = kzParams;
            Params.StegoOperation = StegoOperationType.Extracting;
        }

        public KochZhaoExtractor(ImageHandler img, int? seed = null, TraverseType traverseType = TraverseType.Horizontal, int threshold = 120)
            : this(img)
        {
            Params.Seed = seed;
            Params.TraverseType = traverseType;
            Params.Threshold = threshold;
        }


        /// <inheritdoc/>
        public IExtractResult Extract()
        {
            return Extract(Params);
        }

        /// <inheritdoc/>
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
            Func<ImageBlocks, BlocksTraverseOptions, int?, IEnumerable<byte[,]>> iterator
                = isRandomHiding ? BlocksTraverseHelper.GetForRandomAccessBlock : BlocksTraverseHelper.GetForLinearAccessBlock;

            int brokenBitsNum = 0;

            // Осуществление извлечения
            result.Log("Запущен цикл извлечения");
            var traversalOptions = new BlocksTraverseOptions(kzParams);
            foreach (var block in iterator(kzParams.ImgBlocks, traversalOptions, usedBlocksNum))
            {
                var dctBlock = FrequencyViewTools.DctBlock(block, Params.GetBlockSize());
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

        /// <summary>
        /// Извлекает бит информации из блока ДКП
        /// </summary>
        /// <param name="dctBlock">Блок ДКП</param>
        private bool? ExtractBitFromDctBlock(double[,] dctBlock)
        {
            var coefValues = FrequencyViewTools.GetBlockCoeffs(dctBlock, Params.HidingCoeffs);  // Значения коэффициентов
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
