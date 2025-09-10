namespace StegoRevealer.UI.Lib.Entities;

/// <summary>
/// Данные результатов встраивания, передаваемые во View и для вывода
/// </summary>
public class HidingResultsDto
{
    public string? NewFilePath { get; set; } = string.Empty;

    public long ElapsedTime { get; set; } = 0;
}
