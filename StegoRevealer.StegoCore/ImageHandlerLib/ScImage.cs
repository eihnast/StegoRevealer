using SkiaSharp;
using StegoRevealer.StegoCore.CommonLib.Exceptions;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace StegoRevealer.StegoCore.ImageHandlerLib;

/*
 * Класс-обёртка над объектом изображения текущей используемой библиотеки (Skia-Sharp)
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

    // Кэш пикселей
    private ScPixel[]? _pixelCache;
    private int[]? _filledFlags;
    private int[]? _dirtyFlags;
    private ConcurrentQueue<int>? _dirtyQueue;
    private CancellationTokenSource? _cacheCts;
    private Task? _cacheTask;
    private int _remainingToFill;

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
    public bool UseUnsafeIndexator { get; } = false;  // В текущей реализации замедляет работу, не включать!

    /// <summary>Использовать ли кэш пикселей</summary>
    public bool UsePixelsCache { get; } = true;


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

            if (UsePixelsCache && _pixelCache != null && _filledFlags != null)
            {
                int idx = y * Width + x;
                if (_filledFlags[idx] == 1)
                    return _pixelCache[idx];

                var p = (UseUnsafeIndexator && _unsafeHandlingAvailable)
                        ? UnsafeGet(y, x)
                        : ScPixel.FromSkColor(_image.GetPixel(x, y));

                _pixelCache[idx] = p;
                if (Interlocked.CompareExchange(ref _filledFlags[idx], 1, 0) == 0)
                    Interlocked.Decrement(ref _remainingToFill);
                return p;
            }

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

                if (UsePixelsCache && _pixelCache != null && _filledFlags != null && _dirtyFlags != null && _dirtyQueue != null)
                {
                    int idx = y * Width + x;

                    _pixelCache[idx] = value;

                    if (Interlocked.CompareExchange(ref _filledFlags[idx], 1, 0) == 0)
                        Interlocked.Decrement(ref _remainingToFill);

                    if (Interlocked.CompareExchange(ref _dirtyFlags[idx], 1, 0) == 0)
                        _dirtyQueue.Enqueue(idx);

                    return;
                }

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

        DefineSizes();

        if (UsePixelsCache)
        {
            int n = Width * Height;
            _pixelCache = new ScPixel[n];
            _filledFlags = new int[n];
            _dirtyFlags = new int[n];
            _dirtyQueue = new ConcurrentQueue<int>();
            _remainingToFill = n;

            _cacheCts?.Cancel();
            _cacheCts = new CancellationTokenSource();
            var token = _cacheCts.Token;

            _cacheTask = Task.Run(() =>
            {
                try
                {
                    for (int y = 0; y < Height; y++)
                    {
                        if (token.IsCancellationRequested) 
                            break;
                        for (int x = 0; x < Width; x++)
                        {
                            int idx = y * Width + x;

                            if (_filledFlags![idx] == 1)
                                continue;

                            var p = (_unsafeHandlingAvailable && UseUnsafeIndexator)
                                    ? UnsafeGet(y, x)
                                    : ScPixel.FromSkColor(_image!.GetPixel(x, y));

                            _pixelCache![idx] = p;
                            if (Interlocked.CompareExchange(ref _filledFlags[idx], 1, 0) == 0)
                            {
                                Interlocked.Decrement(ref _remainingToFill);
                                if (_remainingToFill == 0) 
                                    return;
                            }
                        }
                    }
                }
                catch { }
            }, token);
        }
        else
        {
            _pixelCache = null;
            _filledFlags = null;
            _dirtyFlags = null;
            _dirtyQueue = null;
            _remainingToFill = 0;
        }
    }


    // Конструторы

    /// <summary>Приватный конструктор загрузки изображения</summary>
    /// <param name="path">Путь к файлу изображения</param>
    private ScImage(string path)
    {
        _path = path;
        ReadImage(path);
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

            var clonedImage = new ScImage(clonedBitmap, _path);

            if (UsePixelsCache && _pixelCache != null && _filledFlags != null && _dirtyFlags != null)
            {
                int n = Width * Height;
                clonedImage._pixelCache = new ScPixel[n];
                clonedImage._filledFlags = new int[n];
                clonedImage._dirtyFlags = new int[n];
                clonedImage._dirtyQueue = new ConcurrentQueue<int>();

                Array.Copy(_pixelCache, clonedImage._pixelCache, n);
                Array.Copy(_filledFlags, clonedImage._filledFlags, n);
                Array.Copy(_dirtyFlags, clonedImage._dirtyFlags, n);

                for (int i = 0; i < n; i++)
                    if (clonedImage._dirtyFlags[i] == 1)
                        clonedImage._dirtyQueue.Enqueue(i);

                clonedImage._remainingToFill = 0;
                for (int i = 0; i < n; i++)
                    if (clonedImage._filledFlags[i] == 0) clonedImage._remainingToFill++;
            }

            return clonedImage;
        }
    }

    /// <summary>Заливка кэша пикселей в Bitmap</summary>
    private void FlushDirtyToBitmap()
    {
        if (!UsePixelsCache || _pixelCache == null || _dirtyQueue == null || _dirtyFlags == null || _image is null)
            return;

        unsafe
        {
            byte* basePtr = (byte*)_pixelsPtr.ToPointer();
            bool premul = (_image.Info.AlphaType == SKAlphaType.Premul);
            bool bgra = (_image.Info.ColorType == SKColorType.Bgra8888);
            int bpp = _bytesPerPixel;
            int stride = _rowBytes;

            while (_dirtyQueue.TryDequeue(out int idx))
            {
                if (_dirtyFlags[idx] == 0) continue; // вдруг уже сбросили

                int y = idx / Width;
                int x = idx % Width;
                var px = _pixelCache[idx];

                byte r = px.Red, g = px.Green, b = px.Blue, a = px.Alpha;
                if (premul) { r = Premul(r, a); g = Premul(g, a); b = Premul(b, a); }

                byte* row = basePtr + (nint)y * stride;
                byte* p = row + (nint)x * bpp;
                if (bgra) { p[0] = b; p[1] = g; p[2] = r; p[3] = a; }
                else { p[0] = r; p[1] = g; p[2] = b; p[3] = a; }

                _dirtyFlags[idx] = 0;
            }
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

        // Unsafe-механика
        _pixelsPtr = IntPtr.Zero;
        _rowBytes = 0;
        _bytesPerPixel = 0;
        _unsafeHandlingAvailable = false;

        // Кэш пикселей
        _cacheCts?.Cancel();
        try { _cacheTask?.Wait(50); } catch { }
        _cacheTask = null;
        _cacheCts?.Dispose();
        _cacheCts = null;

        _pixelCache = null;
        _filledFlags = null;
        _dirtyFlags = null;
        _dirtyQueue = null;
        _remainingToFill = 0;
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

        FlushDirtyToBitmap();

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
