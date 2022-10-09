using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis
{
    public class ChiSquareParameters
    {
        public ImageHandler Image { get; set; }  // Изображение

        public bool Visualize { get; set; } = false;  // Визуализировать подозрительную область

        public bool IsVerticalTraverse { get; set; } = false;  // Вертикальный обход по массиву пикселей
        // Горизонтальный обход: сначала слева направо, потом сверху вниз
        // Вертикальный обход: сначала сверху вниз, потом слева направо

        // Описывает варинт формирования массива Carr - ColorsArray
        // false: будут отдельно считаться количество интенсивности цветов в каждом канале
        // true: будет считаться общее количество интенсивности цветов без учёта канала
        public bool UseUnitedCnum { get; set; } = true;

        // Учитывать ли подсчёт цветов предыдущих блоков при оценке текущего
        public bool UsePreviousCnums { get; set; } = true;

        // Исключать ли из анализа пары, где ожидаемая величина (частота цвета) (соответсвенно, и наблюдаемая) = 0
        public bool ExcludeZeroPairs { get; set; } = true;

        // Объединять ли низкочастотные категории (массивов ожидаемых и наблюдаемых частот цветов) вместе
        public bool UseUnifiedCathegories { get; set; } = true;

        // Категори с частотой ниже либо равной этой будут объединены
        public int UnifyingCathegoriesThreshold { get; set; } = 4;  


        public double Threshold { get; set; } = 0.95;  // Порог значения p-value
        // Если p-value выше порога, фиксируется обнаружение встраивания


        public UniqueList<ImgChannel> Channels { get; }  // Анализируемые каналы
            = new UniqueList<ImgChannel> { ImgChannel.Red, ImgChannel.Green, ImgChannel.Blue };


        public int BlockWidth { get; set; }  // Ширина анализируемого блока
        public int BlockHeight { get; set; } = 1;  // Высота анализируемого блока


        public ChiSquareParameters(ImageHandler image)
        {
            Image = image;
            BlockWidth = Image.ImgArray.Width;  // По умолчанию анализ ведётся по строкам
        }

    }
}
