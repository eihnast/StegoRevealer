using SkiaSharp;
using System.Runtime.CompilerServices;
using StegoRevealer.StegoCore.CommonLib.Exceptions;

namespace StegoRevealer.StegoCore.ImageHandlerLib;

/*
 * Класс-обёртка над объектом изображения текущей используемой библиотеки (Skia-Sharp)
 * Есть два режима загрузки изображения:
 *   - как ридер: открывается поток чтения файла на диске, и он удерживает файл открытым для манипуляций с изображением
 *   - в память: загружает всё изображение в память при помощи MemoryStream
 * Первый вариант, теоретически, быстрее, а второй предоставляет возможности разделения экземпляров одного изображения - например, клонирование
 */

/// <summary>
/// Класс изображения
/// </summary>
public class ScImage : IDisposable
{
    /// <summary>Объект изображения</summary>
    private SKBitmap? _image = null;

    private string? _path = null;

    private readonly object cloningLock = new object();
    private readonly object setPixelLock = new object();

    /// <summary>Путь к файлу</summary>
    public string? Path { get => _path; }

    // Unsafe-работа с пикселями
    private IntPtr _pixelsPtr = IntPtr.Zero;
    private int _rowBytes;
    private int _bytesPerPixel;
    private bool _unsafeHandlingAvailable;  // Доступна ли реально unsafe-работа с пикселями


    // Параметры изображения

    /// <summary>Высота</summary>
    public int Height { get; private set; } = 0;

    /// <summary>Ширина</summary>
    public int Width { get; private set; } = 0;

    /// <summary>Глубина</summary>
    public int Depth { get; private set; } = 0;

    /// <summary>Является ли изображением типа TrueColor (RGB, 8 бит)</summary>
    public bool IsTrueColor { get; } = true;

    /// <summary>Использовать ли unsafe-механику чтения матрицы пикселей</summary>
    public bool UseUnsafeIndexator { get; } = false;


    /// <summary>Возвращает SKBitmap текущего изображения</summary>
    public SKBitmap? GetBitmap() => _image;

