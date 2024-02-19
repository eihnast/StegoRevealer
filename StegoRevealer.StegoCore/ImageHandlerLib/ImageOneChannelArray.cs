using StegoRevealer.StegoCore.CommonLib;

namespace StegoRevealer.StegoCore.ImageHandlerLib;

/// <summary>
/// Массив значений цветов пикселей одного канала изображения
/// </summary>
public class ImageOneChannelArray
{
    private readonly ScImage? _img = null;  // Изображение

    /// <summary>
    /// Высота массива
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Ширина массива
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Канал текущего массива
    /// </summary>
    public ImgChannel Channel { get; }

    /// <summary>
    /// Следует ли возвращать значения цветов с инвертированными НЗБ
    /// </summary>
    internal bool LsbInvertedGetter { get; set; } = false;


    /// <summary>
    /// Является ли массив пикселей пустым
    /// </summary>
    public bool IsEmpty
    {
        get 
        {
            return Height <= 0 || Width <= 0;
        }
    }

    
    public ImageOneChannelArray(ScImage img, ImgChannel channel)
    {
        _img = img;
        Channel = channel;

        Height = img.Height;
        Width = img.Width;
    }


    /// <summary>
    /// Вывод всех значений массива пикселей
    /// </summary>
    public void Print(TextWriter output)
    {
        if (_img is null)
            return;

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                output.Write($"{_img[y, x][(int)Channel]} ");
            }
            output.WriteLine();
        }
    }


    // Доступ по индексаторам к значениям пикселей

    public byte this[int y, int x]
    {
        get { return Get(y, x); }
        set { Set(y, x, value); }
    }

    /// <summary>
    /// Возвращает пиксель по координатам в массиве пикселей
    /// </summary>
    public byte Get(int y, int x)
    {
        if (_img is null)
            return 0;

        if (LsbInvertedGetter)
        {
            var value = _img[y, x][(int)Channel];
            value = PixelsTools.InvertLsb(value, 1);
            return value;
        }

        return _img[y, x][(int)Channel];
    }

    /// <summary>
    /// Устанавливает значение пикселя по координатам в массиве пикселей
    /// </summary>
    public void Set(int y, int x, byte value)
    {
        if (_img is not null)
        {
            var pixel = _img[y, x];
            pixel[(int)Channel] = value;
        }
    }
}
