using SkiaSharp;
using System.Collections.Concurrent;
using System.Reflection.Metadata.Ecma335;

namespace StegoRevealer.StegoCore.ImageHandlerLib
{
    /*
     * Класс-обёртка над объектом изображения текущей используемой библиотеки (Skia-Sharp)
     * Есть два режима загрузки изображения:
     *   - как ридер: открывается поток чтения файла на диске, и он удерживает файл открытым для манипуляций с изображением;
     *   - в память: загружает всё изображение в память при помощи MemoryStream.
     * Первый вариант, теоретически, быстрее, а второй предоставляет возможности разделения экземпляров одного изображения - например, клонирование
     */

    /// <summary>
    /// Класс изображения
    /// </summary>
    public class ScImage
    {
        /// <summary>
        /// Объект изображения
        /// </summary>
        private SKBitmap? _image = null;

        // Потоки для открытого изображения
        private FileStream? _file = null;
        private SKManagedStream? _imgStream = null;

        // Если изображение загружено в память - будет только этот поток
        private MemoryStream? _memoryStream = null;


        /// <summary>
        /// Хранилище объектов открытых изображений<br/>
        /// Key - путь, Value - объект
        /// </summary>
        private static ConcurrentDictionary<string, ScImage> _loadedImages = new();

        /// <summary>
        /// Хранилище открытых файловых потоков изображений<br/>
        /// Key - путь, Value - объект
        /// </summary>
        private static ConcurrentDictionary<string, FileStream> _fileStreams = new();


        private string? _path = null;

        /// <summary>
        /// Путь к файлу
        /// </summary>
        public string? Path { get => _path; }


        // Параметры изображения

        /// <summary>
        /// Высота
        /// </summary>
        public int Height { get; private set; } = 0;

        /// <summary>
        /// Ширина
        /// </summary>
        public int Width { get; private set; } = 0;

        /// <summary>
        /// Глубина
        /// </summary>
        public int Depth { get; private set; } = 0;

        /// <summary>
        /// Является ли изображением типа TrueColor (RGB, 8 бит)
        /// </summary>
        public bool IsTrueColor { get; } = true;

        /// <summary>
        /// Является ли изображение загруженным в память
        /// </summary>
        public bool IsInMemory { get => _memoryStream is not null; }


