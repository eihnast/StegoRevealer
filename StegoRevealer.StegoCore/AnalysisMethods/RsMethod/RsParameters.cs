using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevealer.StegoCore.AnalysisMethods.RsMethod;

/// <summary>
/// Параметры метода стегоанализа по критерию Хи-квадрат
/// </summary>
public class RsParameters
{
    /// <summary>
    /// Изображение
    /// </summary>
    public ImageHandler Image { get; set; }

    /// <summary>
    /// Анализируемые каналы
    /// </summary>
    public UniqueList<ImgChannel> Channels { get; }
        = new UniqueList<ImgChannel> { ImgChannel.Red, ImgChannel.Green, ImgChannel.Blue };

    /// <summary>
    /// Длина групп пикселей
    /// </summary>
    public int PixelsGroupLength { get; set; } = RsHelper.DefaultPixelsGroupLength;

    /// <summary>
    /// Маска флиппинга
    /// </summary>
    public int[] FlippingMask { get; set; } = RsHelper.DefaultFlippingMask; 

    /// <summary>
    /// Функция регулярности
    /// </summary>
    public Func<IList<int>, int> RegularityFunction { get; set; } = RsHelper.DefaultRegularityFunction;

    /// <summary>
    /// Функция прямого флиппинга
    /// </summary>
    public Func<int, int> FlipDirect { get; set; } = RsHelper.DefaultFlipDirect;

    /// <summary>
    /// Функция обратного флиппинга
    /// </summary>
    public Func<int, int> FlipBack { get; set; } = RsHelper.DefaultFlipBack;

    /// <summary>
    /// Функция нулевого флиппинга
    /// </summary>
    public Func<int, int> FlipNone { get; set; } = RsHelper.DefaultFlipNone;


    public RsParameters(ImageHandler image)
    {
        Image = image;
    }
}
