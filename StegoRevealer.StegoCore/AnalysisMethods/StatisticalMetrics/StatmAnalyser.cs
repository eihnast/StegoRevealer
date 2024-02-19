using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Calculators;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Entities;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.ImageHandlerLib.Blocks;
using StegoRevealer.StegoCore.ScMath;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Channels;

namespace StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics
{
    /// <summary>
    /// Анализатор статистических метрик изображения -
    /// показателей визуального искажения
    /// </summary>
    public class StatmAnalyser
    {
        private const string MethodName = "Statistical metricks calculator";

        /// <summary>
        /// Параметры метода
        /// </summary>
        public StatmParameters Params { get; set; }

        /// <summary>
        /// Внутренний метод-прослойка для записи в лог
        /// </summary>
        private Action<string> _writeToLog = (string str) => new string(str);


        public StatmAnalyser(ImageHandler image)
        {
            Params = new StatmParameters(image);
        }

        public StatmAnalyser(StatmParameters parameters)
        {
            Params = parameters;
        }


        /// <summary>
        /// Запуск анализа
        /// </summary>
        /// <param name="verboseLog">Вести подробный лог</param>
        public StatmResult Analyse(bool verboseLog = false)
        {
            var result = new StatmResult();
            result.Log($"Выполняется подсчёт характеристик для файла изображения {Params.Image.ImgName}");
            _writeToLog = result.Log;

            var noiseCalculator = new NoiseCalculator(Params);
            var noiseLevel = noiseCalculator.CalcNoiseLevel();
            result.NoiseValue = noiseLevel;

            result.Log($"Подсчёт характеристик завершён");
            return result;
        }
    }
}
