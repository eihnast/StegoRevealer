namespace StegoRevealer.StegoCore.ImageHandlerLib
{
    // Класс, представляющий структуру хранения массива значений цветовых каналов пикселей
    public class ImageOneChannelArray
    {
        private readonly ScImage? _img = null;

        public int Height { get; set; }
        public int Width { get; set; }

        public ImgChannel Channel { get; }


        // Проверка, что структура пуста
        public bool IsEmpty
        {
            get 
            {
                return Height <= 0 || Width <= 0;
            }
        }

        // Конструктор
        public ImageOneChannelArray(ScImage img, ImgChannel channel)
        {
            _img = img;
            Channel = channel;

            Height = img.Height;
            Width = img.Width;
        }

        // Методы вывода значений массива пикселей
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

        // Get-еры и Set-еры
        public byte this[int y, int x]
        {
            get { return Get(y, x); }
            set { Set(y, x, value); }
        }

        public byte Get(int y, int x)
        {
            return _img?[y, x][(int)Channel] ?? 0;
        }

        public void Set(int y, int x, byte value)
        {
            if (_img is not null)
            {
                var pixel = _img[y, x];
                pixel[(int)Channel] = value;
            }
        }
    }
}
