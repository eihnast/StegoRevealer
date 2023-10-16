using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevealer.StegoCore.AnalysisMethods.RsMethod
{
    /// <summary>
    /// Стегоанализатор по методу Regular-Singular
    /// </summary>
    public class RsAnalyser
    {
        private const string MethodName = "RS (Regular-Singular) analysis";

        /// <summary>
        /// Параметры метода
        /// </summary>
        public RsParameters Params { get; set; }

        /// <summary>
        /// Внутренний метод-прослойка для записи в лог
        /// </summary>
        private Action<string>? _writeToLog = null;


        public RsAnalyser(ImageHandler image)
        {
            Params = new RsParameters(image);
        }

        public RsAnalyser(RsParameters parameters)
        {
            Params = parameters;
        }


        /// <summary>
        /// Запуск стегоанализа
        /// </summary>
        /// <param name="verboseLog">Вести подробный лог</param>
        public RsResult Analyse(bool verboseLog = false)
        {
            var result = new RsResult();
            result.Log($"Выполняется стегоанализ методом {MethodName} для файла изображения {Params.Image.ImgName}");
            _writeToLog = result.Log;

            double pValuesSum = 0.0;  // Сумма P-значений по всем каналам (сумма относительных заполненностей, рассчитанных для каждого канала отдельно)
            foreach (var channel in Params.Channels)
            {
                var unturnedCalcTask = new Task<RsGroupsCalcResult>(() => AnalyseInOneChannel(channel, invertedImage: false));
                var invertedCalsTask = new Task<RsGroupsCalcResult>(() => AnalyseInOneChannel(channel, invertedImage: true));

                unturnedCalcTask.Start();
                invertedCalsTask.Start();

                unturnedCalcTask.Wait();
                invertedCalsTask.Wait();

                var unturnedValues = unturnedCalcTask.Result;
                result.Log($"Analysis for {channel} channel in original image completed. Regulars num = {unturnedValues.Regulars}, Singulars num = {unturnedValues.Singulars}. " +
                    $"Regulars with inverted mask num = {unturnedValues.RegularsWithInvertedMask}, Singulars with inverted mask num = {unturnedValues.SingularsWithInvertedMask}");

                var invertedValues = invertedCalsTask.Result;
                result.Log($"Analysis for {channel} channel in inverted image completed. Regulars num = {invertedValues.Regulars}, Singulars num = {invertedValues.Singulars}. " +
                    $"Regulars with inverted mask num = {invertedValues.RegularsWithInvertedMask}, Singulars with inverted mask num = {invertedValues.SingularsWithInvertedMask}");

                var pValue = CalculatePValue(unturnedValues, invertedValues);
                pValuesSum += pValue;
                result.Log($"For {channel} channel calculated results: pValue = {pValue}");
            }

            result.MessageRelativeVolume = pValuesSum / Params.Channels.Count;
            result.Log($"MessageRelativeVolume for all image = {result.MessageRelativeVolume}");

            result.Log($"Стегоанализ методом {MethodName} завершён");
            return result;
        }

        /// <summary>
        /// Метод выполнения одной итерации анализа для выбранного цветового канала
        /// </summary>
        /// <param name="channel">Цветовой канал</param>
        /// <param name="invertedImage">Вести ли подсчёт в изображении с "инвертированными" НЗБ</param>
        /// <returns>Количество каждой из групп</returns>
        private RsGroupsCalcResult AnalyseInOneChannel(ImgChannel channel, bool invertedImage = false)
        {
            var result = new RsGroupsCalcResult();
            var channelArray = invertedImage ? Params.Image.Inverted.ChannelsArray.GetChannelArray(channel) : Params.Image.ChannelsArray.GetChannelArray(channel);
            if (channelArray is null)
                throw new Exception($"Error while getting OneChannelArray for channel '{channel}'");

            var groups = SplitIntoGroupsInChannelArray(channelArray);
            var flippingFuncs = GetFlippingFuncsByMask(Params.FlippingMask);
            var flippingFuncsWithInvertedMask = GetFlippingFuncsByMask(RsHelper.InvertMask(Params.FlippingMask));

            foreach (var group in groups)
            {
                var regularityResult = CalculateRegularityResults(group, flippingFuncs);
                var groupType = RsHelper.DefineGroupType(regularityResult);
                switch (groupType)
                {
                    case RsGroupType.Singular:
                        result.Singulars++;
                        break;
                    case RsGroupType.Regular:
                        result.Regulars++;
                        break;
                }

                var regularityWithInvertedMaskResult = CalculateRegularityResults(group, flippingFuncsWithInvertedMask);
                var groupTypeWithInvertedMask = RsHelper.DefineGroupType(regularityWithInvertedMaskResult);
                switch (groupTypeWithInvertedMask)
                {
                    case RsGroupType.Singular:
                        result.SingularsWithInvertedMask++;
                        break;
                    case RsGroupType.Regular:
                        result.RegularsWithInvertedMask++;
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Метод расчёта оценки заполненности контейнера на основе расчётов по RS-диаграмме
        /// </summary>
        /// <param name="unturnedValues">Значения в "обычном" изображении</param>
        /// <param name="invertedValues">Значения в изображении с "инвертированными" НЗБ</param>
        /// <returns></returns>
        private double CalculatePValue(RsGroupsCalcResult unturnedValues, RsGroupsCalcResult invertedValues)
        {
            // Mathematical code
            double d0 = unturnedValues.Regulars - unturnedValues.Singulars;
            double d0i = unturnedValues.RegularsWithInvertedMask - unturnedValues.SingularsWithInvertedMask;
            double d1 = invertedValues.Regulars - invertedValues.Singulars;
            double d1i = invertedValues.RegularsWithInvertedMask - invertedValues.SingularsWithInvertedMask;

            double a = (d1 + d0) * 2;
            double b = d0i - d1i - d1 - 3 * d0;
            double c = d0 - d0i;

            double D = Math.Pow(b, 2) - 4 * a * c;

            double x1 = 0, x2 = 0, minX = 0;
            if (D == 0.0)
                x1 = x2 = minX = -(b / 2 * a);
            if (D > 0.0)
            {
                x1 = (-b + Math.Sqrt(D)) / (2 * a);
                x2 = (-b - Math.Sqrt(D)) / (2 * a);
                if (Math.Abs(x1) < Math.Abs(x2))
                    minX = x1;
                else
                    minX = x2;
            }

            double p = minX / (minX - 0.5);
            return Math.Max(p, 0.0);
        }


        /// <summary>
        /// Метод формирования массива групп (разбиения изображения на группы)
        /// </summary>
        /// <param name="channelArray">Ссылка на массив пикселей изображения в одном канале</param>
        private List<int[]> SplitIntoGroupsInChannelArray(ImageOneChannelArray channelArray)
        {
            var groups = new List<int[]>();
            for (int y = 0; y < channelArray.Height; y++)
            {
                for (int xGroup = 0; xGroup < channelArray.Width / Params.PixelsGroupLength; xGroup++)
                {
                    var group = new List<int>();
                    for (int i = 0; i < Params.PixelsGroupLength; i++)
                        group.Add(channelArray[y, xGroup * Params.PixelsGroupLength + i]);
                    groups.Add(group.ToArray());
                }

                // "Лишние" пиксели, которые не попадают в последовательную выборку групп, в оригинале учитывались
                // и собирались в дополнительные "остаточные" группы здесь (в единый массив excess, откуда по 
                // достижению длины PixelsGroupLength сразу же выгружались в основной.
            }

            return groups;
        }

        /// <summary>
        /// Метод формирования списка функций флиппинга для применения их по маске к группе пикселей
        /// </summary>
        /// <param name="mask">Маска</param>
        /// <returns>Список функций, которые должны быть применены к значениям группы</returns>
        private Func<int, int>[] GetFlippingFuncsByMask(int[] mask)
        {
            var funcs = new List<Func<int, int>>();
            foreach (int maskValue in mask)
            {
                switch (maskValue)
                {
                    case 1:
                        funcs.Add(Params.FlipDirect);
                        break;
                    case 0:
                        funcs.Add(Params.FlipNone);
                        break;
                    case -1:
                        funcs.Add(Params.FlipBack);
                        break;
                    default:
                        throw new ArgumentException("Mask value must be -1, 0 or 1");
                }
            }

            return funcs.ToArray();
        }

        /// <summary>
        /// Расчёт значений регулярности для группы
        /// </summary>
        /// <param name="group">Группа</param>
        /// <param name="flippingFuncs">Массив функций для применения к значениям группы по маске</param>
        /// <returns>Значения регулярности до и после флиппинга</returns>
        private (int beforeFlippingResult, int afterFlippingResult) CalculateRegularityResults(int[] group, Func<int, int>[] flippingFuncs)
        {
            var flippedGroup = RsHelper.ApplyFlipping(group, flippingFuncs);  // Расчёт "перевёрнутой" группы
            int beforeFlippingResult = Params.RegularityFunction(group);
            int afterFlippingResult = Params.RegularityFunction(flippedGroup);
            return (beforeFlippingResult, afterFlippingResult);
        }

    }
}
