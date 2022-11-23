using System.Threading.Channels;

namespace StegoRevealer.StegoCore.ImageHandlerLib
{
    // Класс, представляющий структуру хранения массива значений цветовых каналов пикселей
    public class ImageArray
    {
        private readonly ScImage? _img = null;

        public int Height { get; set; } = 0;
        public int Width { get; set; } = 0;
        public int Depth { get; set; } = 0;

        internal bool LsbInvertedGetter { get; set; } = false;


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
            {
                if (LsbInvertedGetter)
                {
                    var pixel = _img[y, x];
                    foreach (ImgChannel channel in Enum.GetValues(typeof(ImgChannel)))
                        pixel[(int)channel] = PixelsTools.InvertLsb(pixel[(int)channel], 1);
                    return pixel;
                }
                return _img[y, x];
            }
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
            if (_img is null)
                return 0;

            if (LsbInvertedGetter)
            {
                var value = _img[y, x][z];
                value = PixelsTools.InvertLsb(value, 1);
                return value;
            }

            return _img[y, x][z];
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
