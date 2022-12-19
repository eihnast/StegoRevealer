using System.Threading.Channels;
using StegoRevealer.StegoCore.CommonLib;

namespace StegoRevealer.StegoCore.ImageHandlerLib
{
    /// <summary>
    /// Единый массив пикселей изображения
    /// </summary>
    public class ImageArray
    {
        private readonly ScImage? _img = null;  // Изображение

        /// <summary>
        /// Высота массива пикселей изображения
        /// </summary>
        public int Height { get; set; } = 0;

        /// <summary>
        /// Ширина массива пикселей изображения
        /// </summary>
        public int Width { get; set; } = 0;

        /// <summary>
        /// Глубина массива пикселей изображения (число каналов)
        /// </summary>
        public int Depth { get; set; } = 0;

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
                return Depth <= 0 || Height <= 0 || Width <= 0; 
            }
        }


        public ImageArray(ScImage img)
        {
            _img = img;

            Height = img.Height;
            Width = img.Width;
            Depth = img.Depth;
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
                    string str = "(";
                    for (int k = 0; k < Depth; k++)
                        str += $"{_img[y, x][k]}, ";
                    str = str[0..-2] + ")";
                    output.Write($"{str} ");
                }
                output.WriteLine();
            }
        }


        // Доступ по индексаторам к значениям пикселей

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

        /// <summary>
        /// Возвращает пиксель по координатам в массиве пикселей
        /// </summary>
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

        /// <summary>
        /// Устанавливает значение пикселя по координатам в массиве пикселей
        /// </summary>
        public void Set(int y, int x, ScPixel value)
        {
            if (_img is null)
                return;

            for(int i = 0; i < Math.Min(value.Length, _img[y, x].Length); i++)
                Set(y, x, i, value[i]);
        }

        /// <summary>
        /// Получение значения интенсивности цвета определённого координатами пикселя
        /// </summary>
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

        /// <summary>
        /// Установка значения интенсивности цвета определённого координатами пикселя
        /// </summary>
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
