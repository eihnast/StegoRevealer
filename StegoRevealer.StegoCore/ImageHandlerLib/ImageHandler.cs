namespace StegoRevealer.StegoCore.ImageHandlerLib
{
    public class ImageHandler
    {
        private ScImage _image;

        private string _imgPath;

        private ChannelsArray _channelsArray;
        private ImageArray _imgArray;

        public bool IsTrueColor { get { return _image.IsTrueColor; } }

        public ChannelsArray ChannelsArray { get { return _channelsArray; } }
        public ImageArray ImgArray { get { return _imgArray; } }
        public string ImgPath { get { return _imgPath; } }
        public string ImgName { get { return Path.GetFileNameWithoutExtension(ImgPath); } }


        public ImageHandler(string imgPath)
        {
            _imgPath = imgPath;
            _image = ScImage.Load(_imgPath);
            _channelsArray = new ChannelsArray(_image);
            _imgArray = new ImageArray(_image);
        }


        // Получение значений пикселя
        public ScPixel GetPixelValue(int x, int y)
        {
            return _image[y, x];
        }

        // Сохранение изображения
        public string? Save(string fullPath, ImageFormat format)
        {
            return _image.Save(fullPath, format);
        }

        public string? Save(string newName)
        {
            var directory = Path.GetDirectoryName(ImgPath) ?? "";
            var name = newName;
            var ext = Path.GetExtension(ImgPath);

            return Save(Path.Combine(directory, name + ext), _image.GetFormat());
        }

        // Метод для получения размеров изображения
        public (int, int, int) GetImgSizes()
        {
            return (_image.Width, _image.Height, _image.Depth);
        }
        

        // Методы вывода значений массивов пикселей
        public static void PrintImageArray(TextWriter output, ImageHandler imageHandler)
        {
            imageHandler.ImgArray.Print(output);
        }

        public void PrintImageArray(TextWriter output)
        {
            PrintImageArray(output, this);
        }


        // Вспомогательные методы получения каналов по цветам
        public ImageOneChannelArray? GetRed()
        {
            return _channelsArray.GetChannelArray(ImgChannel.Red);
        }

        public ImageOneChannelArray? GetGreen()
        {
            return _channelsArray.GetChannelArray(ImgChannel.Red);
        }

        public ImageOneChannelArray? GetBlue()
        {
            return _channelsArray.GetChannelArray(ImgChannel.Red);
        }


        // Методы изменения НЗБ
        // Если синхронизация включена, режим будет изменён на OnlyImageArray
        public void InvertAllLsb(ImgChannel[]? channels = null, int lsb = 1)
        {
            if (channels is null)  // Инвертирование по всем каналам
            {
                foreach (ImgChannel channel in Enum.GetValues(typeof(ImgChannel)))
                    InvertLsbInOneChannel(channel, lsb);
            }
            else  // Инвертирование для указанных каналов
            {
                foreach (var channel in channels)
                    InvertLsbInOneChannel(channel, lsb);
            }
        }

        private void InvertLsbInOneChannel(ImgChannel channel, int lsb)
        {
            if (_image is null)
                return;

            int channelId = (int)channel;

            for (int y = 0; y < _imgArray.Height; y++)
            {
                for (int x = 0; x < _imgArray.Width; x++)
                {
                    var pixel = _imgArray[y, x];
                    pixel[channelId] = PixelsTools.InvertLsb(_imgArray[y, x][channelId], lsb);
                }
            }
        }
    }
}
