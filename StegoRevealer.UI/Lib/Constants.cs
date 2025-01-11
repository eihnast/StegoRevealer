namespace StegoRevealer.UI.Lib;

public static class Constants
{
    public static class ResultsNames
    {
        public static string HidingDesicionDetection => "Стеганографическое встраивание";
        public static string ChiSqrValue => "Оценка заполненности по методу Хи-квадрат";
        public static string RsValue => "Оценка заполненности по методу RS";
        public static string KzhaDetection => "Обнаружен подозрительный интервал при стегоанализе Коха-Жао";
        public static string KzhaBitsNum => "Количество бит скрытой информации";
        public static string KzhaIndexes => "Индексы блоков подозрительного интервала";
        public static string KzhaThreshold => "Предполагаемый порог встраивания";
        public static string KzhaCoeffs => "Коэффициенты предполагаемого встраивания";
        public static string KzhaExtractedInfo => "Извлечённая информация";
        public static string StatmLabel => "Статистические характеристики изображения";
        public static string StatmNoise => "Уровень шума";
        public static string StatmSharpness => "Уровень резкости";
        public static string StatmBlur => "Уровень размытости";
        public static string StatmContrast => "Уровень контраста";
        public static string StatmShennon => "Энтропия Шеннона";
        public static string StatmRenyi => "Энтропия Реньи (α = 2,5)";
        public static string ElapsedTime => "Затраченное время";
    }

    public static class ResultsDefaults
    {
        public static string ElapsedTimeMeasure => "мс";
        public static string NotAnalyzed => "анализ не проводился";
        public static string NoData => "нет данных";
        public static string NotFoundData => "отсутствует";
        public static string NullElapsedTime => "0";
        public static string IsHidingDecisionCannotBeCalculated => "невозможно определить";
        public static string Yes => "да";
        public static string No => "нет";
        public static string Deceted => "обнаружено";
        public static string NotDetected => "не обнаружено";
    }
}
