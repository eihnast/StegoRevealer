using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis
{
    /// <summary>
    /// Параметры метода стегоанализа по критерию Хи-квадрат
    /// </summary>
    public class ChiSquareParameters
    {
        /// <summary>
        /// Изображение
        /// </summary>
        public ImageHandler Image { get; set; }

        /// <summary>
        /// Визуализировать подозрительную область
        /// </summary>
        public bool Visualize { get; set; } = false;

        /// <summary>
        /// Вертикальный обход по массиву пикселей<br/>
        /// Горизонтальный обход: сначала слева направо, потом сверху вниз<br/>
        /// Вертикальный обход: сначала сверху вниз, потом слева направо
        /// </summary>
        public bool IsVerticalTraverse { get; set; } = false;

        /// <summary>
        /// Описывает варинт формирования массива Carr - ColorsArray<br/>
        /// false: будут отдельно считаться количество интенсивности цветов в каждом канале<br/>
        /// true: будет считаться общее количество интенсивности цветов без учёта канала
        /// </summary>
        public bool UseUnitedCnum { get; set; } = true;

        /// <summary>
        /// Учитывать ли подсчёт цветов предыдущих блоков при оценке текущего
        /// </summary>
        public bool UsePreviousCnums { get; set; } = true;

        /// <summary>
        /// Исключать ли из анализа пары, где ожидаемая величина (частота цвета) (соответсвенно, и наблюдаемая) = 0
        /// </summary>
        public bool ExcludeZeroPairs { get; set; } = true;

        /// <summary>
        /// Объединять ли низкочастотные категории (массивов ожидаемых и наблюдаемых частот цветов) вместе
        /// </summary>
        public bool UseUnifiedCathegories { get; set; } = true;

        /// <summary>
        /// Категори с частотой ниже либо равной этой будут объединены
        /// </summary>
        public int UnifyingCathegoriesThreshold { get; set; } = 4;

        /// <summary>
        /// Порог значения p-value<br/>
        /// Если p-value выше порога, фиксируется обнаружение встраивания
        /// </summary>
        public double Threshold { get; set; } = 0.95;

        /// <summary>
        /// Анализируемые каналы
        /// </summary>
        public UniqueList<ImgChannel> Channels { get; }
            = new UniqueList<ImgChannel> { ImgChannel.Red, ImgChannel.Green, ImgChannel.Blue };

        /// <summary>
        /// Ширина анализируемого блока
        /// </summary>
        public int BlockWidth { get; set; }

        /// <summary>
        /// Высота анализируемого блока
        /// </summary>
        public int BlockHeight { get; set; } = 1;


        public bool UseIncreasedCnum { get; set; } = true;


        public ChiSquareParameters(ImageHandler image)
        {
            Image = image;
            BlockWidth = Image.ImgArray.Width;  // По умолчанию анализ ведётся по строкам
        }

    }
}
