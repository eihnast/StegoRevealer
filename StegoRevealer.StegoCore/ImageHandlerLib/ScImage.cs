using SkiaSharp;

namespace StegoRevealer.StegoCore.ImageHandlerLib
{
    // Класс-обёртка над объектом изображения текущей используемой библиотеки
    public class ScImage
    {
        // Объект изображения
        private SKBitmap? _image = null;
        private FileStream? _file = null;
        private SKManagedStream? _imgStream = null;

        // Хранилище объектов открытых изображений
        private static Dictionary<string, ScImage> _images = new();  // Key - путь, Value - объект

        // Путь к файлу
        private string? _path = null;

        // Параметры изображения
        public int Height { get; } = 0;
        public int Width { get; } = 0;
        public int Depth { get; } = 0;

        // Является ли изображение типа TrueColor (RGB, 8 бит)
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

        // Загрузка изображения
        private void LoadImageFile(string path)
        {
            _file = File.OpenRead(path);
            _imgStream = new SKManagedStream(_file);
            _image = SKBitmap.Decode(_imgStream);
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

        // Приватный конструктор по уже готовому изображению библиотеки
        //private ScImage(SKBitmap image)
        //{
        //    _image = image;
        //    Height = _image.Height;
        //    Width = _image.Width;

        //    // Определение количества каналов через размер массива первого пикселя
        //    if (Height > 0 && Width > 0)
        //        Depth = 4;
        //}

        // Закрытие потоков доступа к изображению
        private void CloseCurrentStreams()
        {
            if (_image is not null && _imgStream is not null && _file is not null)
            {
                _image.Dispose();
                _imgStream.Dispose();
                _file.Close();
            }
        }

        // Метод загрузки изображения
        // Одно и то же изображение не может быть открыто одновременно (загружено) дважды
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

        // Деструктор
        ~ScImage()
        {
            CloseCurrentStreams();  // Закрытие открытых потоков
            if (_path is not null)  // Удаление текущего изображения из списка загруженных
                _images.Remove(_path);
        }

        // Сохранение текущей версии изображения: текущим изображением становится сохранённое
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

        // Сохранение текущей версии изображения без перехода на новое
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

        // Возвращает текущий путь к изображению
        public string GetPath()
        {
            return _path ?? "";
        }

        // Получение формата, требуемого текущей библиотекой
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

        // Возвращает формат изображения по его расширению
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
    }
}
