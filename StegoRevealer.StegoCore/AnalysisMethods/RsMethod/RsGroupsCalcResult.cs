namespace StegoRevealer.StegoCore.AnalysisMethods.RsMethod;

/// <summary>
/// Результат подсчёта количества групп метода RS
/// </summary>
public class RsGroupsCalcResult
{
    /// <summary>
    /// Количество Regular групп
    /// </summary>
    public int Regulars { get; set; } = 0;

    /// <summary>
    /// Количество Singular групп
    /// </summary>
    public int Singulars { get; set; } = 0;

    /// <summary>
    /// Количество Regular групп с инвертированной маской
    /// </summary>
    public int RegularsWithInvertedMask { get; set; } = 0;

    /// <summary>
    /// Количество Singular групп с инвертированной маской
    /// </summary>
    public int SingularsWithInvertedMask { get; set; } = 0;

    /// <summary>
    /// Общее количество групп
    /// </summary>
    public int GroupsNumber { get; set; } = 0;
}
