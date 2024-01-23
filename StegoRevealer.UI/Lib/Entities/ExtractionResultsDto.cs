namespace StegoRevealer.UI.Lib.Entities;

/// <summary>
/// Данные результатов извлечения, передаваемые во View и для вывода
/// </summary>
public class ExtractionResultsDto
{
    public string ExtractedMessage { get; set; } = string.Empty;

    public long ElapsedTime { get; set; } = 0;
}
