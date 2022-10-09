using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevealer.StegoCore.AnalysisMethods.RsMethod
{
    public class RsParameters
    {
        public ImageHandler Image { get; set; }  // Изображение

        public UniqueList<ImgChannel> Channels { get; }  // Анализируемые каналы
            = new UniqueList<ImgChannel> { ImgChannel.Red, ImgChannel.Green, ImgChannel.Blue };

        public int PixelsGroupLength { get; set; } = 4;  // Длина групп пикселей

        // public Action<>


        public RsParameters(ImageHandler image)
        {
            Image = image;
        }
    }
}
