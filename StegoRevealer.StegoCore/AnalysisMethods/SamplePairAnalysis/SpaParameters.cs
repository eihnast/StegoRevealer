using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.CommonLib.ScTypes;

namespace StegoRevealer.StegoCore.AnalysisMethods.SamplePairAnalysis;

/// <summary>
/// Параметры метода стегоанализа SPA
/// </summary>
public class SpaParameters
{
    /// <summary>
    /// Изображение
    /// </summary>
    public ImageHandler Image { get; set; }

    /// <summary>
    /// Версия метода
    /// </summary>
    public SpaVersion MethodVersion { get; set; } = SpaVersion.Original;

    /// <summary>
    /// Направление анализа пар пикселей (если не включён UseDoubleDirection)
    /// </summary>
    public PairDirection Direction { get; set; } = PairDirection.Horizontal;

    /// <summary>
    /// Выполнять двухпроходный алгоритм (с горизонтальным и вертикальным направлением анализа)<br/>
    /// Игнрирует параметр Direction
    /// </summary>
    public bool UseDoubleDirection { get; set; } = true;

    /// <summary>
    /// Анализируемые каналы
    /// </summary>
    public UniqueList<ImgChannel> Channels { get; }
        = new UniqueList<ImgChannel> { ImgChannel.Red, ImgChannel.Green, ImgChannel.Blue };


    public SpaParameters(ImageHandler image)
    {
        Image = image;
    }
}