    // Доступ по индексаторам
    public ScPixel this[int y, int x]
    {
        get
        {
            if (_isDisposed)
                throw new ObjectDisposedException("ScImage");
            if (_image is null)
                throw new IncorrectValueException("Image is null");
            ValidateIndexes(y, x);

            if (UseUnsafeIndexator && _unsafeHandlingAvailable)
                return UnsafeGet(y, x);

            var pixel = _image.GetPixel(x, y);
            return ScPixel.FromSkColor(pixel);
        }
        set
        {
            lock (setPixelLock)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException("ScImage");
                if (_image is null)
                    throw new IncorrectValueException("Image is null");
                ValidateIndexes(y, x);

                if (UseUnsafeIndexator && _unsafeHandlingAvailable)
                    UnsafeSet(y, x, value);
                else
                    _image.SetPixel(x, y, value.ToSkColor());
            }
        }
    }

    private void ValidateIndexes(int y, int x)
    {
        if (x > Width - 1)
            throw new IndexOutOfRangeException($"X index out of range: {x} > {Width - 1}");
        if (x < 0)
            throw new IndexOutOfRangeException($"X index out of range: {x} < 0");
        if (y > Height - 1)
            throw new IndexOutOfRangeException($"Y index out of range: {y} > {Height - 1}");
        if (y < 0)
            throw new IndexOutOfRangeException($"Y index out of range: {y} < 0");
    }

    private ScPixel UnsafeGet(int y, int x)
    {
        if (_image is null) 
            throw new IncorrectValueException("Image is null");

        if (!_unsafeHandlingAvailable)
        {
            var c = _image.GetPixel(x, y);
            return ScPixel.FromSkColor(c);
        }

        unsafe
        {
            byte* basePtr = (byte*)_pixelsPtr.ToPointer();
            nint offset = (nint)y * _rowBytes + (nint)x * _bytesPerPixel;
            byte* p = basePtr + offset;

            var ct = _image.Info.ColorType;
            byte r, g, b, a;

            if (ct == SKColorType.Bgra8888)
            {
                b = p[0]; 
                g = p[1]; 
                r = p[2]; 
                a = p[3];
            }
            else // Rgba8888
            {
                r = p[0]; 
                g = p[1]; 
                b = p[2]; 
                a = p[3];
            }

            if (_image.Info.AlphaType == SKAlphaType.Premul)
            {
                r = Unpremul(r, a);
                g = Unpremul(g, a);
                b = Unpremul(b, a);
            }

            return new ScPixel(r, g, b, a);
        }
    }

    private void UnsafeSet(int y, int x, ScPixel value)
    {
        if (_image is null) 
            throw new IncorrectValueException("Image is null");

        if (!_unsafeHandlingAvailable)
        {
            _image.SetPixel(x, y, value.ToSkColor());
            return;
        }

        unsafe
        {
            byte* basePtr = (byte*)_pixelsPtr.ToPointer();
            nint offset = (nint)y * _rowBytes + (nint)x * _bytesPerPixel;
            byte* p = basePtr + offset;

            var ct = _image.Info.ColorType;

            byte r = value.Red, g = value.Green, b = value.Blue;
            if (_image.Info.AlphaType == SKAlphaType.Premul)
            {
                r = Premul(r, value.Alpha);
                g = Premul(g, value.Alpha);
                b = Premul(b, value.Alpha);
            }

            if (ct == SKColorType.Bgra8888)
            {
                p[0] = b;
                p[1] = g;
                p[2] = r;
                p[3] = value.Alpha;
            }
            else // Rgba8888
            {
                p[0] = r; 
                p[1] = g; 
                p[2] = b; 
                p[3] = value.Alpha;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte Premul(byte c, byte a) => (byte)((c * a + 127) / 255);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte Unpremul(byte cPremul, byte a) => a == 0 ? (byte)0 : (byte)((cPremul * 255 + (a >> 1)) / a);


    /// <summary>Создаёт новый FileStream для чтения файла изображения</summary>
    private static FileStream CreateFileStream(string path) => File.OpenRead(path);


    /// <summary>Создаёт объект изображения</summary>
    private void ReadImage(string path)
    {
        var file = CreateFileStream(path);

        var imgStream = new SKManagedStream(file);
        _image = SKBitmap.Decode(imgStream);
        imgStream.Dispose();

        file?.Close();
        file?.Dispose();

        var info = _image.Info;
        _pixelsPtr = _image.GetPixels();
        _rowBytes = _image.RowBytes;
        _bytesPerPixel = info.BytesPerPixel;
        _unsafeHandlingAvailable = _pixelsPtr != IntPtr.Zero && _bytesPerPixel == 4 && (info.ColorType == SKColorType.Bgra8888 || info.ColorType == SKColorType.Rgba8888);
    }


    // Конструторы

    /// <summary>Приватный конструктор загрузки изображения</summary>
    /// <param name="path">Путь к файлу изображения</param>
    private ScImage(string path)
    {
        _path = path;
        ReadImage(path);
        DefineSizes();
    }

    /// <summary>Приватный конструктор создания объекта изображения из готового битмапа</summary>
    /// <param name="bitmap">Данные изображения</param>
    /// <param name="path">Путь к файлу изображения</param>
    private ScImage(SKBitmap bitmap, string? path)
    {
        _image = bitmap;
        _path = path;
        DefineSizes();
    }

    /// <summary>Загрузка изображения</summary>
    /// <param name="path">Путь к файлу изображения</param>
    public static ScImage LoadImageFile(string path)
    {
        var image = new ScImage(path);
        return image;
    }

    /// <summary>Загрузка изображения</summary>
    /// <param name="path">Путь к файлу изображения</param>
    public static ScImage Load(string path)
    {
        // Основной внешний метод загрузки изображения
        return LoadImageFile(path);
    }


    /// <summary>
    /// Клонирование изображения: загрузка ещё одной копии текущего изображения в отдельный ScImage<br/>
    /// Не клонирует изменения, внесённые в текущий экземпляр загруженного изображения!
    /// </summary>
    public ScImage Clone()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(ScImage));
        if (_image is null)
            throw new IncorrectValueException("Image is null");

        lock (cloningLock)
        {
            if (string.IsNullOrEmpty(_path))
                throw new OperationException("Error while cloning ScImage: path is null");

            if (!File.Exists(_path))
                throw new OperationException("Error while cloning ScImage: file not exists");

            var clonedImage = new ScImage(_path);

            return clonedImage;
        }
    }

    /// <summary>Клонирование изображения: полное клонирование текущей версии изображения</summary>
    public ScImage DeepClone()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(ScImage));
        if (_image is null)
            throw new IncorrectValueException("Image is null");

        lock (cloningLock)
        {
            var info = _image.Info;
            var clonedBitmap = new SKBitmap(info);

            if (!_image.CopyTo(clonedBitmap, info.ColorType))
            {
                using var srcPix = _image.PeekPixels();
                if (srcPix is null)
                    throw new OperationException("Error while deep cloning ScImage: cannot access pixels.");

                if (!srcPix.ReadPixels(info, clonedBitmap.GetPixels(), clonedBitmap.RowBytes, 0, 0))
                    throw new OperationException("Error while deep cloning ScImage: pixel copy failed.");
            }

            return new ScImage(clonedBitmap, _path);
        }
    }


    // Устанавливает линейные размеры и "глубину" (количество каналов)
    private void DefineSizes()
    {

        Height = _image?.Height ?? 0;
        Width = _image?.Width ?? 0;

        // Определение количества каналов
        if (Height > 0 && Width > 0)
            Depth = 4;  // SkiaSharp предоставляет доступ всегда к RGB и Alpha
    }


    // Закрытие потоков доступа к изображению
    private void CloseCurrentStreams()
    {
        _image?.Dispose();
        _image = null;

        _pixelsPtr = IntPtr.Zero;
        _rowBytes = 0;
        _bytesPerPixel = 0;
        _unsafeHandlingAvailable = false;
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
            CloseCurrentStreams();  // Закрытие открытых потоков
        }

        _isDisposed = true;
    }
    ~ScImage() => Dispose(false);


    // Методы сохранения

    /// <summary>Сохранение текущей версии изображения: текущим изображением становится сохранённое</summary>
    public void SaveAndLoad(string path, ImageFormat format)
    {
        if (_image is null || string.IsNullOrEmpty(path))
            return;

        Save(path, format);
        _path = path;

        CloseCurrentStreams();
        ReadImage(_path);
    }

    /// <summary>Сохранение текущей версии изображения без перехода на новое</summary>
    /// <param name="path">Полный путь к файлу изображения с расширением</param>
    /// <param name="format">Формат изображения, если не указан - такой же, что у оригинального изображения</param>
    public string? Save(string path, ImageFormat? format = null)
    {
        if (_image is null)
            return null;

        // Сохранение файла
        format ??= GetFormat();

        var dirPath = System.IO.Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dirPath) && !Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);

        var outFile = File.OpenWrite(path);
        var imgEncoded = _image.Encode(ImageFormatToSkFormat(format.Value), 100);
        imgEncoded.SaveTo(outFile);
        imgEncoded.Dispose();
        outFile.Close();
        outFile.Dispose();

        return path;
    }


    // Вспомогательные методы

    /// <summary>
    /// Получение формата, требуемого текущей библиотекой
    /// </summary>
    private static SKEncodedImageFormat ImageFormatToSkFormat(ImageFormat format)
    {
        switch (format)
        {
            case ImageFormat.Png:
                return SKEncodedImageFormat.Png;
            case ImageFormat.Jpeg:
            case ImageFormat.Jpg:
                return SKEncodedImageFormat.Jpeg;
            case ImageFormat.Bmp:
                return SKEncodedImageFormat.Bmp;
            default:
                return SKEncodedImageFormat.Png;
        }
    }

    /// <summary>
    /// Возвращает формат изображения по его расширению
    /// </summary>
    public ImageFormat GetFormat()
    {
        var ext = System.IO.Path.GetExtension(_path);
        switch (ext)
        {
            case ".png":
                return ImageFormat.Png;
            case ".jpeg":
                return ImageFormat.Jpeg;
            case ".jpg":
                return ImageFormat.Jpg;
            case ".bmp":
                return ImageFormat.Bmp;
        }

        throw new IncorrectValueException("Unknown image extension");
    }
}
