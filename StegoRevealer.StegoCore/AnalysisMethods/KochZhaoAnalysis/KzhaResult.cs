using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.Logger;

namespace StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;

/// <summary>
/// Результаты стегоанализа метода Коха-Жао
/// </summary>
public class KzhaResult : LoggedResult, ILoggedAnalysisResult
{
    /// <inheritdoc/>
    public LoggedResult AsLog()
    {
        return this;
    }


    /// <summary>
    /// Найден ли подозрительный интервал
    /// </summary>
    public bool SuspiciousIntervalIsFound { get; set; } = false;
    
    /// <summary>
    /// Вычисленный порог скрытия
    /// </summary>
    public double Threshold { get; set; }

    /// <summary>
    /// Коэффициенты, в которых найдено подозрение на скрытую информацию
    /// </summary>
    public ScIndexPair Coefficients { get; set; }

    /// <summary>
    /// Битовый размер сообщения
    /// </summary>
    public int MessageBitsVolume { get; set; }

    /// <summary>
    /// Извлечённая автоматически информация
    /// </summary>
    public string? ExtractedData { get; set; } = null;

    /// <summary>
    /// Граница (индексы блоков) подозрительного интервала
    /// </summary>
    public (int leftInd, int rightInd)? SuspiciousInterval { get; set; }

    /// <summary>
    /// Время, затраченное на анализ
    /// </summary>
    public long ElapsedTime { get; set; }


    public KzhaResult() { }
}
