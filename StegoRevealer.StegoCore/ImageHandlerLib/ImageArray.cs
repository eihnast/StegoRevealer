namespace StegoRevealer.StegoCore.ImageHandlerLib
{
    // Класс, представляющий структуру хранения массива значений цветовых каналов пикселей
    public class ImageArray
    {
        private readonly ScImage? _img = null;

        public int Height { get; set; } = 0;
        public int Width { get; set; } = 0;
        public int Depth { get; set; } = 0;


        // Проверка, что структура пуста
        public bool IsEmpty
        {
            get 
            {
                return Depth <= 0 || Height <= 0 || Width <= 0; 
            }
        }

        // Конструктор
        public ImageArray(ScImage img)
        {
            _img = img;

            Height = img.Height;
            Width = img.Width;
            Depth = img.Depth;
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
                    string str = "(";
                    for (int k = 0; k < Depth; k++)
                        str += $"{_img[y, x][k]}, ";
                    str = str[0..-2] + ")";
                    output.Write($"{str} ");
                }
                output.WriteLine();
            }
        }

        // Get-еры и Set-еры
        public ScPixel this[int y, int x]
        {
            get { return Get(y, x); }
            set { Set(y, x, value); }
        }
        
        public byte this[int y, int x, int z]
        {
            get { return Get(y, x, z); }
            set { Set(y, x, z, value); }
        }

        public ScPixel Get(int y, int x)
        {
            if (_img is not null)
                return _img[y, x];
            else
                return new ScPixel();
        }

        public void Set(int y, int x, ScPixel value)
        {
            if (_img is null)
                return;

            for(int i = 0; i < Math.Min(value.Length, _img[y, x].Length); i++)
                Set(y, x, i, value[i]);
        }

        public byte Get(int y, int x, int z)
        {
            return _img?[y, x][z] ?? 0;
        }

        public void Set(int y, int x, int z, byte value)
        {
            if (_img is not null)
            {
                var pixel = _img[y, x];
                pixel[z] = value;
                _img[y, x] = pixel;
            }
        }
    }
}
