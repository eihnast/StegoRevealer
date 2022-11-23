using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevealer.StegoCore.AnalysisMethods.RsMethod
{
    public class RsParameters
    {
        public ImageHandler Image { get; set; }  // Изображение

        public UniqueList<ImgChannel> Channels { get; }  // Анализируемые каналы
            = new UniqueList<ImgChannel> { ImgChannel.Red, ImgChannel.Green, ImgChannel.Blue };

        public int PixelsGroupLength { get; set; } = RsHelper.DefaultPixelsGroupLength;  // Длина групп пикселей

        public int[] FlippingMask { get; set; } = RsHelper.DefaultFlippingMask;  // Маска флиппинга

        // Функция регулярности
        public Func<IList<int>, int> RegularityFunction { get; set; } = RsHelper.DefaultRegularityFunction;

        // Функция прямого флиппинга
        public Func<int, int> FlipDirect { get; set; } = RsHelper.DefaultFlipDirect;

        // Функция обратного флиппинга
        public Func<int, int> FlipBack { get; set; } = RsHelper.DefaultFlipBack;

        // Функция нулевого флиппинга
        public Func<int, int> FlipNone { get; set; } = RsHelper.DefaultFlipNone;


        public RsParameters(ImageHandler image)
        {
            Image = image;
        }
    }
}