        /// <summary>
        /// Возвращает SKBitmap текущего изображения
        /// </summary>
        public SKBitmap? GetBitmap() => _image;


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
                    _image.SetPixel(x, y, value.ToSkColor());
            }
        }


        /// <summary>
        /// Создаёт новый FileStream для чтения файла изображения
        /// </summary>
        private static FileStream CreateFileStream(string path) => File.OpenRead(path);

        /// <summary>
        /// Создаёт SKBitmap по переданному потоку памяти
        /// </summary>
        private static SKBitmap CreateBitmapByMemoryStream(MemoryStream memoryStream) => SKBitmap.Decode(memoryStream);


        /// <summary>
        /// Создаёт объект изображения в режиме чтения с диска<br/>
        /// Т.е. файл "захватывается" потоком чтения и не загружается целиком в оперативную память
        /// </summary>
        private void CreateAsReader(string path)
        {
            _file = CreateFileStream(path);
            _imgStream = new SKManagedStream(_file);
            _image = SKBitmap.Decode(_imgStream);
        }

        /// <summary>
        /// Создаёт объект изображения, загружая его данные в оперативную память
        /// </summary>
        /// <param name="path">Путь к изображению</param>
        private void CreateInMemory(string path)
        {
            var file = CreateFileStream(path);
            CreateInMemory(file);
            file?.Close();
        }

        /// <summary>
        /// Создаёт объект изображения, загружая его данные в оперативную память<br/>
        /// Не закрывает переданный файловый поток!<br/>
        /// Поток file может быть закрыт после создания объекта вызывающей стороной
        /// </summary>
        /// <param name="file">Поток чтения файла изображения</param>
        private void CreateInMemory(FileStream file)
        {
            _memoryStream = CreateMemoryStream(file);
            _image = CreateBitmapByMemoryStream(_memoryStream);
        }

        /// <summary>
        /// Обеспечивает копирование файла изображения в оперативную память в MemoryStream<br/>
        /// Поток file может быть закрыт после создания объекта вызывающей стороной
        /// </summary>
        /// <param name="file">Поток чтения файла изображения</param>
        private static MemoryStream CreateMemoryStream(FileStream file)
        {
            var memoryStream = new MemoryStream();

            var position = file.Position;
            file.Seek(0, SeekOrigin.Begin);
            file.CopyToAsync(memoryStream).Wait();
            file.Seek(position, SeekOrigin.Begin);
            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream;
        }


        // Конструторы

        /// <summary>
        /// Приватный конструктор с выбором создания объекта изображения в качестве ридера или в памяти
        /// </summary>
        /// <param name="path">Путь к файлу изображения</param>
        /// <param name="loadToMemory">Загружать ли изображение в память</param>
        private ScImage(string path, bool loadToMemory = false)
        {
            _path = path;

            if (loadToMemory)
                CreateInMemory(path);
            else
                CreateAsReader(path);

            DefineSizes();
        }

        /// <summary>
        /// Приватный конструктор для создания объекта изображения в памяти<br/>
        /// Поток file может быть закрыт после создания объекта вызывающей стороной
        /// </summary>
        /// <param name="file">Поток чтения файла изображения</param>
        /// <param name="path">Путь к файлу изображения</param>
        private ScImage(FileStream file, string path)
        {
            _path = path;
            CreateInMemory(file);
            DefineSizes();
        }

        /// <summary>
        /// Приватный конструктор для создания объекта изображения для переданного потока памяти
        /// </summary>
        /// <param name="memoryStream">Текущий поток изображения</param>
        /// <param name="path">Путь к файлу изображения</param>
        private ScImage(MemoryStream memoryStream, string? path)
        {
            _path = path;
            _memoryStream = memoryStream;
            _image = CreateBitmapByMemoryStream(_memoryStream);
            DefineSizes();
        }

        /// <summary>
        /// Загрузка изображения в качестве ридера<br/>
        /// "Захватывает" и читает файл на диске, не загружая в оперативную память
        /// </summary>
        /// <param name="path">Путь к файлу изображения</param>
        public static ScImage LoadToReader(string path)
        {
            // При открытии как ридера учитывается, что нельзя дважды открыть на чтение файл изображения
            // при повторном открытии вернётся уже сохранённый существующий экземпляр
            if (_loadedImages.ContainsKey(path))
                return _loadedImages[path];

            var image = new ScImage(path, false);
            _loadedImages.TryAdd(path, image);
            return image;
        }

        /// <summary>
        /// Загрузка изображения целиком в оперативную память
        /// </summary>
        /// <param name="path">Путь к файлу изображения</param>
        public static ScImage LoadToMemory(string path)
        {
            var image = new ScImage(path, true);
            return image;
        }

        /// <summary>
        /// Загрузка изображения
        /// </summary>
        /// <param name="path">Путь к файлу изображения</param>
        /// <param name="loadToMemory">Загружать ли изображение в память (либо читать его с диска)</param>
        /// <returns></returns>
        public static ScImage Load(string path, bool loadToMemory = false)
        {
            if (loadToMemory)
                return LoadToMemory(path);
            return LoadToReader(path);
        }


        /// <summary>
        /// Клонирование изображения<br/>
        /// Всегда создаёт копию текущего изображения в оперативной памяти, вне зависимости от режима загрузки текущего изображения
        /// </summary>
        public ScImage Clone(bool cloneInMemoryImagesDirectly = true)
        {
            FileStream? fileStream;
            bool shouldCloseFileStream = false;
            ScImage clonedImage;

            // Прямое копирование для загруженных в память изображение подразумевает копирование данных из памяти
            if (IsInMemory && cloneInMemoryImagesDirectly)
            {
                if (_memoryStream is null)
                    throw new Exception("Error while cloning image: memory stream is null");
                var clonedMemoryStream = CloneMemoryStream(_memoryStream);
                return new ScImage(clonedMemoryStream, _path);
            }

            // Если непрямое копирование - будет либо взят файловый поток текущего изображения (если оно уже открыто как ридер),
            // либо создан новый поток чтения файла по текущему пути на время клонирования
            if (IsInMemory && !string.IsNullOrEmpty(_path))
            {
                if (_loadedImages.ContainsKey(_path))
                    fileStream = _loadedImages[_path]._file;
                else
                {
                    fileStream = CreateFileStream(_path);
                    shouldCloseFileStream = true;
                }
            }
            else  // Иначе текущее изображение открыто в режиме ридера, и у него должен быть открыт поток чтения файла
                fileStream = _file;

            if (fileStream is null)
                throw new Exception("Error while cloning ScImage: fileStream is null");

            clonedImage = new ScImage(fileStream, _path ?? string.Empty);
            // _path должен быть, но если нет - для клонированного изображения не критично, если реальный путь не запишется

            if (shouldCloseFileStream)
                fileStream.Close();

            return clonedImage;
        }

        // Клонирует MemoryStream
        private static MemoryStream CloneMemoryStream(MemoryStream memoryStream)
        {
            var clonedMemoryStream = new MemoryStream();

            var position = memoryStream.Position;
            memoryStream.Seek(0, SeekOrigin.Begin);
            memoryStream.CopyToAsync(clonedMemoryStream).Wait();
            memoryStream.Seek(position, SeekOrigin.Begin);
            clonedMemoryStream.Seek(0, SeekOrigin.Begin);

            return clonedMemoryStream;
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
            if (IsInMemory)  // Если загружено в память - должен быть только поток памяти
                _memoryStream?.Dispose();
            else  // Если загружено как ридер - актуальны все три потока
            {
                _image?.Dispose();
                _imgStream?.Dispose();
                _file?.Close();
            }
        }


        /// <summary>
        /// Деструктор
        /// </summary>
        ~ScImage()
        {
            Dispose();
        }

        // Метод выгрузки текущего объекта
        public void Dispose()
        {
            CloseCurrentStreams();  // Закрытие открытых потоков
            if (!IsInMemory && _path is not null)  // Удаление текущего изображения из списка загруженных
                _loadedImages.TryRemove(_path, out _);  // (оно должно было быть сюда добавлено, если загружено не в память)
        }


        // Методы сохранения

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
                if (IsInMemory)
                    CreateInMemory(_path);
                else
                    CreateAsReader(_path);
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

                return path;
            }

            return null;
        }


        // Вспомогательные методы

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

            throw new Exception("Unknown image extension");
        }
    }
}
