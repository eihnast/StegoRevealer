using StegoRevealer.StegoCore.Logger;

namespace StegoRevealer.StegoCore.StegoMethods.KochZhao;

public class KochZhaoExtractResult : LoggedResult, IExtractResult
{
    /// <summary>
    /// Извлечённая информация
    /// </summary>
    public string? ResultData { get; set; } = null;

    /// <summary>
    /// Возвращает извлечённую информацию
    /// </summary>
    public string? GetResultData() => ResultData;


    /// <inheritdoc/>
    public LoggedResult AsLog()
    {
        return this;
    }
}
