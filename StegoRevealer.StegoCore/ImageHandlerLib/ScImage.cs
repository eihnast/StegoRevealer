using SkiaSharp;

namespace StegoRevealer.StegoCore.ImageHandlerLib
{
    // Класс-обёртка над объектом изображения текущей используемой библиотеки

    /// <summary>
    /// Класс изображения
    /// </summary>
    public class ScImage
    {
        /// <summary>
        /// Объект изображения
        /// </summary>
        private SKBitmap? _image = null;

        private FileStream? _file = null;
        private SKManagedStream? _imgStream = null;
        private MemoryStream? _memoryStream = null;

        /// <summary>
        /// Хранилище объектов открытых изображений<br/>
        /// Key - путь, Value - объект
        /// </summary>
        private static Dictionary<string, ScImage> _images = new();

        /// <summary>
        /// Путь к файлу
        /// </summary>
        private string? _path = null;

        // Параметры изображения

        /// <summary>
        /// Высота
        /// </summary>
        public int Height { get; } = 0;

        /// <summary>
        /// Ширина
        /// </summary>
        public int Width { get; } = 0;

        /// <summary>
        /// Глубина
        /// </summary>
        public int Depth { get; } = 0;

        /// <summary>
        /// Является ли изображением типа TrueColor (RGB, 8 бит)
        /// </summary>
        public bool IsTrueColor { get; } = true;


        // Доступ по индексаторам
        public ScPixel this[int y, int x]
        {
            get 
            {
                if (_image is not null)
                {
                    var pixel = _image.GetPixel(x, y);
                    return ScPixel.FromSkColor(pixel);
                }
                return new ScPixel();
            }
            set
            {
                if (_image is not null)
                {
                    _image.SetPixel(x, y, value.ToSkColor());
                }
            }
        }

        /// <summary>
        /// Загрузка изображения
        /// </summary>
        private void LoadImageFile(string path)
        {
            _file = File.OpenRead(path);
            _imgStream = new SKManagedStream(_file);
            // var imgCodec = SKCodec.Create(_imgStream);
            // var imgInfo = new SKImageInfo(imgCodec.Info.Width, imgCodec.Info.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul, SKColorSpace.CreateSrgb());
            _image = SKBitmap.Decode(_imgStream);
        }

        public ScImage Clone()
        {
            if (_file is null)
                throw new Exception("Error while cloning image: image not loaded");
            
            var memory = new MemoryStream();
            var position = _file.Position;
            _file.Seek(0, SeekOrigin.Begin);
            _file.CopyToAsync(memory).Wait();
            _file.Seek(position, SeekOrigin.Begin);
            memory.Seek(0, SeekOrigin.Begin);
            var bitmap = SKBitmap.Decode(memory);

            return new ScImage(bitmap, memory, this);
        }

        // Приватный конструктор по пути к файлу
        private ScImage(string path)
        {
            _path = path;

            LoadImageFile(_path);

            // _image = Image.NewFromFile(path);
            Height = _image?.Height ?? 0;
            Width = _image?.Width ?? 0;

            // Определение количества каналов
            if (Height > 0 && Width > 0)
                Depth = 4;  // SkiaSharp предоставляет доступ всегда к RGB и Alpha
        }

        private ScImage(SKBitmap image, MemoryStream memoryStream, ScImage clonedImage)
        {
            _path = clonedImage._path;
            Height = clonedImage.Height;
            Width = clonedImage.Width;
            Depth = clonedImage.Depth;

            _image = image;
            _memoryStream = memoryStream;
        }

        // Закрытие потоков доступа к изображению
        private void CloseCurrentStreams()
        {
            if (_image is not null && _imgStream is not null && _file is not null)
            {
                _image.Dispose();
                _imgStream.Dispose();
                _file.Close();
            }

            if (_memoryStream is not null)
                _memoryStream.Dispose();
        }

        /// <summary>
        /// Метод загрузки изображения<br/>
        /// Одно и то же изображение не может быть открыто одновременно (загружено) дважды
        /// </summary>
        public static ScImage Load(string path)
        {
            if (!_images.ContainsKey(path))
            {
                var img = new ScImage(path);
                _images.Add(path, img);
                return img;
            }
            return _images[path];
        }

        /// <summary>
        /// Деструктор
        /// </summary>
        ~ScImage()
        {
            Dispose();
        }

        public void Dispose()
        {
            CloseCurrentStreams();  // Закрытие открытых потоков
            if (_path is not null)  // Удаление текущего изображения из списка загруженных
                _images.Remove(_path);
        }

        /// <summary>
        /// Сохранение текущей версии изображения: текущим изображением становится сохранённое
        /// </summary>
        public void SaveAndLoad(string path, ImageFormat format)
        {
            if (_image is not null)
            {
                var outFile = File.OpenWrite(path);
                var imgEncoded = _image.Encode(ImageFormatToSkFormat(format), 100);
                imgEncoded.SaveTo(outFile);
                outFile.Close();

                _path = path;

                CloseCurrentStreams();
                LoadImageFile(_path);
            }
        }

        /// <summary>
        /// Сохранение текущей версии изображения без перехода на новое
        /// </summary>
        public string? Save(string path, ImageFormat format)
        {
            if (_image is not null)
            {
                // Сохранение файла
                var outFile = File.OpenWrite(path);
                var imgEncoded = _image.Encode(ImageFormatToSkFormat(format), 100);
                imgEncoded.SaveTo(outFile);
                outFile.Close();

                // Сброс изменений в открытом изображении
                CloseCurrentStreams();
                if (_path is not null)
                    LoadImageFile(_path);
                else
                    throw new Exception("Cant't re-open (reset) existing image linked to this handler");

                return path;
            }

            return null;
        }

        /// <summary>
        /// Возвращает текущий путь к изображению
        /// </summary>
        public string GetPath()
        {
            return _path ?? "";
        }

        /// <summary>
        /// Получение формата, требуемого текущей библиотекой
        /// </summary>
        private SKEncodedImageFormat ImageFormatToSkFormat(ImageFormat format)
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
            var ext = Path.GetExtension(_path);
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

            throw new Exception("Unknown image extension");
        }

        /// <summary>
        /// Возвращает SKBitmap изображения для отрисовки на WPF-форме
        /// </summary>
        public SKBitmap? GetSkiaSharpImageForPainting() => _image;
    }
}
