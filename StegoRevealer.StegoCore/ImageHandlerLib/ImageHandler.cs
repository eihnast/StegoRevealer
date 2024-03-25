using StegoRevealer.StegoCore.CommonLib;

namespace StegoRevealer.StegoCore.ImageHandlerLib;

/// <summary>
/// Обработчик изображения (StegoCore-класс представления изображения)
/// </summary>
public class ImageHandler : IDisposable
{
    private ScImage _image;  // Изображение

    private string _imgPath;  // Путь к изображению

    private ChannelsArray _channelsArray;  // Массив пикселей по каналам
    private ImageArray _imgArray;  // Массив пикселей


    /// <summary>
    /// Является ли изображением формата TrueColor
    /// </summary>
    public bool IsTrueColor { get { return _image.IsTrueColor; } }

    /// <summary>
    /// Массив пикселей изображения по каналам
    /// </summary>
    public ChannelsArray ChannelsArray { get { return _channelsArray; } }

    /// <summary>
    /// Массив пикселей изображения
    /// </summary>
    public ImageArray ImgArray { get { return _imgArray; } }

    /// <summary>
    /// Путь к файлу изображения
    /// </summary>
    public string ImgPath { get { return _imgPath; } }

    /// <summary>
    /// Имя файла изображения
    /// </summary>
    public string ImgName { get { return Path.GetFileNameWithoutExtension(ImgPath); } }


    private ImageHandler? _invertedHandler = null;  // Обработчик получения пикселей с "инвертированными" НЗБ

    /// <summary>
    /// Получение "инвертированного" обработчика - возвращает пиксели с "инвертированными" НЗБ<br/>
    /// В пикселях с инвертированными НЗБ меняется значение последнего бита интенсивности цвета в каждом канале на противоположное
    /// </summary>
    public ImageHandler Inverted { get { return GetLsbInvertedVersion(); } }

    /// <summary>
    /// Ширина изображения
    /// </summary>
    public int Width { get { return _image.Width; } }

    /// <summary>
    /// Высота изображения
    /// </summary>
    public int Height { get { return _image.Height; } }

    /// <summary>
    /// Возвращает объект изображения
    /// </summary>
    public ScImage GetScImage() => _image;


    public ImageHandler(string imgPath)
    {
        _imgPath = imgPath;
        _image = ScImage.Load(_imgPath);
        _channelsArray = new ChannelsArray(_image);
        _imgArray = new ImageArray(_image);
    }

    // Конструктор поверхностного дублирования ImageHandler
    private ImageHandler(ImageHandler originalHandler) 
    {
        _imgPath = originalHandler._imgPath;
        _image = originalHandler._image;
        _channelsArray = originalHandler._channelsArray;
        _imgArray = originalHandler._imgArray;
    }

    private ImageHandler(ScImage image)
    {
        _image = image;
        _imgPath = image.Path ?? string.Empty;
        _channelsArray = new ChannelsArray(_image);
        _imgArray = new ImageArray(_image);
    }


    /// <summary>
    /// Получение значений пикселя
    /// </summary>
    public ScPixel GetPixelValue(int x, int y)
    {
        return _image[y, x];
    }

    /// <summary>
    /// Сохранение изображения
    /// </summary>
    /// <param name="fullPath">Полный путь к файлу изображения с расширением</param>
    /// <param name="format">Формат изображения, если не указан - такой же, что у оригинального изображения</param>
    public string? Save(string fullPath, ImageFormat? format = null)
    {
        return _image.Save(fullPath, format);
    }

    /// <summary>
    /// Сохранение изображения рядом с оригинальным
    /// </summary>
    /// <param name="newName">Имя файла изображения без пути и расширения</param>
    public string? SaveNear(string newName)
    {
        var directory = Path.GetDirectoryName(ImgPath) ?? "";
        var name = newName;
        var ext = Path.GetExtension(ImgPath);

        return Save(Path.Combine(directory, name + ext));
    }

    /// <summary>
    /// Метод для получения размеров изображения
    /// </summary>
    public (int, int, int) GetImgSizes()
    {
        return (_image.Width, _image.Height, _image.Depth);
    }


    // Деструктор
    private bool _isDisposed = false;
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        if (disposing)
        {
            CloseHandler();
        }

        _isDisposed = true;
    }
    ~ImageHandler() => Dispose(false);

    /// <summary>
    /// Закрывает обработчик и "отпускает" файл изображения
    /// </summary>
    public void CloseHandler() => _image.Dispose();


    /// <summary>
    /// Вывод массива пикселей
    /// </summary>
    public static void PrintImageArray(TextWriter output, ImageHandler imageHandler)
    {
        imageHandler.ImgArray.Print(output);
    }

    /// <summary>
    /// Вывод массива пикселей
    /// </summary>
    public void PrintImageArray(TextWriter output)
    {
        PrintImageArray(output, this);
    }


    // Вспомогательные методы получения каналов по цветам

    /// <summary>
    /// Массив красных цветов пикселей
    /// </summary>
    public ImageOneChannelArray? GetRed()
    {
        return _channelsArray.GetChannelArray(ImgChannel.Red);
    }

    /// <summary>
    /// Массив зелёных цветов пикселей
    /// </summary>
    public ImageOneChannelArray? GetGreen()
    {
        return _channelsArray.GetChannelArray(ImgChannel.Red);
    }

    /// <summary>
    /// Массив синих цветов пикселей
    /// </summary>
    public ImageOneChannelArray? GetBlue()
    {
        return _channelsArray.GetChannelArray(ImgChannel.Red);
    }


    // Методы изменения НЗБ
    // Если синхронизация включена, режим будет изменён на OnlyImageArray

    /// <summary>
    /// Инвертировать НЗБ во всех пикселях в выбранных каналах
    /// </summary>
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

    // Инвертирование НЗБ в пикселях в одном цветовом канале
    private void InvertLsbInOneChannel(ImgChannel channel, int lsbNum)
    {
        if (_image is null)
            return;

        int channelId = (int)channel;

        for (int y = 0; y < _imgArray.Height; y++)
        {
            for (int x = 0; x < _imgArray.Width; x++)
            {
                var pixel = _imgArray[y, x];
                pixel[channelId] = PixelsTools.InvertLsb(_imgArray[y, x][channelId], lsbNum);
            }
        }
    }

    // Получение "инвертированного" обработчика
    private ImageHandler GetLsbInvertedVersion()
    {
        if (_invertedHandler is not null)
            return _invertedHandler;

        var invertedHandler = new ImageHandler(this);
        invertedHandler._channelsArray = new ChannelsArray(_image);
        foreach (ImgChannel channel in Enum.GetValues(typeof(ImgChannel)))
        {
            var oneChannelArray = invertedHandler._channelsArray.GetChannelArray(channel);
            if (oneChannelArray is not null)
                oneChannelArray.LsbInvertedGetter = true;
        }

        invertedHandler._imgArray = new ImageArray(_image);
        invertedHandler._imgArray.LsbInvertedGetter = true;

        return invertedHandler;
    }

    public ImageHandler Clone()
    {
        var scImage = _image.Clone();
        return new ImageHandler(scImage);
    }
}
